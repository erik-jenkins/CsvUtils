using System.Text.Json;
using System.Text.Json.Serialization;

namespace CsvUtils.App
{
    public enum Gender
    {
        Male,
        Female
    }

    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Gender Gender { get; set; }
        public string IpAddress { get; set; }
        public double ExampleDecimal { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}