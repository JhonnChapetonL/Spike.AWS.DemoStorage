namespace Contact.API.Models
{
    public class Result
    {
        public string ExecutionTime { get; set; }
        public Body Body { get; set; }
    }

    public class Body
    {
        public int Total { get; set; }
        public List<Employee> Employees { get; set; }
    }
}