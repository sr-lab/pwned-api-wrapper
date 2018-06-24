using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PwnedApiWrapper.Shared;

namespace PwnedApiWrapper.FrequencyExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check number of arguments.
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: App <limit> <pwned_passwords_corpus>");
                return;
            }

            // Check file exists.
            var limit = -1;
            if (!int.TryParse(args[0], out limit) || !File.Exists(args[1]))
            {
                Console.WriteLine("Invalid arguments.");
                return;
            }

            // Query file.
            var service = new FrequencyExtractorWrapper(args[1]);
            var results = service.Pull(limit);

            // Output goes straight to console.
            foreach (var entry in results)
            {
                Console.WriteLine(entry);
            }
        }
    }
}
