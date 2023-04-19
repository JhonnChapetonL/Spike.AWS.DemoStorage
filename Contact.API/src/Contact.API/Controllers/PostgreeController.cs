using Amazon.Lambda.Core;
using Contact.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Contact.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostgreeController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public PostgreeController(AppDbContext db)
        {
            _dbContext = db;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var sw = Stopwatch.StartNew();

            var employees = await _dbContext.Employee.ToListAsync();

            sw.Stop();

            var executionTime = $"PostgreSQL: It was executed in {sw.ElapsedMilliseconds}ms";

            LambdaLogger.Log(executionTime);

            var response = new Result
            {
                ExecutionTime = executionTime,
                Body = new Body { Total = employees.Count, Employees = employees }
            };

            return Ok(response);
        }
    }
}
