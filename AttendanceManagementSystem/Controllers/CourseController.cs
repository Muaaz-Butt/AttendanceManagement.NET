using AttendanceManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AttendanceManagementSystem.Controllers
{
  [Authorize(Roles = "Teacher")]
  public class CourseController : Controller
  {
    private readonly AppilcationDBContext _context;

    public CourseController(AppilcationDBContext context)
    {
      _context = context;
    }

    // Display the form to create a course
    public IActionResult Create()
    {
      return View();
    }

    // Handle form submission to create a course
    [HttpPost]
    public async Task<IActionResult> Create(Course course)
    {
      if (ModelState.IsValid)
      {
        // Link the course to the logged-in teacher
        course.CreatedBy = User.Identity.Name;
        course.TeacherName = User.Identity.Name; // You can store the name here too

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return RedirectToAction("TeacherDashboard", "Dashboard");
      }
      return View(course);
    }

    // Show only courses for the logged-in teacher
    public IActionResult MyCourses()
    {
      var teacherEmail = User.Identity.Name;
      var myCourses = _context.Courses
                              .Where(c => c.CreatedBy == teacherEmail)
                              .ToList();
      return View(myCourses);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
      var course = _context.Courses.FirstOrDefault(c => c.Id == id);
      if (course == null)
      {
        return NotFound();
      }

      _context.Courses.Remove(course);
      _context.SaveChanges();

      TempData["Message"] = "Course deleted successfully.";
      return RedirectToAction("MyCourses");
    }

    public IActionResult Subjects()
    {
      var teacherEmail = User.Identity.Name;
      var subjects = _context.Courses
                                .Where(c => c.CreatedBy == teacherEmail)
                                .ToList();
      return View(subjects);

    }
    public async Task<IActionResult> AvailableStudents(int courseId)
    {
      // Get all students who aren't enrolled in this course
      var availableStudents = await _context.Users
          .Where(u => u.Role == "Student")
          .Where(u => !_context.Enrollments
              .Any(e => e.CourseId == courseId && e.StudentId == u.Id))
          .ToListAsync();

      ViewBag.CourseId = courseId;
      return View(availableStudents);
    }

    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> EnrollStudent(int courseId, int studentId)
    {
      var student = await _context.Users.FindAsync(studentId);

      var enrollment = new Enrollment
      {
        CourseId = courseId,
        StudentId = studentId,
        StudentName = student.Name,
        StudentCode = student.Email
      };

      _context.Enrollments.Add(enrollment);
      await _context.SaveChangesAsync();

      TempData["Message"] = "Student enrolled successfully.";
      return RedirectToAction("AvailableStudents", new { courseId = courseId });
    }
  }
}
