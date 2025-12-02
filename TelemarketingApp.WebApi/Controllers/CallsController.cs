using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelemarketingApp.WebApi.Contexts;
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
            return await _context.Calls.ToListAsync();
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
        [HttpPost]
        public async Task<ActionResult<Call>> PostCall(Call call)
        {
            _context.Calls.Add(call);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCall", new { id = call.CallId }, call);
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
