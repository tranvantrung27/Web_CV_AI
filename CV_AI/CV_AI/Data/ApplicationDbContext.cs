using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CV_AI.Models;

namespace CV_AI.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Employer> Employers { get; set; }
        public DbSet<JobPost> JobPosts { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<JobPostCategory> JobPostCategories { get; set; }
        public DbSet<SavedJob> SavedJobs { get; set; }
        public DbSet<CV_AI.Models.Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite keys
            modelBuilder.Entity<JobPostCategory>()
                .HasKey(jpc => new { jpc.ID_JobPost, jpc.ID_Category });

            modelBuilder.Entity<SavedJob>()
                .HasKey(sj => new { sj.ID_Candidate, sj.ID_JobPost });

            // Configure relationships with explicit cascade delete behavior
            modelBuilder.Entity<Candidate>()
                .HasOne(c => c.User)
                .WithOne(u => u.Candidate)
                .HasForeignKey<Candidate>(c => c.ID_Candidate)
                .OnDelete(DeleteBehavior.Restrict); // Tắt cascade delete

            modelBuilder.Entity<Employer>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employer)
                .HasForeignKey<Employer>(e => e.ID_Employer)
                .OnDelete(DeleteBehavior.Restrict); // Tắt cascade delete

            modelBuilder.Entity<JobPost>()
                .HasOne(jp => jp.Employer)
                .WithMany(e => e.JobPosts)
                .HasForeignKey(jp => jp.ID_Employer)
                .OnDelete(DeleteBehavior.Restrict); // Tắt cascade delete

            modelBuilder.Entity<JobPostCategory>()
                .HasOne(jpc => jpc.JobPost)
                .WithMany(jp => jp.JobPostCategories)
                .HasForeignKey(jpc => jpc.ID_JobPost)
                .OnDelete(DeleteBehavior.Cascade); // Cho phép cascade delete

            modelBuilder.Entity<JobPostCategory>()
                .HasOne(jpc => jpc.Category)
                .WithMany(c => c.JobPostCategories)
                .HasForeignKey(jpc => jpc.ID_Category)
                .OnDelete(DeleteBehavior.Restrict); // Tắt cascade delete

            modelBuilder.Entity<Application>()
                .HasOne(a => a.JobPost)
                .WithMany(jp => jp.Applications)
                .HasForeignKey(a => a.ID_JobPost)
                .OnDelete(DeleteBehavior.Cascade); // Cho phép cascade delete khi xóa JobPost

            modelBuilder.Entity<Application>()
                .HasOne(a => a.Candidate)
                .WithMany(c => c.Applications)
                .HasForeignKey(a => a.ID_Candidate)
                .OnDelete(DeleteBehavior.Restrict); // Tắt cascade delete

            modelBuilder.Entity<SavedJob>()
                .HasOne(sj => sj.Candidate)
                .WithMany(c => c.SavedJobs)
                .HasForeignKey(sj => sj.ID_Candidate)
                .OnDelete(DeleteBehavior.Cascade); // Cho phép cascade delete

            modelBuilder.Entity<SavedJob>()
                .HasOne(sj => sj.JobPost)
                .WithMany(jp => jp.SavedJobs)
                .HasForeignKey(sj => sj.ID_JobPost)
                .OnDelete(DeleteBehavior.Cascade); // Cho phép cascade delete
        }
    }
} 