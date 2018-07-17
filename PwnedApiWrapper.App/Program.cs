using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using PwnedApiWrapper.Shared;

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
        /// Gets the version string for the application.
        /// </summary>
        /// <returns></returns>
        private static string GetVersionString()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.FileVersion;
        }

        /// <summary>
        /// Prints the help card for the application.
        /// </summary>
        private static void PrintHelp()
        {
            // Write text file to console.
            Console.WriteLine(Properties.Resources.help_card.Replace("%VERSION", GetVersionString()));
        }

        /// <summary>
        /// Gets a valid format at the specified index in the program arguments, or returns a fallback on failure.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
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
                format = args[index];
            }

            return format;
        }

        /// <summary>
        /// Generates and outputs results for a batch-mode operation.
        /// </summary>
        /// <param name="service">The service to use for the operation.</param>
        /// <param name="passwords">The passwords to get frequencies for.</param>
        /// <param name="format">The format to output results in.</param>
        private static void GenerateResults(IPwnedClient service, string[] passwords, string format)
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
                        .Replace("%NAME", "pwned_passwords_output")
                        .Replace("%PASSWORDS", output.ToString()));

                    break;
            }
        }

        /// <summary>
        /// Launches interactive mode with the specified client.
        /// </summary>
        /// <param name="client">The client to use.</param>
        private static void LaunchInteractiveMode(IPwnedClient client)
        {
            Console.WriteLine($"Interactive mode launched against service: {client.GetInteractiveModePrompt()}");
            string buffer;
            do
            {
                // Prompt for password.
                Console.Write($"{client.GetInteractiveModePrompt()}> ");
                buffer = Console.ReadLine();

                // Output result.
                Console.WriteLine(client.GetNumberOfAppearances(buffer) + " occurrences found.");
            } while (buffer != "exit"); // Read until exit.

            // Say goodbye and quit.
            Console.WriteLine("Bye!");
        }

        /// <summary>
        /// Launches interactive mode against the Pwned Passwords API.
        /// </summary>
        private static void InteractiveApiMode()
        {
            // Launch interactive mode.
            LaunchInteractiveMode(new ApiPwnedClient(ApiUrl));
        }

        /// <summary>
        /// Launches interactive mode against a local instance of Pwned Passwords.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        private static void InteractiveFileMode(string[] args)
        {
            // Check database file path was passed.
            if (args.Length < 3)
            {
                Console.WriteLine("Pwned Passwords database file must be specified.");
                return;
            }

            // Check file exists.
            var filename = args[2];
            if (!File.Exists(filename))
            {
                Console.WriteLine($"Could not read Pwned Passwords database at '{filename}'.");
                return;
            }

            // Launch interactive mode.
            Console.WriteLine("Local interactive mode will be *slow* for rare passwords. Local batch mode is much more efficient.");
            LaunchInteractiveMode(new LocalFilePwnedClient(filename));
        }

        /// <summary>
        /// Launches interactive mode against some instance of Pwned Passwords.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        private static void InteractiveMode(string[] args)
        {
            // Check mode was specified.
            if (args.Length < 2)
            {
                Console.WriteLine("Mode must be specified (-a or -f).");
                return;
            }

            // Interactive against API or file?
            switch (args[1])
            {
                case "-a": // Interactive/API mode.
                    InteractiveApiMode();
                    break;
                case "-f": // Interactive/File mode.
                    InteractiveFileMode(args);
                    break;
            }
        }

        /// <summary>
        /// Runs a whole file of passwords through the Pwned Passwords API.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <param name="passwords">The passwords to query the database for.</param>
        private static void BatchApiMode(string[] args, string[] passwords)
        {
            // Check mode was specified.
            if (args.Length < 4)
            {
                Console.WriteLine("Mode must be specified (-s or -h).");
                return;
            }

            // Read password list. Warn if very long.
            if (passwords.Length > WarnSizeLimit)
            {
                Console.WriteLine($"You're about to throw more than {WarnSizeLimit} passwords at the API at {ApiUrl}. Continue? (Y/n)");
                if (Console.ReadKey(true).Key != ConsoleKey.Y)
                {
                    Console.WriteLine("Aborting...");
                    return;
                }
            }

            // Mode select.
            switch (args[3])
            {
                case "-s": // Standard mode.

                    // Get format, default to plain.
                    var format = GetFormat(args, 4);

                    // Initialise service.
                    var service = new ApiPwnedClient(ApiUrl);

                    // Generate results.
                    GenerateResults(service, passwords, format);

                    break;
                case "-h": // Frequency-only mode.

                    Console.WriteLine("Frequency-only output is not supported in API mode.");
                    break;
            }
        }

        /// <summary>
        /// Runs a whole file of passwords through a local instance of Pwned Passwords.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <param name="passwords">The passwords to query the database for.</param>
        private static void BatchFileMode(string[] args, string[] passwords)
        {
            // Check database file path was passed.
            if (args.Length < 4)
            {
                Console.WriteLine("Pwned Passwords database file must be specified.");
                return;
            }

            // Check file exists.
            var filename = args[3];
            if (!File.Exists(filename))
            {
                Console.WriteLine($"Could not read Pwned Passwords database at '{filename}'.");
                return;
            }
            
            // Get format, default to plain.
            var format = GetFormat(args, 4);

            // Initialise service.
            var service = new LocalFilePwnedClient(filename);

            // Generate results.
            GenerateResults(service, passwords, format);
        }

        /// <summary>
        /// Runs a whole file of passwords through some instance of Pwned Passwords.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        private static void BatchMode(string[] args)
        {
            // Check passwords file path was passed.
            if (args.Length < 2)
            {
                Console.WriteLine("Passwords file must be specified.");
                return;
            }

            // Read passwords file in.
            var filename = args[1];
            if (!File.Exists(filename))
            {
                Console.WriteLine($"Could not load passwords file at '{filename}'.");
                return;
            }
            var passwords = FileUtils.ReadFileAsLines(filename).Where(x => x.Length > 0).ToArray(); // Remove blank lines.
            
            // Check mode was specified.
            if (args.Length < 3)
            {
                Console.WriteLine("Mode must be specified (-a or -f).");
                return;
            }

            // Batch against API or file?
            switch (args[2])
            {
                case "-a": // API mode.
                    BatchApiMode(args, passwords);
                    break;
                case "-f": // File mode.
                    BatchFileMode(args, passwords);
                    break;
            }
        }

        /// <summary>
        /// Counts the total number of records in a local instance of Pwned Passwords.
        /// </summary>
        /// <param name="args">The command-line argments passed to the application.</param>
        private static void CardinalityMode(string[] args)
        {
            // Check database file path was passed.
            if (args.Length < 2)
            {
                Console.WriteLine("Pwned Passwords database file must be specified.");
                return;
            }

            // Check file exists.
            var filename = args[1];
            if (!File.Exists(filename))
            {
                Console.WriteLine($"Could not read Pwned Passwords database at '{filename}'.");
                return;
            }

            // Pass output back to user.
            var service = new LocalFilePwnedFrequencyExtractor(filename);
            Console.WriteLine($"Computing cardinality of database at '{filename}'...");
            Console.WriteLine($"{service.GetCardinality()} individual passwords found.");
        }

        /// <summary>
        /// Retrieves frequencies from a local instance of Pwned Passwords.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        private static void FrequencyMode(string[] args)
        {
            // Check database file path was passed.
            if (args.Length < 2)
            {
                Console.WriteLine("Pwned Passwords database file must be specified.");
                return;
            }

            // Check file exists.
            var filename = args[1];
            if (!File.Exists(filename))
            {
                Console.WriteLine($"Could not read Pwned Passwords database at '{filename}'.");
                return;
            }

            // Check query type was specified.
            if (args.Length < 3)
            {
                Console.WriteLine("Query type must be specified (-l or -t).");
                return;
            }

            // Get ready to query file.
            var service = new LocalFilePwnedFrequencyExtractor(filename);
            IList<int> results = new List<int>();

            // Switch on query type.
            switch (args[2])
            {
                case "-l":

                    // Check limit was specified.
                    if (args.Length < 4)
                    {
                        Console.WriteLine("Limit must be specified.");
                        return;
                    }

                    // Check limit parses as an integer.
                    if (!int.TryParse(args[3], out var limit))
                    {
                        Console.WriteLine("Invalid limit provided.");
                        return;
                    }

                    results = service.GetAbove(limit); // Get frequencies above `limit`.
                    break;
                case "-t":

                    // Check count was specified.
                    if (args.Length < 4)
                    {
                        Console.WriteLine("Count must be specified.");
                        return;
                    }

                    // Check count parses as an integer.
                    if (!int.TryParse(args[3], out var count))
                    {
                        Console.WriteLine("Invalid count provided.");
                        return;
                    }

                    results = service.GetTop(count); // Get `count` top frequencies.
                    break;
                default:

                    // Invalid query type specified.
                    Console.WriteLine("Invalid query type specified (Use -h for help).");
                    break;
            }

            // Output goes straight to console.
            foreach (var entry in results)
            {
                Console.WriteLine(entry);
            }
        }

        static void Main(string[] args)
        {
            // We at least need a mode.
            if (args.Length < 1)
            {
                Console.WriteLine("Use -h for help.");
                return;
            }

            // Interactive, batch or frequency-only mode?
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
                case "-c": 

                    // Frequency only mode.
                    FrequencyMode(args);
                    break;
                case "-n":

                    //Cardinality mode.
                    CardinalityMode(args);
                    break;
                case "-h":

                    // Show help.
                    PrintHelp();
                    break;
                default:

                    // Invalid mode specified.
                    Console.WriteLine("Invalid mode specified (Use -h for help).");
                    break;
            }
        }
    }
}
