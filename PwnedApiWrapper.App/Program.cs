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

        private static void BatchApiMode(string[] passwords, string[] args)
        {

        }

        private static void BatchFileMode(string[] passwords, string[] args)
        {
            // Check file exists.
            if (!File.Exists(args[4]))
            {
                Console.WriteLine($"Could not read input file {args[3]}.");
                return;
            }

            // Mode select.
            switch (args[5])
            {
                case "-s": // Standard mode.

                    // Get format, default to plain.
                    var format = GetFormat(args, 6);

                    // Query file.
                    var service = new LocalFilePwnedClient(args[1]);
                    var results = service.GetNumberOfAppearancesForAll(passwords);

                    // Plain output goes straight to console.
                    if (format == "plain")
                    {
                        Console.WriteLine("password, appearances");
                        foreach (var entry in results)
                        {
                            Console.WriteLine($"\"{entry.Key.Replace("\"", "\"\"")}\", {entry.Value}");
                        }
                    }
                    else if (format == "coq")
                    {
                        // Coq format needs placing into template.
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
                            .Replace("%NAME", Path.GetFileNameWithoutExtension(args[0]) + "_pwned_count")
                            .Replace("%PASSWORDS", output.ToString()));
                    }

                    break;
                case "-f": // Frequency-only mode.


                    break;
            }
        }

        private static void BatchMode(string[] args)
        {
            // Read passwords file in.
            if (!File.Exists(args[2]))
            {
                Console.WriteLine($"Could not load passwords file {args[2]}.");
                return;
            }
            var passwords = FileUtils.ReadFileAsLines(args[2]);

            // Batch against API or file?
            switch (args[3])
            {
                case "-a": // API mode.
                    BatchApiMode(passwords, args);
                    break;
                case "-f": // File mode.
                    BatchFileMode(passwords, args);
                    break;
            }
        }

        static void Main(string[] args)
        {
            //// We at least need a password list.
            //if (args.Length < 1)
            //{
            //    Console.WriteLine("Usage: App <password_list> [format]");
            //    return;
            //}

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
