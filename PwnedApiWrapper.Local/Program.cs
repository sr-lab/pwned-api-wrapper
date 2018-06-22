using PwnedApiWrapper.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwnedApiWrapper.Local
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check number of arguments.
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: App <password_list> <pwned_passwords_corpus> [format]");
                Console.WriteLine("Or");
                Console.WriteLine("Usage: App -i <pwned_passwords_corpus>");
                return;
            }

            // Interactive mode.
            if (args.Length == 2 && args[0] == "-i" && File.Exists(args[1]))
            {
                var interactiveService = new PwnedFileWrapper(args[1]);
                Console.WriteLine("Pwned Passwords Explorer: Interactive Mode");
                var buffer = "";
                do
                {
                    // Prompt for password.
                    Console.Write("query>");
                    buffer = Console.ReadLine();

                    // Output result.
                    Console.WriteLine(interactiveService.LookupSingle(buffer) + " occurrences found.");
                } while (buffer != "exit"); // Read until exit.
                
                // Say goodbye and quit.
                Console.WriteLine("Bye!");
                return;
            }

            // Check file exists.
            if (!File.Exists(args[0]) || !File.Exists(args[1]))
            {
                Console.WriteLine("Could not read one or more input file.");
                return;
            }

            // Validate format, default to plain.
            var format = "plain";
            var permittedFormats = new string[] { "plain", "coq" };
            if (args.Length > 2 && permittedFormats.Contains(args[2]))
            {
                format = args[2];
            }

            // Read password list.
            var passwords = FileUtils.ReadFileAsLines(args[0]);
            
            // Query file.
            var service = new PwnedFileWrapper(args[1]);
            var results = service.Lookup(passwords);

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
        }
    }
}
