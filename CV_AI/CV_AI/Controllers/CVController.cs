using Microsoft.AspNetCore.Mvc;
using CV_AI.Services;
using Microsoft.AspNetCore.Authorization;
using CV_AI.Data;
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
            if (cvFile == null || cvFile.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file CV.";
                return RedirectToAction("Index");
            }

            var allowedExtensions = new[] { ".txt", ".doc", ".docx", ".pdf" };
            var fileExtension = System.IO.Path.GetExtension(cvFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                TempData["Error"] = "Chỉ chấp nhận file .txt, .doc, .docx, .pdf.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(jobDescription))
            {
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
                TempData["EvaluationResult"] = FormatAnalysisResult(analysisResult);
            }
            catch (Exception ex)
            {
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

            // Check if overallScore exists
            if (data.TryGetProperty("overallScore", out var overallScore))
            {
                sb.Append($"<h3>Điểm tổng thể: {overallScore.GetInt32()}/100</h3>");
            }
            else
            {
                sb.Append("<h3>Điểm tổng thể: Chưa có dữ liệu</h3>");
            }
            
            if (data.TryGetProperty("matchPercentage", out var matchPercentage))
            {
                sb.Append($"<h4>Mức độ phù hợp với công việc: {matchPercentage.GetInt32()}%</h4>");
            }
            
            // Check if scores exist
            if (data.TryGetProperty("scores", out var scores))
            {
                sb.Append("<h4>Chi tiết điểm</h4><ul>");
                foreach (var score in scores.EnumerateObject())
                {
                    sb.Append($"<li>{score.Name}: {score.Value.GetInt32()}/100</li>");
                }
                sb.Append("</ul>");
            }

            // Check if strengths exist
            if (data.TryGetProperty("strengths", out var strengths))
            {
                sb.Append("<h4>Điểm mạnh</h4><ul>");
                foreach (var strength in strengths.EnumerateArray())
                {
                    sb.Append($"<li>{strength.GetString()}</li>");
                }
                sb.Append("</ul>");
            }
            
            // Check if weaknesses exist
            if (data.TryGetProperty("weaknesses", out var weaknesses))
            {
                sb.Append("<h4>Điểm yếu</h4><ul>");
                foreach (var weakness in weaknesses.EnumerateArray())
                {
                    sb.Append($"<li>{weakness.GetString()}</li>");
                }
                sb.Append("</ul>");
            }

            // Check if recommendations exist
            if (data.TryGetProperty("recommendations", out var recommendations))
            {
                sb.Append("<h4>Khuyến nghị</h4><ul>");
                foreach (var recommendation in recommendations.EnumerateArray())
                {
                    sb.Append($"<li>{recommendation.GetString()}</li>");
                }
                sb.Append("</ul>");
            }

            if (data.TryGetProperty("detailedAnalysis", out var detailedAnalysis))
            {
                var analysis = detailedAnalysis.GetString();
                if (!string.IsNullOrEmpty(analysis))
                {
                    sb.Append("<h4>Phân tích chi tiết</h4>");
                    sb.Append($"<p>{analysis}</p>");
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
