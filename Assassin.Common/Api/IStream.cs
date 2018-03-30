//-----------------------------------------------------------------------------------------------------------
// <copyright file="istream.cs" company="Miltenyi Biotec GmbH">
//  Copyright (c) Miltenyi Biotec. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------------------

namespace Assassin.Models.Api
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="IStream" />
    /// </summary>
    public interface IStream
    {
        /// <summary>
        /// Gets or sets the action that is called on exception
        /// </summary>
        /// <value>
        /// The on log.
        /// </value>
        Action<Exception> OnException { get; set; }

        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        string ClientId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the stream is locked
        /// </summary>
        bool Locked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the client is connected
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Checks the connetion.
        /// </summary>
        bool CheckConnection();

        /// <summary>
        /// Writes the package asynchronous to the stream
        /// </summary>
        /// <typeparam name="T">any type of IPackageData</typeparam>
        /// <param name="data">The data.</param>
        /// <returns>a task</returns>
        Task<bool> WriteAsync<T>(T data) where T : WebPackage;

        /// <summary>
        /// Adds a package to the send queue.
        /// </summary>
        /// <typeparam name="T">any type of IPackageData</typeparam>
        /// <param name="package">The package.</param>
        void AddToQueue<T>(T package) where T : WebPackage;

        /// <summary>
        /// Starts the guard, checking if the client is still connected
        /// </summary>
        void StartGuard();

        /// <summary>
        /// Closes this instance.
        /// </summary>
        void Close();
    }
}
