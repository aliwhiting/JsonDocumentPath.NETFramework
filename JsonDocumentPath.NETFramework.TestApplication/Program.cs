using System;
using System.Linq;
using System.Text.Json;

namespace JsonDocumentPath.NETFramework.TestApplication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string json = @"{
              ""persons"": [
                {
                  ""name""  : ""John"",
                  ""age"": ""26""
                },
                {
                  ""name""  : ""Jane"",
                  ""age"": ""2""
                }
              ]
            }";

            var models = JsonDocument.Parse(json).RootElement;

            var results = models.SelectElements("$.persons[?(@.age > 3)]").ToList();

            Console.WriteLine(JsonSerializer.Serialize(results.First(), new JsonSerializerOptions { WriteIndented = true }));

        }
    }
}
