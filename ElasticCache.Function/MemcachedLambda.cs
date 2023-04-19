using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Enyim.Caching.Memcached;
using ElasticCache.Function.Models;
using System.Diagnostics;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ElasticCache.Function;

public class MemcachedLambda
{
    private readonly string _endpoint = "memcached.ctswd6.cfg.use2.cache.amazonaws.com:11211";

    public MemcachedLambda()
    {
    }

    public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var recordsCount = Convert.ToInt32(request.Body);

        var response = await InnerHandler(recordsCount);

        return response;
    }

    private async Task<APIGatewayProxyResponse> InnerHandler(int count)
    {
        LambdaLogger.Log($"ElastiCache: Executing {count} records...");

        var employees = GenerateRandomEmployees(count);

        long elapsedMilliseconds = 0;

        using (var _cluster = new MemcachedCluster(_endpoint))
        {
            _cluster.Start();

            var client = _cluster.GetClient();

            await client.FlushAll();

            var sw = Stopwatch.StartNew();

            foreach (var employee in employees)
            {
                var isExecuted = await client.SetAsync($"key-{employee.Id}", JsonSerializer.Serialize(employee));
                LambdaLogger.Log(isExecuted.ToString());
            }

            sw.Stop();

            elapsedMilliseconds = sw.ElapsedMilliseconds;

            _cluster.Dispose();
        }

        var result = $"ElastiCache: {count} records was added in {elapsedMilliseconds} ms";

        LambdaLogger.Log(result);

        var response = new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = result,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json; charset=utf-8" } }
        };

        return response;
    }

    private static List<Employee> GenerateRandomEmployees(int count)
    {
        var employees = new List<Employee>();

        var random = new Random();

        for (int i = 0; i < count; i++)
        {
            var employee = new Employee
            {
                Id = i,
                Name = "Employee " + (i + 1),
                Age = random.Next(18, 65),
                Department = "Department " + random.Next(1, 6)
            };

            employees.Add(employee);
        }

        return employees;
    }
}