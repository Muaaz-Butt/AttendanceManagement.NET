using AttendanceManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class StudentController : Controller
{
  private readonly AppilcationDBContext _context;

  public StudentController(AppilcationDBContext context)
  {
    _context = context;
  }

  public async Task<IActionResult> ViewAttendance()
  {
    var studentEmail = User.Identity.Name;
    var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == studentEmail);

    if (student == null)
    {
      return NotFound("Student not found.");
    }

    var enrolledCourses = await _context.Enrollments
        .Where(e => e.StudentId == student.Id)
        .Select(e => new AttendanceViewModel
        {
          StudentId = e.StudentId,
          StudentName = e.Student.Name,
          CourseId = e.CourseId
        })
        .ToListAsync();

    return View(enrolledCourses);
  }

  public async Task<IActionResult> SubjectAttendanceReport(int courseId)
  {
    var studentEmail = User.Identity.Name;
    var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == studentEmail);

    if (student == null)
    {
      return NotFound("Student not found.");
    }

    var courseName = await _context.Courses
        .Where(c => c.Id == courseId)
        .Select(c => c.CourseName)
        .FirstOrDefaultAsync();

    // Get attendance records for the logged-in student only
    var attendanceRecords = await _context.Attendances
        .Include(a => a.Student)
        .Where(a => a.CourseId == courseId && a.Student.Email == studentEmail) // Filter by student
        .GroupBy(a => new { a.Date })
        .Select(g => new AttendanceReportViewModel
        {
          Date = g.Key.Date,
          Records = g.Select(a => new AttendanceRecord
          {
            StudentName = a.Student.Name,
            Status = a.Status
          }).ToList()
        })
        .OrderByDescending(a => a.Date)
        .ToListAsync();

    // Calculate attendance statistics
    var attendanceData = await _context.Attendances
        .Where(a => a.CourseId == courseId && a.Student.Email == studentEmail) // Filter by student
        .GroupBy(a => a.Student.Name)
        .Select(g => new
        {
          StudentName = g.Key,
          TotalClasses = g.Count(),
          ClassesPresent = g.Count(a => a.Status == "Present")
        })
        .ToListAsync();

    // Calculate percentages and create stats objects
    var studentStats = attendanceData
        .Select(g => new StudentAttendanceStats
        {
          StudentName = g.StudentName,
          TotalClasses = g.TotalClasses,
          ClassesPresent = g.ClassesPresent,
          AttendancePercentage = (decimal)g.ClassesPresent / g.TotalClasses * 100
        })
        .FirstOrDefault(); // Get stats for the logged-in student

    ViewBag.CourseName = courseName;
    ViewBag.StudentStats = studentStats; // Only pass stats for the logged-in student

    return View(attendanceRecords);
  }


}