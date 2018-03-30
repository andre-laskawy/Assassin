///-----------------------------------------------------------------
///   File:     IWebServerService.cs
///   Author:   Andre Laskawy           
///   Date:	    21.01.2018 14:29:56
///   Revision History: 
///   Name:  Andre Laskawy         Date:  21.01.2018 14:29:56      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Models.Api
{
    using Assassin.Common.Enumerations;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="IWebServerService{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWebServerService<T> where T : WebPackage
    {
        /// <summary>
        /// Gets or sets the on notify.
        /// </summary>
        /// <value>
        /// The on notify.
        /// </value>
        Action<object, string, LogLevel> OnNotify { get; set; }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Gets or sets the on streaming.
        /// </summary>
        Func<T, IStream, Task> OnStreaming { get; set; }

        /// <summary>
        /// Gets or sets the on fetch.
        /// </summary>
        Func<WebPackage, Task<WebPackage>> OnFetch { get; set; }

        /// <summary>
        /// Gets or sets the on authentication function.
        /// </summary>
        Func<WebPackage, Task<WebPackage>> OnAuthenticate { get; set; }
    }
}
