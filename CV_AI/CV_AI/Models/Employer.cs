using System.ComponentModel.DataAnnotations;

namespace CV_AI.Models
{
    public class Employer
    {
        [Key]
        public string ID_Employer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên công ty không được để trống")]
        [StringLength(100, ErrorMessage = "Tên công ty không được vượt quá 100 ký tự")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? CompanyWebsite { get; set; }

        [StringLength(255)]
        public string? CompanyAddress { get; set; }

        public string? Description { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
    }
} 