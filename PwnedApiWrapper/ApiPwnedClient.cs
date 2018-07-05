using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using PwnedApiWrapper.Shared;

namespace PwnedApiWrapper
{
    /// <summary>
    /// A Pwned Passwords client that makes use of the official API over HTTP.
    /// </summary>
    public class ApiPwnedClient : IPwnedClient
    {
        /// <summary>
        /// Gets the base URL the API is located at.
        /// </summary>
        public string BaseUrl { get; private set; }

        /// <summary>
        /// The WebClient instance used to download data.
        /// </summary>
        private WebClient client;

        /// <summary>
        /// Initializes a new instance of a Pwned Passwords API client.
        /// </summary>
        /// <param name="baseUrl">The base URL of the API.</param>
        public ApiPwnedClient(string baseUrl)
        {
            BaseUrl = baseUrl + (baseUrl.EndsWith("/") ? "" : "/"); // Append trailing slash if needed.
            client = new WebClient();
        }

        /// <summary>
        /// Splits a string by lines.
        /// </summary>
        /// <param name="str">The string to split.</param>
        /// <returns></returns>
        private string[] SplitByLines(string str)
        {
            return str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        /// <summary>
        /// Gets the range of hashes with the given prefix.
        /// </summary>
        /// <param name="hashPrefix">The hash prefix to search.</param>
        /// <returns></returns>
        private Dictionary<string, int> GetRange(string hashPrefix)
        {
            // Check argument.
            if (hashPrefix.Length != 5)
            {
                throw new ArgumentException($"Hash prefix provided must be of length 5, length {hashPrefix.Length} given.");
            }

            // Download range.
            var response = client.DownloadString($"{BaseUrl}/range/{hashPrefix}");

            // Loop through response lines.
            var lines = SplitByLines(response);
            var output = new Dictionary<string, int>();
            for (var i = 0; i < lines.Length; i++)
            {
                // Split along colon, add to output if line is valid.
                var entry = lines[i].Split(':');
                if (entry.Length == 2 && int.TryParse(entry[1], out int count))
                {
                    output.Add(entry[0], count);
                }
            }

            return output;
        }

        public int GetNumberOfAppearances(string password)
        {
            // Compute hash, prefix and suffix.
            var hash = CryptoUtils.Sha1(password);
            var prefix = hash.Substring(0, 5);
            var suffix = hash.Substring(5);

            // Get result from returned range.
            var results = GetRange(prefix);
            return results.ContainsKey(suffix) ? results[suffix] : 0;
        }

        public Dictionary<string, int> GetNumberOfAppearancesForAll(IEnumerable<string> passwords)
        {
            // Query for each password in the collection.
            return passwords.ToDictionary(x => x, x => GetNumberOfAppearances(x));
        }
    }
}
