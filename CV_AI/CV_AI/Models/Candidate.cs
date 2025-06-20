using System.ComponentModel.DataAnnotations;

namespace CV_AI.Models
{
    public class Candidate
    {
        [Key]
        public string ID_Candidate { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(255)]
        public string? CV_Link { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
        public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    }
} 