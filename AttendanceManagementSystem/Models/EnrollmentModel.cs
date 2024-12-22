namespace AttendanceManagementSystem.Models {
    public class Enrollment {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; } 
        public string? StudentCode { get; set; } 

        public Course? Course { get; set; }
        public User? Student { get; set; }
    }
}
