namespace AttendanceManagementSystem.Models
{
  public class AttendanceViewModel
  {
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public int CourseId { get; set; }
    public DateTime Date { get; set; }
    public string? Status { get; set; }
  }
  public class AttendanceReportViewModel
  {
    public DateTime Date { get; set; }
    public List<AttendanceRecord>? Records { get; set; }
  }

  public class AttendanceRecord
  {
    public string? StudentName { get; set; }
    public string? Status { get; set; }
  }

  public class StudentAttendanceStats
  {
    public string? StudentName { get; set; }
    public int TotalClasses { get; set; }
    public int ClassesPresent { get; set; }
    public decimal AttendancePercentage { get; set; }
    public bool IsBelowThreshold => AttendancePercentage < 75;
  }
}