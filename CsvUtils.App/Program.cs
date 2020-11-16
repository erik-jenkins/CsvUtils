using System;
using CsvUtils.Lib;
using System.IO;

namespace CsvUtils.App
{
    static class Program
    {
        static void Main()
        {
            using var reader = new CsvReader();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "data", "employees.csv");
            var employees = reader.ReadCsv<Employee>(path);
            employees.ForEach(employee => Console.WriteLine(employee.ToString()));
        }
    }
}
