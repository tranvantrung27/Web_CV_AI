using System.ComponentModel.DataAnnotations;

namespace CV_AI.Models
{
    public class Application
    {
        [Key]
        public int ID_Application { get; set; }

        [Required]
        public int ID_JobPost { get; set; }

        [Required]
        public string ID_Candidate { get; set; } = string.Empty;

        public DateTime AppliedDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected

        // Navigation properties
        public virtual JobPost JobPost { get; set; } = null!;
        public virtual Candidate Candidate { get; set; } = null!;
    }
} 