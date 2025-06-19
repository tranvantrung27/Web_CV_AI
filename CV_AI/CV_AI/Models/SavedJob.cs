namespace CV_AI.Models
{
    public class SavedJob
    {
        public int ID_Candidate { get; set; }
        public int ID_JobPost { get; set; }
        public DateTime SavedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Candidate Candidate { get; set; } = null!;
        public virtual JobPost JobPost { get; set; } = null!;
    }
} 