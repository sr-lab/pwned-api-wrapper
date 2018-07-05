using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PwnedApiWrapper
{
    /// <summary>
    /// A Pwned Passwords client that makes use of a local database file for extracting frequencies only.
    /// </summary>
    public class LocalFilePwnedFrequencyExtractor : IPwnedFrequencyExtractor
    {
        /// <summary>
        /// Gets the path this class is wrapping.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Initialises a new instance of a Pwned Passwords client that makes use of a local database file for extracting frequencies only.
        /// </summary>
        public LocalFilePwnedFrequencyExtractor(string path)
        {
            Path = path;
        }
        
        public IList<int> GetAbove(int limit)
        {
            // Prepare output dictionary.
            var output = new List<int>();

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

                    // Add count to output if above limit.
                    var x = int.Parse(splitter[1]);
                    if (x > limit)
                    {
                        output.Add(x);
                    }
                }
            }
            return output;
        }
        
        public IList<int> GetTop(int count)
        {
            // Get all, sort and take.
            return GetAbove(int.MinValue)
                .OrderByDescending(x => x)
                .Take(count)
                .ToList();
        }
    }
}
