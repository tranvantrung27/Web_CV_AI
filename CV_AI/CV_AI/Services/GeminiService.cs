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
                Analyze the following CV based on the provided job description. 
                **Your response MUST be in Vietnamese.** 
                However, keep all technical terms and programming-related keywords (e.g., ReactJS, Node.js, API, Docker, etc.) in their original English form.
                Return the result in a strict JSON format.

                **Job Description:**
                ---
                {jobDescription}
                ---

                **CV Content:**
                ---
                {cvText}
                ---

                **JSON Output Format:**
                Please provide a detailed analysis in the following JSON structure. Do not add any text before or after the JSON object.
                {{
                  ""overallScore"": <A number from 0 to 100 representing the overall match>,
                  ""scores"": {{
                    ""Experience"": <A score from 0 to 100 for experience>,
                    ""Skills"": <A score from 0 to 100 for skills>,
                    ""Education"": <A score from 0 to 100 for education>,
                    ""Relevance"": <A score from 0 to 100 for job relevance>
                  }},
                  ""strengths"": [
                    ""A summary of the candidate's first strength in Vietnamese."",
                    ""A summary of the candidate's second strength in Vietnamese.""
                  ],
                  ""weaknesses"": [
                    ""A summary of the candidate's first weakness in Vietnamese."",
                    ""A summary of the candidate's second weakness in Vietnamese.""
                  ],
                  ""matchPercentage"": <A number from 0 to 100 for the match percentage>,
                  ""detailedAnalysis"": ""A comprehensive text analysis of the CV against the job description in Vietnamese.""
                }}";
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