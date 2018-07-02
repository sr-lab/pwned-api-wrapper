using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PwnedApiWrapper.FrequencyExtractor
{
    /// <summary>
    /// Represents a wrapper around the Pwned Passwords corpus file for extracting frequencies only.
    /// </summary>
    class FrequencyExtractorWrapper
    {
        /// <summary>
        /// Gets the path this class is wrapping.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Initialises a new instance of a wrapper around the Pwned Passwords corpus file for extracting frequencies only.
        /// </summary>
        public FrequencyExtractorWrapper(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Pulls all frequencies from the file above a limit.
        /// </summary>
        /// <param name="limit">The lower limit of frequencies to extract.</param>
        /// <returns>A list of frequencies.</returns>
        public List<int> Pull(int limit)
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
                    if (x >= limit)
                    {
                        output.Add(x);
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Returns the largest frequencies.
        /// </summary>
        /// <param name="count">The number of frequencies to return.</param>
        /// <returns>A list of frequencies.</returns>
        public List<int> PullTop(int count)
        {
            // Get all, sort and take.
            return Pull(int.MaxValue)
                .OrderByDescending(x => x)
                .Take(count)
                .ToList();
        }
    }
}
