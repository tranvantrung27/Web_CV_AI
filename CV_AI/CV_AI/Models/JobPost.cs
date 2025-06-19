using System.ComponentModel.DataAnnotations;

namespace CV_AI.Models
{
    public class JobPost
    {
        [Key]
        public int ID_JobPost { get; set; }

        [Required]
        public int ID_Employer { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(100, ErrorMessage = "Tiêu đề không được vượt quá 100 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string Description { get; set; } = string.Empty;

        public string? Requirements { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? Salary { get; set; }

        public DateTime PostedDate { get; set; } = DateTime.Now;

        public DateTime? ExpirationDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Employer Employer { get; set; } = null!;
        public virtual ICollection<JobPostCategory> JobPostCategories { get; set; } = new List<JobPostCategory>();
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
        public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    }
} 