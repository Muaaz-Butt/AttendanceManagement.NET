namespace AttendanceManagementSystem.Models
{
  public class Attendance
  {
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime Date { get; set; }
    public string? Status { get; set; }

    public User? Student { get; set; }
    public Course? Course { get; set; }
  }
}