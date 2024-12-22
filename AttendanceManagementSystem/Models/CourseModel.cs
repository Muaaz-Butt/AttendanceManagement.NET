namespace AttendanceManagementSystem.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string? CourseName { get; set; }
        public string? CourseCode { get; set; }
        public string? TeacherName { get; set; } 
        public int CreditHours { get; set; }
        public string? CreatedBy { get; set; } // To link to the teacher (store email or ID)
    }
}
  