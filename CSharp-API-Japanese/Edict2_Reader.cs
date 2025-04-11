
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSharp_API_Japanese
{
    public class EDict2_Reader : IDisposable
    {
        private List<string> Records;
        private bool _disposed;

        public EDict2_Reader(string filePath)
        {
            Records = new List<string>();
            LoadDictionary(filePath);
        }

        public void LoadDictionary(string filePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding eucJp = Encoding.GetEncoding("euc-jp");

            Records = File.ReadAllLines(filePath, eucJp)
                          .Where(line => !line.StartsWith("#")) // skip comments
                          .ToList();

            Console.WriteLine($"Loaded {Records.Count} entries from EDICT2.");
            Console.WriteLine($"Sample entry: {Records.FirstOrDefault()}");
        }

        public List<string> LookupDefinitionsBasic(string term)
        {
            return Records
                .Where(line => line.Contains(term))
                .ToList();

        }

        // Disposal for the JMDict_Reader class
        // Dispose method to clean up resources
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
                //Records = null;
            }

            _disposed = true;
        }
        // Finalizer to clean up resources
        ~EDict2_Reader()
        {
            Dispose(disposing: false);
        }


    }

}
