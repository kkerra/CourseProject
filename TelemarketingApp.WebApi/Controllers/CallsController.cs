using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelemarketingApp.WebApi.DataContexts;
using TelemarketingApp.WebApi.DTOs;
using TelemarketingApp.WebApi.Models;

namespace TelemarketingApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CallsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<CallDTO>>> GetCallsByEmployee(int employeeId)
        {
            var calls = await _context.Calls
                .Include(c => c.Client)
                .Include(c => c.Services)
                .Where(c => c.EmployeeId == employeeId)
                .OrderByDescending(c => c.CallDatetime)
                .ToListAsync();

            var callDtos = calls.Select(c => new CallDTO
            {
                CallId = c.CallId,
                CallDatetime = c.CallDatetime,
                Duration = c.Duration,
                Result = c.Result,
                Comment = c.Comment,
                EmployeeId = c.EmployeeId,
                ClientId = c.ClientId,
                Client = c.Client != null ? new ClientDTO
                {
                    ClientId = c.Client.ClientId,
                    Surname = c.Client.Surname,
                    Name = c.Client.Name,
                    Patronymic = c.Client.Patronymic
                } : null,
                Services = c.Services.Select(s => new ServiceDTO
                {
                    ServiceId = s.ServiceId,
                    Title = s.Title,
                    Description = s.Description,
                    Price = s.Price,
                    Category = s.Category
                }).ToList()
            }).ToList();

            return Ok(callDtos);
        }

        // GET: api/Calls
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Call>>> GetCalls()
        {
            var calls = await _context.Calls
        .Include(c => c.Client)
        .Include(c => c.Employee)
        .Include(c => c.Services)
        .OrderByDescending(c => c.CallDatetime)
        .Select(c => new
        {
            c.CallId,
            c.CallDatetime,
            c.Duration,
            c.Result,
            c.Comment,
            EmployeeName = c.Employee != null
                ? $"{c.Employee.Surname} {c.Employee.Name} {c.Employee.Patronymic}"
                : "Неизвестный оператор",
            ClientId = c.ClientId,
            ClientName = c.Client != null
                ? $"{c.Client.Surname} {c.Client.Name} {c.Client.Patronymic}"
                : "Клиент не указан",
            ServicesList = c.Services.Any()
                ? string.Join(", ", c.Services.Select(s => s.Title))
                : "Нет услуг"
        })
        .ToListAsync();

            return Ok(calls);
        }

        // GET: api/Calls/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Call>> GetCall(int id)
        {
            var call = await _context.Calls.FindAsync(id);

            if (call == null)
            {
                return NotFound();
            }

            return call;
        }

        // PUT: api/Calls/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCall(int id, Call call)
        {
            if (id != call.CallId)
            {
                return BadRequest();
            }

            _context.Entry(call).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CallExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Calls
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<IActionResult> CreateCall([FromBody] CreateCallDto callDto)
        {
            try
            {
                var client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.PhoneNumber == callDto.ClientPhone);

                if (client == null)
                {
                    // Разбиваем ФИО на части
                    var nameParts = callDto.ClientFullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    client = new Client
                    {
                        Surname = nameParts.Length > 0 ? nameParts[0] : "",
                        Name = nameParts.Length > 1 ? nameParts[1] : "",
                        Patronymic = nameParts.Length > 2 ? nameParts[2] : null,
                        PhoneNumber = callDto.ClientPhone,
                        Address = callDto.Address,
                        InteractionStatus = "Новый"
                    };

                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Обновляем адрес, если он изменился
                    if (!string.IsNullOrEmpty(callDto.Address) && client.Address != callDto.Address)
                    {
                        client.Address = callDto.Address;
                        _context.Clients.Update(client);
                        await _context.SaveChangesAsync();
                    }
                }

                // 2. Ищем услугу - несколько вариантов поиска
                Service? service = null;

                // Вариант 1: Ищем по точному совпадению
                service = await _context.Services
                    .FirstOrDefaultAsync(s => s.Title == callDto.ServiceTitle);

                // Вариант 2: Если не нашли, ищем без учета регистра
                if (service == null)
                {
                    service = await _context.Services
                        .FirstOrDefaultAsync(s => s.Title.ToLower() == callDto.ServiceTitle.ToLower());
                }

                // Вариант 3: Ищем по содержанию строки
                if (service == null)
                {
                    service = await _context.Services
                        .FirstOrDefaultAsync(s => s.Title.Contains(callDto.ServiceTitle));
                }

                // Вариант 4: Показываем все доступные услуги для отладки
                if (service == null)
                {
                    var allServices = await _context.Services
                        .Select(s => new { s.Title, s.Category })
                        .ToListAsync();

                    Console.WriteLine("Доступные услуги в базе:");
                    foreach (var s in allServices)
                    {
                        Console.WriteLine($"- {s.Title} (категория: {s.Category})");
                    }

                    return BadRequest(new
                    {
                        message = $"Услуга '{callDto.ServiceTitle}' не найдена. Доступные услуги: " +
                                  string.Join(", ", allServices.Select(s => s.Title))
                    });
                }

                Console.WriteLine($"Найдена услуга: ID={service.ServiceId}, Title='{service.Title}'");

                // 3. Создаем заявку (Call)
                var call = new Call
                {
                    CallDatetime = DateTime.Now,
                    Duration = callDto.Duration,
                    Result = callDto.Result,
                    Comment = callDto.Comment,
                    ClientId = client.ClientId,
                    EmployeeId = callDto.EmployeeId, 
                    Services = new List<Service> { service }
                };

                _context.Calls.Add(call);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Заявка успешно создана",
                    callId = call.CallId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { message = $"Ошибка при создании заявки: {ex.Message}" });
            }
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetServices()
        {
            var services = await _context.Services
                .Select(s => new {
                    s.ServiceId,
                    s.Title,
                    s.Description,
                    s.Price,
                    s.Category
                })
                .ToListAsync();

            return Ok(services);
        }

        // DELETE: api/Calls/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCall(int id)
        {
            var call = await _context.Calls.FindAsync(id);
            if (call == null)
            {
                return NotFound();
            }

            _context.Calls.Remove(call);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CallExists(int id)
        {
            return _context.Calls.Any(e => e.CallId == id);
        }
    }
}
