using Amazon.Lambda.Core;
using Contact.API.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;

namespace Contact.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisController : ControllerBase
    {
        private static ConnectionMultiplexer _redis;

        private string redisEndpoint = "memorydb.ctswd6.clustercfg.memorydb.us-east-2.amazonaws.com:6379";
        public RedisController()
        {
            _redis = ConnectionMultiplexer.Connect(redisEndpoint);
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var employees = new List<Employee>();

            IDatabase db = _redis.GetDatabase();

            var keys = _redis.GetServer(redisEndpoint).Keys();

            var redisValues = new List<RedisValue>();

            var sw = Stopwatch.StartNew();

            foreach (var key in keys)
            {
                var value = await db.StringGetAsync(key);

                redisValues.Add(value);
            }

            sw.Stop();

            foreach (var value in redisValues)
            {
                var employee = JsonSerializer.Deserialize<Employee>(value);

                employees.Add(employee);
            }

            var executionTime = $"Redis: It was executed in {sw.ElapsedMilliseconds}ms";

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