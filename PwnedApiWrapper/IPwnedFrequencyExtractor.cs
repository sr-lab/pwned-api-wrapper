using System.Collections.Generic;

namespace PwnedApiWrapper
{
    /// <summary>
    /// Represents some service capable of querying Pwned Passwords for frequency extraction only.
    /// </summary>
    public interface IPwnedFrequencyExtractor
    {
        /// <summary>
        /// Pulls all frequencies from Pwned Passwords above a limit.
        /// </summary>
        /// <param name="limit">The excluive lower limit of frequencies to extract.</param>
        /// <returns>A list of frequencies.</returns>
        IList<int> GetAbove(int limit);

        /// <summary>
        /// Returns the specified number of the largest frequencies from Pwned Passwords.
        /// </summary>
        /// <param name="count">The number of frequencies to return.</param>
        /// <returns>A list of frequencies.</returns>
        IList<int> GetTop(int count);

        /// <summary>
        /// Gets the total number of passwords in the database.
        /// </summary>
        /// <returns></returns>
        long GetCardinality();
    }
}
