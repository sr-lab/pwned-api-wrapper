using PwnedApiWrapper.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwnedApiWrapper.App
{
    class Program
    {
        /// <summary>
        /// The base URL of the API to query. You probably shouldn't change this.
        /// </summary>
        private const string ApiUrl = "https://api.pwnedpasswords.com";

        /// <summary>
        /// The threshold number of passwords at which to warn the user about API usage.
        /// </summary>
        private const int WarnSizeLimit = 50;
        
        /// <summary>
        /// Gets a valid format at the specified index in the program arguments, or returns a fallback on failure.
        /// </summary>
        /// <param name="args">The arguments passed to the application.</param>
        /// <param name="index">The index at which to look for the format specifier.</param>
        /// <param name="fallback">The fallback format to return in case of failure.</param>
        /// <returns></returns>
        private static string GetFormat(string[] args, int index, string fallback = "plain")
        {
            // Validate format, default to plain.
            var format = fallback;
            var permittedFormats = new[] { "plain", "coq" };
            if (args.Length > index && permittedFormats.Contains(args[index]))
            {
                format = args[5];
            }

            return format;
        }

        /// <summary>
        /// Generates and outputs results for a batch-mode operation.
        /// </summary>
        /// <param name="service">The service to use for the operation.</param>
        /// <param name="passwordsFilename">The passwordsFilename of the passwords file loaded.</param>
        /// <param name="passwords">The passwords to get frequencies for.</param>
        /// <param name="format">The format to output results in.</param>
        private static void GenerateResults(IPwnedClient service, string passwordsFilename, string[] passwords, string format)
        {
            // Query service.
            var results = service.GetNumberOfAppearancesForAll(passwords);

            // Plain output goes straight to console.
            switch (format)
            {
                case "plain":

                    // Plain is just plain CSV.
                    Console.WriteLine("password, appearances");
                    foreach (var entry in results)
                    {
                        Console.WriteLine($"\"{entry.Key.Replace("\"", "\"\"")}\", {entry.Value}");
                    }

                    break;
                case "coq":

                    // Coq lookup format needs placing into template.
                    var output = new StringBuilder();
                    var first = true;
                    foreach (var entry in results)
                    {
                        if (!first)
                        {
                            output.Append(";");
                            output.AppendLine();
                        }
                        output.Append($"  (\"{entry.Key}\", {entry.Value} # 1)");
                        first = false;
                    }

                    // Output template to console with placeholders filled.
                    Console.Write(Properties.Resources.coq_template
                        .Replace("%NAME", Path.GetFileNameWithoutExtension(passwordsFilename) + "_pwned_count")
                        .Replace("%PASSWORDS", output.ToString()));

                    break;
            }
        }

        private static void InteractiveApiMode(string[] args)
        {
            var interactiveService = new ApiPwnedClient(ApiUrl);
            Console.WriteLine("Pwned Passwords Explorer: Interactive/API Mode");
            string buffer;
            do
            {
                // Prompt for password.
                Console.Write("query>");
                buffer = Console.ReadLine();

                // Output result.
                Console.WriteLine(interactiveService.GetNumberOfAppearances(buffer) + " occurrences found.");
            } while (buffer != "exit"); // Read until exit.

            // Say goodbye and quit.
            Console.WriteLine("Bye!");
        }

        private static void InteractiveFileMode(string[] args)
        {
        }

        private static void InteractiveMode(string[] args)
        {
            // TODO: Check args length.
            switch (args[1])
            {
                case "-a": // Interactive/API mode.
                    InteractiveApiMode(args);
                    break;
                case "-f": // Interactive/File mode.
                    InteractiveFileMode(args);
                    break;
            }
        }

        /// <summary>
        /// Runs a whole file of passwords through the Pwned Passwords API.
        /// </summary>
        /// <param name="passwordsFilename">The filename from which the passwords were loaded.</param>
        /// <param name="passwords">The passwords to query the database for.</param>
        /// <param name="args">The arguments passed to the application.</param>
        private static void BatchApiMode(string passwordsFilename, string[] passwords, string[] args)
        {
            // Check mode was specified.
            if (args.Length < 6)
            {
                Console.WriteLine("Mode must be specified (-s or -h).");
                return;
            }

            // Mode select.
            switch (args[5])
            {
                case "-s": // Standard mode.

                    // Get format, default to plain.
                    var format = GetFormat(args, 6);

                    // Initialise service.
                    var service = new ApiPwnedClient(ApiUrl);

                    // Generate results.
                    GenerateResults(service, passwordsFilename, passwords, format);

                    break;
                case "-h": // Frequency-only mode.

                    Console.WriteLine("Frequency-only output is not supported in API mode.");
                    break;
            }
        }

        /// <summary>
        /// Runs a whole file of passwords through a local instance of Pwned Passwords.
        /// </summary>
        /// <param name="passwordsFilename">The filename from which the passwords were loaded.</param>
        /// <param name="passwords">The passwords to query the database for.</param>
        /// <param name="args">The arguments passed to the application.</param>
        private static void BatchFileMode(string passwordsFilename, string[] passwords, string[] args)
        {
            // Check database file path was passed.
            if (args.Length < 5)
            {
                Console.WriteLine("Pwned Passwords database file must be specified.");
                return;
            }

            // Check file exists.
            var pwnedFilename = args[4];
            if (!File.Exists(pwnedFilename))
            {
                Console.WriteLine($"Could not read Pwned Passwords database at '{pwnedFilename}'.");
                return;
            }

            // Check mode was specified.
            if (args.Length < 6)
            {
                Console.WriteLine("Mode must be specified (-s or -h).");
                return;
            }

            // Mode select.
            switch (args[5])
            {
                case "-s": // Standard mode.

                    // Get format, default to plain.
                    var format = GetFormat(args, 6);

                    // Initialise service.
                    var service = new LocalFilePwnedClient(pwnedFilename);

                    // Generate results.
                    GenerateResults(service, passwordsFilename, passwords, format);

                    break;
                case "-h": // Frequency-only mode.
                    
                    // Check limit parses as an integer.
                    if (!int.TryParse(args[6], out var limit))
                    {
                        Console.WriteLine("Invalid limit provided.");
                        return;
                    }

                    // Query file.
                    var frequencyService = new LocalFilePwnedFrequencyExtractor(pwnedFilename);
                    var frequencyResults = frequencyService.GetAbove(limit);

                    // Output goes straight to console.
                    foreach (var entry in frequencyResults)
                    {
                        Console.WriteLine(entry);
                    }

                    break;
            }
        }
        
        /// <summary>
        /// Runs a whole file of passwords through some instance of Pwned Passwords.
        /// </summary>
        /// <param name="args">The arguments passed to the application.</param>
        private static void BatchMode(string[] args)
        {
            // Check database file path was passed.
            if (args.Length < 3)
            {
                Console.WriteLine("Passwords file must be specified.");
                return;
            }

            // Read passwords file in.
            var filename = args[2];
            if (!File.Exists(filename))
            {
                Console.WriteLine($"Could not load passwords file at '{filename}'.");
                return;
            }
            var passwords = FileUtils.ReadFileAsLines(filename);
            
            // Check mode was specified.
            if (args.Length < 4)
            {
                Console.WriteLine("Mode must be specified (-a or -f).");
                return;
            }

            // Batch against API or file?
            switch (args[3])
            {
                case "-a": // API mode.
                    BatchApiMode(filename, passwords, args);
                    break;
                case "-f": // File mode.
                    BatchFileMode(filename, passwords, args);
                    break;
            }
        }

        static void Main(string[] args)
        {
            // We at least need a mode.
            if (args.Length < 1)
            {
                Console.WriteLine("Interactive: App -i");
                Console.WriteLine("Batch: App -b <password_file> <-a | -f <pwned_db>> [format]");
                return;
            }

            switch (args[0])
            {
                case "-i":
                    // Interactive mode.
                    InteractiveMode(args);
                    break;
                case "-b":
                    // Batch mode.
                    BatchMode(args);
                    break;
            }

            //// Read password list. Warn if very long.
            //var passwords = FileUtils.ReadFileAsLines(args[0]);
            //if (passwords.Length > WarnSizeLimit)
            //{
            //    Console.WriteLine($"You're about to throw more than {WarnSizeLimit} passwords at the API at {ApiUrl}. Continue? (Y/n)");
            //    if (Console.ReadKey(true).Key != ConsoleKey.Y)
            //    {
            //        Console.WriteLine("Aborting...");
            //        return;
            //    }
            //}

            //// Initialize client and do the work.
            //var client = new ApiPwnedClient(ApiUrl);
            //var results = new Dictionary<string, int>();
            //for (var i = 0; i < passwords.Length; i++)
            //{
            //    var appearances = client.GetNumberOfAppearances(passwords[i]);
            //    results.Add(passwords[i], appearances);

            //    // Give status to user.
            //    Console.WriteLine($"Password {passwords[i]} found {appearances} times!");
            //}

            //// Plain output goes straight to console.
            //if (format == "plain")
            //{
            //    Console.WriteLine("password, appearances");
            //    foreach (var entry in results)
            //    {
            //        Console.WriteLine($"{entry.Key}, {entry.Value}");
            //    }
            //}
            //else if (format == "coq")
            //{
            //    // Coq format needs placing into template.
            //    var output = new StringBuilder();
            //    var first = true;
            //    foreach (var entry in results)
            //    {
            //        if (!first)
            //        {
            //            output.Append(";");
            //            output.AppendLine();
            //        }
            //        output.Append($"  (\"{entry.Key}\", {entry.Value} # 1)");
            //        first = false;
            //    }

            //    // Output template to console with placeholders filled.
            //    Console.Write(Properties.Resources.coq_template
            //        .Replace("%NAME", Path.GetFileNameWithoutExtension(args[0]) + "_pwned_count")
            //        .Replace("%PASSWORDS", output.ToString()));
            //}
        }
    }
}
