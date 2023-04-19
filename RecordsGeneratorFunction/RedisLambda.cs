using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using StackExchange.Redis;
using MemoryDB.Models;
using System.Text.Json;
using System.Diagnostics;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MemoryDB;

public class RedisLambda
{
    private static ConnectionMultiplexer _redis;

    static RedisLambda()
    {
        string redisEndpoint = "memorydb.ctswd6.clustercfg.memorydb.us-east-2.amazonaws.com:6379";
        _redis = ConnectionMultiplexer.Connect(redisEndpoint);
    }

    public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var recordsCount = Convert.ToInt32(request.Body);

        var response = await InnerHandler(recordsCount); ;

        return response;
    }

    private async Task<APIGatewayProxyResponse> InnerHandler(int count)
    {
        IDatabase db = _redis.GetDatabase();

        LambdaLogger.Log($"Redis: Executing {count} records...");

        var employees = GenerateRandomEmployees(count);

        var sw = Stopwatch.StartNew();

        foreach (var employee in employees)
        {
            var json = JsonSerializer.Serialize(employee);

            await db.StringSetAsync($"employee_{employee.Id}", json);
        }

        sw.Stop();

        var result = $"Redis: {count} records was added in {sw.ElapsedMilliseconds} ms";

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