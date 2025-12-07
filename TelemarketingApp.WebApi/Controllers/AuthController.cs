using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelemarketingApp.WebApi.DataContexts;
using TelemarketingApp.WebApi.Models;

namespace TelemarketingApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<Employee>> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Login) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Логин и пароль обязательны" });
            }

            var employee = await _context.Employees
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Login == request.Login && e.Password == request.Password);

            if (employee == null)
            {
                return Unauthorized(new { message = "Неверный логин или пароль" });
            }

            var response = new
            {
                employee.EmployeeId,
                employee.Surname,
                employee.Name,
                employee.Patronymic,
                employee.Login,
                employee.Email,
                employee.RoleId
            };

            return Ok(response);
        }

        public class LoginRequest
        {
            public string Login { get; set; }
            public string Password { get; set; }
        }
    }
}
