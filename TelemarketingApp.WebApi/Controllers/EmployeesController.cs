using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelemarketingApp.WebApi.DataContexts;
using TelemarketingApp.WebApi.Models;

namespace TelemarketingApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            return await _context.Employees.ToListAsync();
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Проверяем уникальность логина (если логин изменился)
            if (employee.Login != request.Login)
            {
                bool loginExists = await _context.Employees.AnyAsync(e => e.Login == request.Login);
                if (loginExists)
                {
                    return BadRequest(new { message = "Пользователь с таким логином уже существует" });
                }
            }

            employee.Surname = request.Surname;
            employee.Name = request.Name;
            employee.Patronymic = request.Patronymic;
            employee.Login = request.Login;
            employee.Email = request.Email;
            employee.RoleId = request.RoleId;

            // Обновляем пароль только если он указан
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                employee.Password = request.Password;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Surname) ||
                string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Login) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Фамилия, имя, логин и пароль обязательны" });
            }

            bool loginExists = await _context.Employees.AnyAsync(e => e.Login == request.Login);
            if (loginExists)
            {
                return BadRequest(new { message = "Пользователь с таким логином уже существует" });
            }

            var employee = new Employee
            {
                Surname = request.Surname,
                Name = request.Name,
                Patronymic = request.Patronymic,
                Login = request.Login,
                Password = request.Password,
                Email = request.Email,
                RoleId = request.RoleId
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployees), new { id = employee.EmployeeId }, employee);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateEmployeeStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Логируем полученные данные
            Console.WriteLine($"Получен запрос на изменение статуса: IsActive = {request.IsActive}");

            // Конвертируем bool в ulong (если request.IsActive имеет тип bool)
            // Или напрямую присваиваем (если request.IsActive имеет тип ulong)
            employee.IsActive = request.IsActive; // Если request.IsActive уже ulong

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Calls)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            if (employee.Calls.Any())
            {
                return Conflict(new { message = "Нельзя удалить сотрудника, у которого есть заявки" });
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class UpdateStatusRequest
    {
        public ulong IsActive { get; set; }
    }

    public class UpdateEmployeeRequest
    {
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
    }

    public class CreateEmployeeRequest
    {
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
    }
}
