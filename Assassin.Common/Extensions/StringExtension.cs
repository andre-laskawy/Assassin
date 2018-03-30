///-----------------------------------------------------------------
///   File:     StringExtension.cs
///   Author:   Andre Laskawy           
///   Date:	    21.01.2018 13:04:56
///   Revision History: 
///   Name:  Andre Laskawy         Date:  21.01.2018 13:04:56      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Common
{
    using System;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="StringExtension" />
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// To the enum.
        /// </summary>
        /// <typeparam name="T">any kind of enum</typeparam>
        /// <param name="enumString">The <see cref="string" /></param>
        /// <returns>
        /// The <see cref="T" />
        /// </returns>
        /// <exception cref="Exception">Could not convert " + enumString + " to enum of type: " + typeof(T)</exception>
        public static T ToEnum<T>(this string enumString) where T : struct
        {
            T result = default(T);

            try
            {
                result = (T)Enum.Parse(typeof(T), enumString, true);
            }
            catch
            {
                throw new Exception("Could not convert " + enumString + " to enum of type: " + typeof(T));
            }

            return result;
        }

        /// <summary>
        /// The GetIpEndpoint
        /// </summary>
        /// <param name="address">The <see cref="string"/></param>
        /// <returns>The <see cref="IPEndPoint"/></returns>
        public static IPEndPoint ToIpEndpoint(this string address)
        {
            IPEndPoint result = null;
            string errorMessage = string.Empty;

            Task.Run(async () =>
           {
               try
               {
                   Uri url;
                   IPAddress ip;
                   if (Uri.TryCreate(String.Format("http://{0}", address), UriKind.Absolute, out url) &&
                      IPAddress.TryParse(url.Host, out ip))
                   {
                       result = new IPEndPoint(ip, url.Port);
                   }

                   IPAddress[] IpInHostAddress = await Dns.GetHostAddressesAsync(url.Host);
                   result = new IPEndPoint(IpInHostAddress[0].MapToIPv4(), url.Port);
               }
               catch
               {
                   errorMessage = "Address is not valid or could not be found: " + address;
               }
           });

            while (result == null && string.IsNullOrEmpty(errorMessage))
            {
                Task.Delay(5).Wait();
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new Exception("Address is not valid or could not be found: " + address);
            }

            return result;
        }
    }
}
