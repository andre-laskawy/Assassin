///-----------------------------------------------------------------
///   File:     IWebClientService.cs
///   Author:   Andre Laskawy           
///   Date:	    21.01.2018 14:30:17
///   Revision History: 
///   Name:  Andre Laskawy         Date:  21.01.2018 14:30:17      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Models.Api
{
    using Assassin.Common.Enumerations;
    using Assassin.Common.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="IWebClientService{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWebClientService<T> where T : WebPackage
    {
        /// <summary>
        /// Gets or sets the on notify.
        /// </summary>
        /// <value>
        /// The on notify.
        /// </value>
        Action<string, string, LogLevel> OnNotify { get; set; }

        /// <summary>
        /// Gets or sets the on connected.
        /// </summary>
        /// <value>
        /// The on connected.
        /// </value>
        Action OnConnected { get; set; }

        /// <summary>
        /// Gets or sets the on disconnected.
        /// </summary>
        /// <value>
        /// The on disconnected.
        /// </value>
        Action OnDisconnected { get; set; }

        /// <summary>
        /// Gets or sets the stream.
        /// </summary>
        IStream Stream { get; set; }

        /// <summary>
        /// Connects the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tryReconnect">if set to <c>true</c> [try reconnect].</param>
        /// <returns></returns>
        Task<bool> Connect(string id, bool tryReconnect);

        /// <summary>
        /// Authenticates the specified user.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        Task<AuthenticationData> Authenticate(AuthenticationData data);

        /// <summary>
        /// Disconnets this instance.
        /// </summary>
        /// <returns></returns>
        Task Disconnect();

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        WebPackage Send(WebPackage message);

        /// <summary>
        /// Fetches the specified requested types.
        /// </summary>
        /// <param name="requestedTypes">The requested types.</param>
        /// <param name="query">The query.</param>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        Task<List<Guid>> GetIdList(string requestedType, string query, int timeout = 60, int maxItems = 9999);

        /// <summary>
        /// Fetches the specified requested types.
        /// </summary>
        /// <param name="requestedTypes">The requested types.</param>
        /// <param name="query">The query.</param>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        Task<List<BaseModel>> Fetch(string requestedType, string query, bool includeAll = true, int timeout = 60);

        /// <summary>
        /// Fetches the specified requested types.
        /// </summary>
        /// <param name="requestedTypes">The requested types.</param>
        /// <param name="takeCount">The take count.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        Task<List<BaseModel>> Fetch(string requestedType, int takeCount, int skip, string query, bool includeAll = true, int timeout = 60);

        /// <summary>
        /// Fetches the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        Task<WebPackage> Fetch(WebPackage package);
    }
}
