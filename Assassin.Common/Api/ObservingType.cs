using System;
using System.Collections.Generic;
using System.Text;

namespace Assassin.Models.Api
{
    public class ObservingType
    {
        /// <summary>
        /// Gets or sets the maximum items to synchronize.
        /// </summary>
        /// <value>
        /// The maximum items to synchronize.
        /// </value>
        public int MaxItemsToSync { get; set; }

        /// <summary>
        /// Gets or sets the maximum items to fetch at on request.
        /// </summary>
        /// <value>
        /// The maximum items to fetch at on request.
        /// </value>
        public int MaxItemsToFetchAtOnRequest { get; set; }

        /// <summary>
        /// Gets or sets the type descritor.
        /// </summary>
        /// <value>
        /// The type descritor.
        /// </value>
        public string TypeDescritor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [snync asynchronous].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [snync asynchronous]; otherwise, <c>false</c>.
        /// </value>
        public bool SyncToServerOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [lazy loading].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [lazy loading]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeRelations { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservingType" /> class.
        /// </summary>
        /// <param name="typeDescriptor">The type descriptor.</param>
        /// <param name="maxItemsToFetchAtOnRequest">The maximum items to fetch at on request.</param>
        /// <param name="syncToServerOnly">if set to <c>true</c> [synchronize asynchronous].</param>
        public ObservingType(string typeDescriptor, int maxItemsToFetchAtOnRequest, bool syncToServerOnly, int maxItemsToSync, bool includeRealtions)
        {
            this.TypeDescritor = typeDescriptor;
            this.MaxItemsToFetchAtOnRequest = maxItemsToFetchAtOnRequest;
            this.SyncToServerOnly = syncToServerOnly;
            this.MaxItemsToSync = maxItemsToSync;
            this.IncludeRelations = includeRealtions;
        }
    }
}
