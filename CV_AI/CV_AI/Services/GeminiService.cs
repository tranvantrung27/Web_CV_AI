using System;
using System.Text.Json;
using System.Threading.Tasks;
using Mscc.GenerativeAI;
using Microsoft.Extensions.Configuration;

namespace CV_AI.Services
{
    public interface IGeminiService
    {
        Task<object?> AnalyzeCVAsync(string cvText, string jobDescription);
    }

    public class GeminiService : IGeminiService
    {
        private readonly GenerativeModel _generativeModel;

        public GeminiService(IConfiguration configuration)
        {
            var apiKey = configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Gemini API key is not configured.");
            }
            var googleAI = new GoogleAI(apiKey: apiKey);
            _generativeModel = googleAI.GenerativeModel(Model.Gemini15FlashLatest);
        }

        public async Task<object?> AnalyzeCVAsync(string cvText, string jobDescription)
        {
            var prompt = CreateAnalysisPrompt(cvText, jobDescription);
            try
            {
                var response = await _generativeModel.GenerateContent(prompt);
                
                var jsonResponse = ExtractJsonFromResponse(response.Text);
                if (string.IsNullOrEmpty(jsonResponse))
                {
                    // Fallback if JSON is not found
                    return new { detailedAnalysis = response.Text };
                }

                var analysisResult = JsonSerializer.Deserialize<object>(jsonResponse);
                return analysisResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Gemini API: {ex.Message}");
                // Return a structured error object
                return new 
                { 
                    error = "Failed to analyze CV with Gemini.",
                    details = ex.Message
                };
            }
        }

        private string CreateAnalysisPrompt(string cvText, string jobDescription)
        {
            return $@"
                Phân tích CV dựa trên mô tả công việc và trả về kết quả theo format JSON nghiêm ngặt bằng tiếng Việt.
                Giữ nguyên các thuật ngữ kỹ thuật bằng tiếng Anh (ReactJS, Node.js, API, Docker, etc.).

                **Mô tả công việc:**
                ---
                {jobDescription}
                ---

                **Nội dung CV:**
                ---
                {cvText}
                ---

                **Yêu cầu format JSON:**
                Trả về kết quả theo cấu trúc JSON sau đây, không thêm text trước hoặc sau JSON:

                {{
                  ""overallScore"": <Điểm tổng thể từ 0-100>,
                  ""matchPercentage"": <Phần trăm phù hợp từ 0-100>,
                  ""criteriaComparison"": [
                    {{
                      ""criteria"": ""<Tên tiêu chí - PHẢI chứa 'Kinh nghiệm', 'Kỹ năng' hoặc 'Học vấn'>"",
                      ""jobRequirement"": ""<Yêu cầu từ JD>"",
                      ""candidateProfile"": ""<Hồ sơ ứng viên>"",
                      ""status"": ""<Đạt/Chưa đạt/Ưu tiên>"",
                      ""score"": <Điểm từ 0-100>
                    }}
                  ],
                  ""strengths"": [
                    ""<Điểm mạnh 1>"",
                    ""<Điểm mạnh 2>"",
                    ""<Điểm mạnh 3>""
                  ],
                  ""weaknesses"": [
                    ""<Điểm yếu 1>"",
                    ""<Điểm yếu 2>"",
                    ""<Điểm yếu 3>""
                  ],
                  ""improvements"": [
                    ""<Gợi ý cải thiện 1>"",
                    ""<Gợi ý cải thiện 2>"",
                    ""<Gợi ý cải thiện 3>""
                  ],
                  ""improvementScore"": <Điểm cải thiện từ 0-100>,
                  ""conclusion"": ""<Kết luận tổng quan về khả năng phù hợp>"",
                  ""detailedAnalysis"": ""<Phân tích chi tiết bằng tiếng Việt>""
                }}

                **Hướng dẫn phân tích:**
                1. So sánh từng tiêu chí giữa yêu cầu công việc và hồ sơ ứng viên. Bảng 'criteriaComparison' PHẢI bao gồm ít nhất một tiêu chí cho mỗi loại sau: 'Kinh nghiệm', 'Kỹ năng', và 'Học vấn'.
                2. Đánh giá điểm mạnh dựa trên kinh nghiệm, kỹ năng phù hợp
                3. Chỉ ra điểm yếu cần khắc phục
                4. Đưa ra gợi ý cải thiện cụ thể
                5. Kết luận về khả năng đạt được vị trí
                6. Điểm tổng thể dựa trên mức độ phù hợp tổng quan";
        }

        private string ExtractJsonFromResponse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var startIndex = text.IndexOf('{');
            var endIndex = text.LastIndexOf('}');

            if (startIndex == -1 || endIndex == -1 || endIndex < startIndex)
            {
                return string.Empty;
            }

            return text.Substring(startIndex, endIndex - startIndex + 1);
        }
    }
} 