  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using AttendanceManagementSystem.Models;

  namespace AttendanceManagementSystem.Controllers
  {
      public class DashboardController : Controller
      {
          private readonly AppilcationDBContext _context;

          public DashboardController(AppilcationDBContext context) {
              _context = context;
          }
          [Authorize(Roles = "Teacher")]
          public IActionResult TeacherDashboard()
          {
              return View();
          }

          [Authorize(Roles = "Student")]

          public IActionResult StudentDashboard()
          {
              return View();
          }
      }
  }
