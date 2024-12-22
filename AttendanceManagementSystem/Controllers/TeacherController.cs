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
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
public class TeacherController : Controller
{
  private readonly AppilcationDBContext _context;

  public TeacherController(AppilcationDBContext context)
  {
    _context = context;
  }

  public IActionResult MarkAttendance()
  {
    var teacherEmail = User.Identity.Name;
    var subjects = _context.Courses
                        .Where(c => c.CreatedBy == teacherEmail)
                        .ToList();
    return View(subjects);
  }

  public async Task<IActionResult> SubjectAttendance(int courseId)
  {
    var enrolledStudents = await _context.Enrollments
        .Include(e => e.Student)
        .Where(e => e.CourseId == courseId)
        .Select(e => new AttendanceViewModel
        {
          StudentId = e.StudentId,
          StudentName = e.StudentName,
          CourseId = e.CourseId,
          Date = DateTime.Now.Date,
          Status = "Present" // Default value
        })
        .ToListAsync();

    ViewBag.CourseId = courseId;
    ViewBag.CourseName = await _context.Courses
        .Where(c => c.Id == courseId)
        .Select(c => c.CourseName)
        .FirstOrDefaultAsync();
    ViewBag.CurrentDate = DateTime.Now.ToString("yyyy-MM-dd");

    return View(enrolledStudents);
  }

  [HttpPost]
  public async Task<IActionResult> SaveAttendance(List<AttendanceViewModel> attendanceList)
  {
    foreach (var attendance in attendanceList)
    {
      var newAttendance = new Attendance
      {
        StudentId = attendance.StudentId,
        CourseId = attendance.CourseId,
        Date = attendance.Date,
        Status = attendance.Status
      };
      _context.Attendances.Add(newAttendance);
    }
    await _context.SaveChangesAsync();

    TempData["Message"] = "Attendance marked successfully!";
    return RedirectToAction("MarkAttendance");
  }
  public IActionResult ViewAttendance()
  {
    var teacherEmail = User.Identity.Name;
    var subjects = _context.Courses
                        .Where(c => c.CreatedBy == teacherEmail)
                        .ToList();
    return View(subjects);
  }

  public async Task<IActionResult> SubjectAttendanceReport(int courseId)
  {
    var courseName = await _context.Courses
        .Where(c => c.Id == courseId)
        .Select(c => c.CourseName)
        .FirstOrDefaultAsync();

    // Get all attendance records
    var attendanceRecords = await _context.Attendances
        .Include(a => a.Student)
        .Where(a => a.CourseId == courseId)
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
        .Where(a => a.CourseId == courseId)
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
        .OrderByDescending(s => s.AttendancePercentage)
        .ToList();

    ViewBag.CourseName = courseName;
    ViewBag.StudentStats = studentStats;
    return View(attendanceRecords);
  }

  public IActionResult GenerateReports()
  {
    var teacherEmail = User.Identity.Name;
    var subjects = _context.Courses
                        .Where(c => c.CreatedBy == teacherEmail)
                        .ToList();
    return View(subjects);
  }

  public async Task<IActionResult> DownloadReport(int courseId)
  {
    var course = await _context.Courses
        .FirstOrDefaultAsync(c => c.Id == courseId);

    if (course == null)
      return NotFound();

    // Get attendance data
    var attendanceData = await _context.Attendances
        .Where(a => a.CourseId == courseId)
        .GroupBy(a => a.Student.Name)
        .Select(g => new
        {
          StudentName = g.Key,
          TotalClasses = g.Count(),
          ClassesPresent = g.Count(a => a.Status == "Present")
        })
        .ToListAsync();

    // Calculate statistics
    var studentStats = attendanceData
        .Select(g => new StudentAttendanceStats
        {
          StudentName = g.StudentName,
          TotalClasses = g.TotalClasses,
          ClassesPresent = g.ClassesPresent,
          AttendancePercentage = (decimal)g.ClassesPresent / g.TotalClasses * 100
        })
        .OrderByDescending(s => s.AttendancePercentage)
        .ToList();

    // Generate PDF
    using (MemoryStream ms = new MemoryStream())
    {
      Document document = new Document(PageSize.A4, 25, 25, 30, 30);
      PdfWriter writer = PdfWriter.GetInstance(document, ms);

      document.Open();

      // Add title
      Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
      Paragraph title = new Paragraph($"Attendance Report - {course.CourseName}", titleFont);
      title.Alignment = Element.ALIGN_CENTER;
      title.SpacingAfter = 20f;
      document.Add(title);

      // Add course details
      Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
      document.Add(new Paragraph($"Course Code: {course.CourseCode}", normalFont));
      document.Add(new Paragraph($"Teacher: {course.TeacherName}", normalFont));
      document.Add(new Paragraph($"Report Generated: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont));
      document.Add(new Paragraph("\n"));

      // Create table
      PdfPTable table = new PdfPTable(5);
      table.WidthPercentage = 100;
      table.SetWidths(new float[] { 3f, 2f, 2f, 2f, 2f });

      // Add headers
      Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
      table.AddCell(new PdfPCell(new Phrase("Student Name", headerFont)));
      table.AddCell(new PdfPCell(new Phrase("Total Classes", headerFont)));
      table.AddCell(new PdfPCell(new Phrase("Classes Present", headerFont)));
      table.AddCell(new PdfPCell(new Phrase("Classes Absent", headerFont)));
      table.AddCell(new PdfPCell(new Phrase("Attendance %", headerFont)));

      // Add data
      foreach (var stat in studentStats)
      {
        table.AddCell(new Phrase(stat.StudentName, normalFont));
        table.AddCell(new Phrase(stat.TotalClasses.ToString(), normalFont));
        table.AddCell(new Phrase(stat.ClassesPresent.ToString(), normalFont));
        table.AddCell(new Phrase((stat.TotalClasses - stat.ClassesPresent).ToString(), normalFont));
        table.AddCell(new Phrase($"{stat.AttendancePercentage:F1}%", normalFont));
      }

      document.Add(table);

      // Add summary
      document.Add(new Paragraph("\nSummary:", headerFont));
      document.Add(new Paragraph($"Total Students: {studentStats.Count}", normalFont));
      document.Add(new Paragraph($"Students Below 75% Attendance: {studentStats.Count(s => s.AttendancePercentage < 75)}", normalFont));

      document.Close();

      // Return the PDF file
      string fileName = $"Attendance_Report_{course.CourseCode}_{DateTime.Now:yyyyMMdd}.pdf";
      return File(ms.ToArray(), "application/pdf", fileName);
    }
  }
}