using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CV_AI.Models
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty; // "Candidate", "Employer", "Admin"

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Candidate? Candidate { get; set; }
        public virtual Employer? Employer { get; set; }
    }
} 