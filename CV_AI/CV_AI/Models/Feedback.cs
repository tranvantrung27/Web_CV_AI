using System;
using System.ComponentModel.DataAnnotations;

namespace CV_AI.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "General Feedback"; // "Bug Report", "Feature Request", "General Feedback", "Complaint"

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung góp ý")]
        public string Content { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // "Pending", "In Progress", "Resolved", "Closed"

        [StringLength(20)]
        public string Priority { get; set; } = "Medium"; // "Low", "Medium", "High"

        public string? AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        // Navigation property
        public virtual User? User { get; set; }
    }
}

