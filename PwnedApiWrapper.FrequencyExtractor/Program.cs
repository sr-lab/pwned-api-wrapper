using System;
using System.IO;

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

            // Check limit parses as an integer.
            var limit = -1;
            if (!int.TryParse(args[0], out limit))
            {
                Console.WriteLine("Invalid limit provided.");
                return;
            }

            // Check file exists.
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Could not read input file.");
                return;
            }

            // Query file.
            IPwnedFrequencyExtractor service = new LocalFilePwnedFrequencyExtractor(args[1]);
            var results = service.GetAbove(limit);

            // Output goes straight to console.
            foreach (var entry in results)
            {
                Console.WriteLine(entry);
            }
        }
    }
}
