using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PwnedApiWrapper.Shared
{
    public class CryptoUtils
    {
        /// <summary>
        /// Returns the SHA-1 hash of the given password as a hexadecimal string.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="lower">If true, the hash will be returned in lowercase.</param>
        /// <returns></returns>
        public static string Sha1(string password, bool lower = false)
        {
            using (var sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString((lower ? "x" : "X") + "2"));
                }

                return sb.ToString();
            }
        }
    }
}
