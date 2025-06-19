using System.ComponentModel.DataAnnotations;

namespace CV_AI.Models
{
    public class Category
    {
        [Key]
        public int ID_Category { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<JobPostCategory> JobPostCategories { get; set; } = new List<JobPostCategory>();
    }
} 