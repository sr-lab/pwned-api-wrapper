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
            return passwords.ToDictionary(x => x, x => CryptoUtils.Sha1(x, lower));
        }

        public int GetNumberOfAppearances(string password)
        {
            // More efficient in this case to pass single password out to many-password method.
            return GetNumberOfAppearancesForAll(new[] { password })[password];
        }

        public Dictionary<string, int> GetNumberOfAppearancesForAll(IEnumerable<string> passwords)
        {
            // Pre-hash passwords.
            var hashes = Sha1Many(passwords);

            // Prepare output dictionary.
            var output = new Dictionary<string, int>();
            foreach (var password in passwords)
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
                        output[hashes[splitter[0]]] = int.Parse(splitter[1]);
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
