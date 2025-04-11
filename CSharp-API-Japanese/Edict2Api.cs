using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using CSharp_API_Japanese;

namespace CSharp_API_Japanese
{
    public class EDict2Api : IApiModule
    {
        private readonly EDict2_Reader reader;
        private readonly JsonSerializerOptions jsonOptions;

        public EDict2Api(string filePath)
        {
            reader = new EDict2_Reader(filePath);

            jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
        }

        public void RegisterRoutes(WebApplication app)
        {
            app.MapGet("/", () => "EDICT2 API is running 🈳");

            // GET /lookup?term=猫
            app.MapGet("/lookup", (string word) =>
            {
                Console.WriteLine($"Received lookup for word: {word}");
                var results = reader.LookupDefinitionsBasic(word);
                if (results == null || results.Count == 0)
                    return Results.NotFound();

                return Results.Json(results, jsonOptions);
            });

            // Optional: GET /search?query=... if you later add more advanced filtering
        }
        public void RegisterRoutes2(WebApplication app)
        {
            var group = app.MapGroup("/edict2");

            group.MapGet("/", () => "EDICT2 API is running 🈳");

            group.MapGet("/lookup", (string word) =>
            {
                Console.WriteLine($"Received lookup for word: {word}");

                var results = reader.LookupDefinitionsBasic(word);
                if (results == null || results.Count == 0)
                    return Results.NotFound();

                return Results.Json(results, jsonOptions);
            });
        }
    }
}
