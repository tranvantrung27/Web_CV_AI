using System.Text.Json.Serialization;

namespace CV_AI.Models.CV
{
    public class EvaluationResultViewModel
    {
        [JsonPropertyName("overallScore")]
        public int OverallScore { get; set; }

        [JsonPropertyName("matchPercentage")]
        public int MatchPercentage { get; set; }

        [JsonPropertyName("criteriaComparison")]
        public List<CriteriaComparison>? CriteriaComparison { get; set; }

        [JsonPropertyName("strengths")]
        public List<string>? Strengths { get; set; }

        [JsonPropertyName("weaknesses")]
        public List<string>? Weaknesses { get; set; }

        [JsonPropertyName("improvements")]
        public List<string>? Improvements { get; set; }

        [JsonPropertyName("improvementScore")]
        public int ImprovementScore { get; set; }

        [JsonPropertyName("conclusion")]
        public string? Conclusion { get; set; }

        [JsonPropertyName("detailedAnalysis")]
        public string? DetailedAnalysis { get; set; }

        // Legacy properties for backward compatibility
        [JsonPropertyName("match_scores")]
        public MatchScores? MatchScores { get; set; }

        [JsonPropertyName("explanations")]
        public Explanations? Explanations { get; set; }
        
        // This property will hold the original parsed data if scoring fails
        // or if we want to show both.
        public ParsedCVData? ParsedData { get; set; }

        public bool IsScored { get; set; } = false;
    }

    public class CriteriaComparison
    {
        [JsonPropertyName("criteria")]
        public string? Criteria { get; set; }

        [JsonPropertyName("jobRequirement")]
        public string? JobRequirement { get; set; }

        [JsonPropertyName("candidateProfile")]
        public string? CandidateProfile { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }
    }

    public class MatchScores
    {
        [JsonPropertyName("overall_match")]
        public int OverallMatch { get; set; }

        [JsonPropertyName("skills_match")]
        public int SkillsMatch { get; set; }

        [JsonPropertyName("experience_match")]
        public int ExperienceMatch { get; set; }

        [JsonPropertyName("education_match")]
        public int EducationMatch { get; set; }
        
        // Add any other scores you want to display from the API response
    }

    public class Explanations
    {
        [JsonPropertyName("skills_match")]
        public string? SkillsMatch { get; set; }

        [JsonPropertyName("experience_match")]
        public string? ExperienceMatch { get; set; }

        [JsonPropertyName("education_match")]
        public string? EducationMatch { get; set; }

        // Add any other explanations
    }

    // Classes to hold the initially parsed CV data, in case we need it
    public class ParsedCVData
    {
        [JsonPropertyName("candidate_name")]
        public string? CandidateName { get; set; }

        [JsonPropertyName("candidate_email")]
        public string? CandidateEmail { get; set; }

        [JsonPropertyName("candidate_phone")]
        public string? CandidatePhone { get; set; }

        [JsonPropertyName("positions")]
        public List<Position>? Positions { get; set; }

        [JsonPropertyName("education_qualifications")]
        public List<Education>? EducationQualifications { get; set; }
    }

    public class Position
    {
        [JsonPropertyName("position_name")]
        public string? PositionName { get; set; }

        [JsonPropertyName("company_name")]
        public string? CompanyName { get; set; }

        [JsonPropertyName("job_details")]
        public string? JobDetails { get; set; }
        
        [JsonPropertyName("skills")]
        public List<string>? Skills { get; set; }
    }

    public class Education
    {
        [JsonPropertyName("school_name")]
        public string? SchoolName { get; set; }

        [JsonPropertyName("degree_type")]
        public string? DegreeType { get; set; }
        
        [JsonPropertyName("specialization_subjects")]
        public string? SpecializationSubjects { get; set; }
    }
} 