using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        /// <param name="baseUrl"></param>
        public PwnedPasswordsClient(string baseUrl)
        {
            BaseUrl = baseUrl + (baseUrl.EndsWith("/") ? "" : "/"); // Append trailing slash if needed.
            client = new WebClient();
        }

        /// <summary>
        /// Gets the number of times the given password appears in the 
        /// </summary>
        /// <param name="password">The password to look up.</param>
        /// <returns></returns>
        public int GetNumberOfAppearances(string password)
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
