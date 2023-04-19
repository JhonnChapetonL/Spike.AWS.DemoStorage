using Amazon.Lambda.Core;
using Contact.API.Models;
using Enyim.Caching.Memcached;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Contact.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemcachedController : ControllerBase
    {
        private readonly string _endpoint = "memcached.ctswd6.cfg.use2.cache.amazonaws.com:11211";

        [HttpGet("{count}")]
        public async Task<ActionResult> GetAll(int count)
        {
            var employees = new List<Employee>();

            var sw = Stopwatch.StartNew();

            using (var _cluster = new MemcachedCluster(_endpoint))
            {
                _cluster.Start();

                var client = _cluster.GetClient();

                for (int i = 0; i < count; i++)
                {
                    var value = await client.GetAsync($"key-{i}");

                    if (value != null)
                    {
                        var employee = JsonSerializer.Deserialize<Employee>(value.ToString());
                        employees.Add(employee); ;
                    }
                }

                _cluster.Dispose();
            }

            sw.Stop();

            var executionTime = $"Memcached: It was executed in {sw.ElapsedMilliseconds}ms";

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
