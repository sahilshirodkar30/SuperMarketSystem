namespace SuperMarketSystem.Server.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public decimal Salary { get; set; }
        public string? ImageUrl { get; set; }
    }
}
