namespace CV_AI.Models
{
    public class JobPostCategory
    {
        public int ID_JobPost { get; set; }
        public int ID_Category { get; set; }

        // Navigation properties
        public virtual JobPost JobPost { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
    }
} 