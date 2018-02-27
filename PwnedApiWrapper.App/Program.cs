using System;
using System.Collections.Generic;
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
        /// Reads a file as lines, returning it as an array of strings.
        /// </summary>
        /// <param name="filename">The filename of the file to read.</param>
        /// <returns></returns>
        private static string[] ReadFileAsLines(string filename)
        {
            return File.ReadAllText(filename)
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        static void Main(string[] args)
        {
            // Check number of arguments.
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: App <password_list> [format]");
                return;
            }

            // Check file exists.
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Could not read input file.");
                return;
            }

            // Validate format, default to plain.
            var format = "plain";
            var permittedFormats = new string[] { "plain", "coq" };
            if (args.Length > 1 && permittedFormats.Contains(args[1]))
            {
                format = args[1];
            }

            // Read password list. Warn if very long.
            var passwords = ReadFileAsLines(args[0]);
            if (passwords.Length > WarnSizeLimit)
            {
                Console.WriteLine($"You're about to throw more than {WarnSizeLimit} passwords at the API at {ApiUrl}. Continue? (Y/n)");
                if (Console.ReadKey(true).Key != ConsoleKey.Y)
                {
                    Console.WriteLine("Aborting...");
                    return;
                }
            }

            // Initialize client and do the work.
            var client = new PwnedPasswordsClient(ApiUrl);
            var results = new Dictionary<string, int>();
            for (var i = 0; i < passwords.Length; i++)
            {
                var appearances = client.GetNumberOfAppearances(passwords[i]);
                results.Add(passwords[i], appearances);

                // Give status to user.
                Console.WriteLine($"Password {passwords[i]} found {appearances} times!");
            }

            // Plain output goes straight to console.
            if (format == "plain")
            {
                Console.WriteLine("password, appearances");
                foreach (var entry in results)
                {
                    Console.WriteLine($"{entry.Key}, {entry.Value}");
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
                        output.Append(",");
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
        }
    }
}
