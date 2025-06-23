using Microsoft.AspNetCore.Mvc;
using CV_AI.Services;
using Microsoft.AspNetCore.Authorization;
using CV_AI.Data;
using CV_AI.Models.CV;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using DocumentFormat.OpenXml.Packaging;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static System.Math;

namespace CV_AI.Controllers
{
    public class CVController : Controller
    {
        private readonly IGeminiService _geminiService;

        public CVController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EvaluateCV(IFormFile cvFile, string jobDescription)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json");

            if (cvFile == null || cvFile.Length == 0)
            {
                if (isAjax)
                    return Content("<div class='alert alert-error'>Vui lòng chọn file CV.</div>", "text/html");
                TempData["Error"] = "Vui lòng chọn file CV.";
                return RedirectToAction("Index");
            }

            var allowedExtensions = new[] { ".txt", ".doc", ".docx", ".pdf" };
            var fileExtension = System.IO.Path.GetExtension(cvFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                if (isAjax)
                    return Content("<div class='alert alert-error'>Chỉ chấp nhận file .txt, .doc, .docx, .pdf.</div>", "text/html");
                TempData["Error"] = "Chỉ chấp nhận file .txt, .doc, .docx, .pdf.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(jobDescription))
            {
                if (isAjax)
                    return Content("<div class='alert alert-error'>Vui lòng nhập mô tả công việc.</div>", "text/html");
                TempData["Error"] = "Vui lòng nhập mô tả công việc.";
                return RedirectToAction("Index");
            }

            try
            {
                string cvText;
                var sb = new StringBuilder();
                using (var stream = cvFile.OpenReadStream())
                {
                    if (fileExtension == ".pdf")
                    {
                        using (var reader = new PdfReader(stream))
                        {
                            for (int i = 1; i <= reader.NumberOfPages; i++)
                            {
                                sb.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                            }
                        }
                    }
                    else if (fileExtension == ".docx")
                    {
                        using (var wordDocument = WordprocessingDocument.Open(stream, false))
                        {
                            sb.Append(wordDocument.MainDocumentPart.Document.Body.InnerText);
                        }
                    }
                    else
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            sb.Append(await reader.ReadToEndAsync());
                        }
                    }
                }
                cvText = sb.ToString();

                var analysisResult = await _geminiService.AnalyzeCVAsync(cvText, jobDescription);
                
                // Try to deserialize to new model format
                EvaluationResultViewModel? evaluationModel = null;
                try
                {
                    var json = JsonSerializer.Serialize(analysisResult);
                    evaluationModel = JsonSerializer.Deserialize<EvaluationResultViewModel>(json);
                }
                catch
                {
                    // If deserialization fails, use the old format
                }

                if (isAjax)
                {
                    if (evaluationModel != null)
                    {
                        // Return JSON for AJAX requests
                        return Json(new { success = true, data = evaluationModel });
                    }
                    else
                    {
                        var htmlResult = FormatAnalysisResult(analysisResult);
                        if (string.IsNullOrWhiteSpace(htmlResult))
                            return Content("<div class='alert alert-warning'>Không có kết quả phân tích.</div>", "text/html");
                        return Content(htmlResult, "text/html");
                    }
                }

                // For non-AJAX requests, store the result in TempData
                if (evaluationModel != null)
                {
                    TempData["EvaluationModel"] = JsonSerializer.Serialize(evaluationModel);
                }
                else
                {
                    var htmlResult = FormatAnalysisResult(analysisResult);
                    TempData["EvaluationResult"] = htmlResult;
                }
            }
            catch (Exception ex)
            {
                if (isAjax)
                {
                    var errorHtml = $"<div class='alert alert-error'>Lỗi khi phân tích CV: {System.Net.WebUtility.HtmlEncode(ex.Message)}</div>";
                    return Content(errorHtml, "text/html");
                }
                TempData["Error"] = $"Lỗi khi phân tích CV: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        private string FormatAnalysisResult(object analysisResult)
        {
            if (analysisResult == null) return "";

            var json = JsonSerializer.Serialize(analysisResult);
            var data = JsonDocument.Parse(json).RootElement;
            var sb = new StringBuilder();

            // Overall Score
            if (data.TryGetProperty("overallScore", out var overallScore))
            {
                sb.Append($"<h3>Điểm tổng thể: {overallScore.GetInt32()}/100</h3>");
            }
            
            if (data.TryGetProperty("matchPercentage", out var matchPercentage))
            {
                sb.Append($"<h4>Mức độ phù hợp với công việc: {matchPercentage.GetInt32()}%</h4>");
            }

            // Criteria Comparison Table
            if (data.TryGetProperty("criteriaComparison", out var criteriaComparison))
            {
                sb.Append("<div class='criteria-comparison'>");
                sb.Append("<h4>🔍 So sánh tiêu chí (CV vs JD)</h4>");
                sb.Append("<div class='table-responsive'>");
                sb.Append("<table class='comparison-table'>");
                sb.Append("<thead><tr>");
                sb.Append("<th>Tiêu chí</th>");
                sb.Append("<th>Yêu cầu JD</th>");
                sb.Append("<th>Hồ sơ ứng viên</th>");
                sb.Append("<th>Trạng thái</th>");
                sb.Append("<th>Điểm</th>");
                sb.Append("</tr></thead><tbody>");
                
                foreach (var criteria in criteriaComparison.EnumerateArray())
                {
                    var criteriaName = criteria.TryGetProperty("criteria", out var c) ? c.GetString() : "";
                    var jobReq = criteria.TryGetProperty("jobRequirement", out var jr) ? jr.GetString() : "";
                    var candidateProf = criteria.TryGetProperty("candidateProfile", out var cp) ? cp.GetString() : "";
                    var status = criteria.TryGetProperty("status", out var s) ? s.GetString() : "";
                    var score = criteria.TryGetProperty("score", out var sc) ? sc.GetInt32() : 0;
                    
                    string statusClass = status switch
                    {
                        "Đạt" => "status-success",
                        "Chưa đạt" => "status-warning",
                        "Ưu tiên" => "status-info",
                        _ => "status-default"
                    };
                    
                    sb.Append("<tr>");
                    sb.Append($"<td><strong>{criteriaName}</strong></td>");
                    sb.Append($"<td>{jobReq}</td>");
                    sb.Append($"<td>{candidateProf}</td>");
                    sb.Append($"<td><span class='status-badge {statusClass}'>{status}</span></td>");
                    sb.Append($"<td><span class='score-badge'>{score}/100</span></td>");
                    sb.Append("</tr>");
                }
                sb.Append("</tbody></table></div></div>");
            }

            // Strengths and Weaknesses
            if (data.TryGetProperty("strengths", out var strengths))
            {
                sb.Append("<div class='strengths-section'>");
                sb.Append("<h4>💪 Điểm mạnh</h4><ul>");
                foreach (var strength in strengths.EnumerateArray())
                {
                    sb.Append($"<li>{strength.GetString()}</li>");
                }
                sb.Append("</ul></div>");
            }
            
            if (data.TryGetProperty("weaknesses", out var weaknesses))
            {
                sb.Append("<div class='weaknesses-section'>");
                sb.Append("<h4>⚠️ Điểm yếu</h4><ul>");
                foreach (var weakness in weaknesses.EnumerateArray())
                {
                    sb.Append($"<li>{weakness.GetString()}</li>");
                }
                sb.Append("</ul></div>");
            }

            // Improvements
            if (data.TryGetProperty("improvements", out var improvements))
            {
                sb.Append("<div class='improvements-section'>");
                sb.Append("<h4>🛠️ Gợi ý cải thiện</h4><ul>");
                foreach (var improvement in improvements.EnumerateArray())
                {
                    sb.Append($"<li>{improvement.GetString()}</li>");
                }
                sb.Append("</ul></div>");
            }

            // Improvement Score
            if (data.TryGetProperty("improvementScore", out var improvementScore))
            {
                sb.Append("<div class='improvement-score'>");
                sb.Append($"<h4>📈 Điểm cải thiện: {improvementScore.GetInt32()}/100</h4>");
                sb.Append("</div>");
            }

            // Conclusion
            if (data.TryGetProperty("conclusion", out var conclusion))
            {
                sb.Append("<div class='conclusion-section'>");
                sb.Append("<h4>📋 Kết luận</h4>");
                sb.Append($"<p class='conclusion-text'>{conclusion.GetString()}</p>");
                sb.Append("</div>");
            }

            // Detailed Analysis
            if (data.TryGetProperty("detailedAnalysis", out var detailedAnalysis))
            {
                var analysis = detailedAnalysis.GetString();
                if (!string.IsNullOrEmpty(analysis))
                {
                    sb.Append("<div class='detailed-analysis'>");
                    sb.Append("<h4>📊 Phân tích chi tiết</h4>");
                    sb.Append($"<p>{analysis}</p>");
                    sb.Append("</div>");
                }
            }

            // If no structured data found, show the raw response
            if (sb.Length == 0)
            {
                sb.Append("<h3>Kết quả phân tích</h3>");
                sb.Append($"<p>{json}</p>");
            }

            return sb.ToString();
        }
    }
}
