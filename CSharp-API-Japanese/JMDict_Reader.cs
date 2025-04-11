using System.Runtime.Intrinsics.X86;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace CSharp_API_Japanese
{
    public class JMDict_Reader : IDisposable
    {
        public readonly string _filePath;
        private readonly Dictionary<string, JMDictEntry> _records = new();
        public IReadOnlyDictionary<string, JMDictEntry> Records => _records;

        private XDocument _doc;
        private bool _disposed;

        public JMDict_Reader(string xmlFilePath)
        {
            _filePath = xmlFilePath;
            if (!string.IsNullOrWhiteSpace(xmlFilePath) && System.IO.File.Exists(xmlFilePath)) LoadDictionary();
        }

        // Disposal for the JMDict_Reader class | Dispose method to clean up resources
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // Dispose method to clean up resources
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _doc = null;
                //Records = null;
            }

            _disposed = true;
        }
        // Finalizer to clean up resources
        ~JMDict_Reader()
        {
            Dispose(disposing: false);
        }

        public void LoadDictionary()
        {
            // Load entire JMDict Library
            var doc = XDocument.Load(_filePath);
            // Get default XML namespace
            var ns = doc.Root.GetDefaultNamespace();

            // Parse the dictionary and store it in the Records variable
            foreach (var entry in doc.Descendants(ns + "entry"))
            {

                var entSeq = (int?)entry.Element(ns + "ent_seq") ?? 0;

                var kanjiElements = entry.Elements()
                    .Where(e => e.Name.LocalName == "k_ele")
                    .Select(k => new KanjiElement
                    {
                        Keb = k.Elements().FirstOrDefault(e => e.Name.LocalName == "keb")?.Value
                    })
                    .ToList();

                var readingElements = entry.Elements()
                    .Where(e => e.Name.LocalName == "r_ele")
                    .Select(r => new ReadingElement
                    {
                        Reb = r.Elements().FirstOrDefault(e => e.Name.LocalName == "reb")?.Value
                    })
                    .ToList();


                var senseElements = entry.Elements("sense")
                    .Select(s => new Sense
                    {
                        PartOfSpeech = s.Elements("pos").Select(e => (string)e).ToList(),
                        Glosses = s.Elements("gloss")
                            .Select(g => new Gloss
                            {
                                Text = g.Value,
                                Language = (string?)g.Attribute(XNamespace.Xml + "lang") ?? "eng"
                            })
                            .ToList()
                    }).ToList();

                var entryObject = new JMDictEntry
                {
                    EntSeq = entSeq,
                    KanjiElements = kanjiElements,
                    ReadingElements = readingElements,
                    Senses = senseElements
                };

                foreach (var keb in kanjiElements.Select(k => k.Keb).Where(k => k != null))
                {
                    _records[keb] = entryObject;

                }

                foreach (var reb in readingElements.Select(r => r.Reb).Where(r => r != null))
                {
                    _records[reb] = entryObject;

                }


            }

        }

        public List<JMDictEntry> LookupWord(string word)
        {
            var results = new List<JMDictEntry>();

            // Optional fast path if your dict is keyed by readings/kanji
            if (Records.TryGetValue(word, out var exactMatch))
            {
                results.Add(exactMatch);
            }

            foreach (var entry in Records.Values)
            {
                bool matches = false;

                if (entry.KanjiElements != null)
                {
                    foreach (var kEle in entry.KanjiElements)
                    {
                        if (kEle.Keb == word)
                        {
                            matches = true;
                            break;
                        }
                    }
                }

                if (!matches && entry.ReadingElements != null)
                {
                    foreach (var rEle in entry.ReadingElements)
                    {
                        if (rEle.Reb == word)
                        {
                            matches = true;
                            break;
                        }
                    }
                }

                if (matches && !results.Contains(entry))
                {
                    results.Add(entry);
                }
            }

            return results;
        }


    }



    public class JMDictEntry
    {
        [JsonInclude]
        public int EntSeq { get; set; }
        [JsonInclude]
        public List<KanjiElement> KanjiElements { get; set; } = new();
        [JsonInclude]
        public List<ReadingElement> ReadingElements { get; set; } = new();
        [JsonInclude]
        public List<Sense> Senses { get; set; } = new();
    }

    public class KanjiElement
    {
        public string Keb { get; set; }
        public List<string> Priority { get; set; } = new();
    }

    public class ReadingElement
    {
        public string Reb { get; set; }
        public List<string> Priority { get; set; } = new();
    }

    public class Sense
    {
        public List<string> PartOfSpeech { get; set; } = new();
        public List<Gloss> Glosses { get; set; } = new();
        public List<string> Fields { get; set; } = new();
        public List<string> Misc { get; set; } = new();
    }

    public class Gloss
    {
        public string Text { get; set; }
        public string Language { get; set; } = "eng"; // default fallback

    }
}
