using PwnedApiWrapper.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwnedApiWrapper.Local
{
    /// <summary>
    /// Represents a wrapper around the Pwned Passwords corpus file.
    /// </summary>
    class PwnedFileWrapper
    {
        /// <summary>
        /// Gets the path this class is wrapping.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Initialises a new instance of a wrapper around the Pwned Passwords corpus file.
        /// </summary>
        public PwnedFileWrapper(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Computes the SHA-1 hash of a given array of passwords.
        /// </summary>
        /// <param name="passwords">The passwords to compute hashes for.</param>
        /// <param name="lower">Whether or not to compute lowercase hashes.</param>
        /// <returns>A dictionary of password hashes against plaintext passwords.</returns>
        private static Dictionary<string, string> Sha1Many(string[] passwords, bool lower = false)
        {
            var output = new Dictionary<string, string>();
            for (int i = 0; i < passwords.Length; i++)
            {
                output.Add(CryptoUtils.Sha1(passwords[i], lower), passwords[i]);
            }
            return output;
        }

        /// <summary>
        /// Looks up an array of passwords in the file, in one pass.
        /// </summary>
        /// <param name="passwords">The passwords to look up.</param>
        /// <returns>A dictionary of passwords against counts.</returns>
        public Dictionary<string, int> Lookup(string[] passwords)
        {
            // Pre-hash passwords.
            var hashes = Sha1Many(passwords);

            // Prepare output dictionary.
            var output = new Dictionary<string, int>();
            for (int i = 0; i < passwords.Length; i++)
            {
                output.Add(passwords[i], 0);
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
    }
}
