using System.Text.Json;

namespace CSharp_API_Japanese
{
    public class JMDictApi : IApiModule
    {
        private JMDict_Reader reader;
        private JsonSerializerOptions jsonOptions;

        public JMDictApi(string filePath)
        {
            reader = new JMDict_Reader(filePath);
            //reader.LoadDictionary();

            jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
        }

        public void RegisterRoutes(WebApplication app)
        {
            app.MapGet("/", () => "JMdict API is running 🚀");

            app.MapGet("/lookup", (string word) =>
            {
                Console.WriteLine($"Received JMDict lookup for word: {word}");
                if (reader.Records.TryGetValue(word, out var entry))
                    return Results.Json(entry, jsonOptions);
                return Results.NotFound();
            });

            app.MapGet("/search", (string query, bool kanjiOnly = false) =>
            {
                Console.WriteLine($"Received lookup for word: {query}");
                var results = reader.LookupWord(query)
                    .Select(r => new
                    {
                        entSeq = r.EntSeq,
                        kebs = r.KanjiElements.Select(k => k.Keb),
                        rebs = r.ReadingElements.Select(r => r.Reb),
                        glosses = r.Senses.SelectMany(s => s.Glosses).Where(g => g.Language == "eng").Select(g => g.Text)
                    });

                return Results.Json(results, jsonOptions);
            });

            app.MapGet("/entry/{entSeq}", (int entSeq) =>
            {
                var match = reader.Records.Values.FirstOrDefault(e => e.EntSeq == entSeq);
                return match != null ? Results.Json(match, jsonOptions) : Results.NotFound();
            });
        }

        public void RegisterRoutes2(WebApplication app)
        {
            var group = app.MapGroup("/jmdict");

            group.MapGet("/", () => "JMdict API is running 🚀");

            group.MapGet("/lookup", (string word) =>
            {
                if (reader.Records.TryGetValue(word, out var entry))
                    return Results.Json(entry, jsonOptions);

                return Results.NotFound();
            });

            group.MapGet("/search", (string query, bool kanjiOnly = false) =>
            {
                Console.WriteLine($"Received lookup for word: {query}");

                var results = reader.LookupWord(query)
                    .Select(r => new
                    {
                        entSeq = r.EntSeq,
                        kebs = r.KanjiElements.Select(k => k.Keb),
                        rebs = r.ReadingElements.Select(r => r.Reb),
                        glosses = r.Senses
                                  .SelectMany(s => s.Glosses)
                                  .Where(g => g.Language == "eng")
                                  .Select(g => g.Text)
                    });

                return Results.Json(results, jsonOptions);
            });

            group.MapGet("/entry/{entSeq}", (int entSeq) =>
            {
                var match = reader.Records.Values.FirstOrDefault(e => e.EntSeq == entSeq);
                return match != null ? Results.Json(match, jsonOptions) : Results.NotFound();
            });
        }
    }

}
