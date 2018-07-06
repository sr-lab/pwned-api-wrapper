using System.Collections.Generic;

namespace PwnedApiWrapper
{
    /// <summary>
    /// Represents some service capable of querying Pwned Passwords.
    /// </summary>
    public interface IPwnedClient
    {
        /// <summary>
        /// Gets the number of times the given password appears according to the service.
        /// </summary>
        /// <param name="password">The password to look up.</param>
        /// <returns></returns>
        int GetNumberOfAppearances(string password);

        /// <summary>
        /// Gets the number of times each given password appears according to the service.
        /// </summary>
        /// <param name="passwords">The passwords to look up.</param>
        /// <returns></returns>
        Dictionary<string, int> GetNumberOfAppearancesForAll(IEnumerable<string> passwords);

        /// <summary>
        /// Gets the prompt to show for the service when in interactive mode.
        /// </summary>
        /// <returns></returns>
        string GetInteractiveModePrompt();
    }
}
