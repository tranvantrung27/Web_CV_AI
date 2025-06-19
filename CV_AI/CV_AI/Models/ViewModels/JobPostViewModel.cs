using System.ComponentModel.DataAnnotations;

namespace CV_AI.Models.ViewModels
{
    public class JobPostViewModel
    {
        public int ID_JobPost { get; set; }

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

        public DateTime? ExpirationDate { get; set; }

        public List<int> CategoryIds { get; set; } = new List<int>();

        // For display
        public string? CompanyName { get; set; }
        public DateTime PostedDate { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
    }
} 