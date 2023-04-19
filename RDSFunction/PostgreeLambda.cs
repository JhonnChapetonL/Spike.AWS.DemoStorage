using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Diagnostics;
using Npgsql;
using RDS.Function.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RDSFunction;

public class PostgreeLambda
{
    private static string _connectionString;

    public PostgreeLambda()
    {
        // Construct the connection string from environment variables
        string dbHost = "test.cmild9c2qxrl.us-east-2.rds.amazonaws.com";
        string dbPort = "5432";
        string dbName = "Test";
        string dbUser = "postgres";
        string dbPassword = "T3st1ng$";

        _connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
    }

    public APIGatewayProxyResponse Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var recordsCount = Convert.ToInt32(request.Body);

        var response = InnerHandler(recordsCount);

        return response;
    }

    public static APIGatewayProxyResponse InnerHandler(int count)
    {

        var employees = GenerateRandomEmployees(count);

        var sw = Stopwatch.StartNew();

        LambdaLogger.Log($"PostgreSQL: Executing {count} records...");

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            // Create a command to insert records into the table
            using (var command = new NpgsqlCommand("INSERT INTO Employee (Name, Age, Department) VALUES (@Name, @Age, @Department)", connection))
            {
                // Add parameters to the command
                command.Parameters.AddWithValue("Name", "");
                command.Parameters.AddWithValue("Age", 0);
                command.Parameters.AddWithValue("Department", "");

                // Iterate over the list of records and execute the command for each record
                foreach (var employee in employees)
                {
                    // Set parameter values
                    command.Parameters["Name"].Value = employee.Name;
                    command.Parameters["Age"].Value = employee.Age;
                    command.Parameters["Department"].Value = employee.Department;

                    // Execute the command
                    command.ExecuteNonQuery();
                }
            }
        }

        sw.Stop();

        var result = $"PostgreSQL: {count} records was added in {sw.ElapsedMilliseconds} ms";

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
                //Id = i + 1,
                Name = "Employee " + (i + 1),
                Age = random.Next(18, 65),
                Department = "Department " + random.Next(1, 6)
            };

            employees.Add(employee);
        }

        return employees;
    }
}