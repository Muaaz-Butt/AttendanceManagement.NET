using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Models {
    public class AppilcationDBContext : DbContext {
        public AppilcationDBContext(DbContextOptions<AppilcationDBContext> options) : base(options) {}
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }


    }
}