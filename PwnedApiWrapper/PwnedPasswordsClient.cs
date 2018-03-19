using PwnedApiWrapper.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PwnedApiWrapper
{
    /// <summary>
    /// Represents a Pwned Passwords API client.
    /// </summary>
    public class PwnedPasswordsClient
    {
        /// <summary>
        /// The base URL the API is located at.
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
        public PwnedPasswordsClient(string baseUrl)
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
        /// 
        /// </summary>
        /// <param name="hashPrefix"></param>
        /// <returns></returns>
        public Dictionary<string, int> GetRange(string hashPrefix)
        {
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

        /// <summary>
        /// Gets the number of times the given password appears according to the service.
        /// </summary>
        /// <param name="password">The password to look up.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the number of times the given password appears according to the service.
        /// </summary>
        /// <param name="password">The password to look up.</param>
        /// <returns></returns>
        [Obsolete("This method is to be removed in a future version of the API. Use GetNumberOfAppearances instead.")]
        public int GetNumberOfAppearancesUsingEndpoint(string password)
        {
            try
            {
                // Download number of occurences.
                var response = client.DownloadString($"{BaseUrl}/pwnedpassword/{Uri.EscapeDataString(password)}");
                return int.Parse(response);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return 0; // We get a 404 if the password was not found.
                    }
                }
                throw; // There was some other exception. Throw it.
            }
        }
    }
}
