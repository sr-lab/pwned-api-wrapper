using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using PwnedApiWrapper.Shared;

namespace PwnedApiWrapper
{
    /// <summary>
    /// A Pwned Passwords client that makes use of a local database file.
    /// </summary>
    public class LocalFilePwnedClient : IPwnedClient
    {
        /// <summary>
        /// Gets the path this class is wrapping.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Initialises a new instance of a wrapper around the Pwned Passwords corpus file.
        /// </summary>
        public LocalFilePwnedClient(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Computes the SHA-1 hash of a given array of passwords.
        /// </summary>
        /// <param name="passwords">The passwords to compute hashes for.</param>
        /// <param name="lower">Whether or not to compute lowercase hashes.</param>
        /// <returns>A dictionary of password hashes against plaintext passwords.</returns>
        private static Dictionary<string, string> Sha1Many(IEnumerable<string> passwords, bool lower = false)
        {
            return passwords.ToDictionary(x => CryptoUtils.Sha1(x, lower), x => x);
        }

        public int GetNumberOfAppearances(string password)
        {
            // More efficient in this case to pass single password out to many-password method.
            return GetNumberOfAppearancesForAll(new[] { password })[password];
        }

        public Dictionary<string, int> GetNumberOfAppearancesForAll(IEnumerable<string> passwords)
        {
            // Deduplicate and create cloned list for optimisation.
            var distinct = passwords.Distinct();
            var remaining = new List<string>(distinct);

            // Pre-hash passwords.
            var hashes = Sha1Many(distinct);

            // Prepare output dictionary.
            var output = new Dictionary<string, int>();
            foreach (var password in distinct)
            {
                if (!output.ContainsKey(password))
                {
                    output.Add(password, 0);
                }
            }

            // Read file one line at a time, has to be buffered, file too big otherwise.
            using (var reader = new BufferedStream(new FileStream(Path, FileMode.Open)))
            {
                // Lines are 63 bytes long (including line endings, which are Windows-style).
                byte[] buffer = new byte[63];
                while (reader.Read(buffer, 0, buffer.Length) != 0)
                {
                    // Decode and split along colon.
                    var line = Encoding.ASCII.GetString(buffer);
                    var splitter = line.Trim().Split(':');
                    if (splitter.Length == 2 && hashes.ContainsKey(splitter[0]))
                    {
                        // Add count to output if found.
                        var plaintext = hashes[splitter[0]];
                        output[plaintext] = int.Parse(splitter[1]);

                        // Optimisation to break out if we've found all passwords.
                        remaining.Remove(plaintext);
                        if (remaining.Count == 0)
                        {
                            break;
                        }
                    }
                }
            }
            return output;
        }

        public string GetInteractiveModePrompt()
        {
            return $"LocalFile@{Path}";
        }
    }
}
