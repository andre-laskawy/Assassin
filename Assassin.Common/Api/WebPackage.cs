///-----------------------------------------------------------------
///   File:     WebPackage.cs
///   Author:   Andre Laskawy           
///   Date:	    21.01.2018 13:31:44
///   Revision History: 
///   Name:  Andre Laskawy         Date:  21.01.2018 13:31:44      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Models.Api
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using Assassin.Common.Models;
    using Assassin.Common.Enumerations;

    /// <summary>
    /// Defines the <see cref="WebPackage" />
    /// </summary>
    public class WebPackage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebPackage"/> class.
        /// </summary>
        public WebPackage()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [JsonProperty(PropertyName = "Id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the package.
        /// </summary>
        [JsonProperty(PropertyName = "PackageType")]
        public PackageType PackageType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the package was processed sucessfuly.
        /// </summary>
        [JsonProperty(PropertyName = "PackageProcessed")]
        public bool PackageProcessed { get; set; }

        /// <summary>
        /// Gets or sets the method (e.g. Insert, Update, Delete)
        /// </summary>
        [JsonProperty(PropertyName = "Method")]
        public PackageMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        [JsonProperty(PropertyName = "ClientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the authentication data used to authenticate the client.
        /// </summary>
        [JsonProperty(PropertyName = "AuthenticationData")]
        public AuthenticationData AuthenticationData { get; set; }

        /// <summary>
        /// Gets or sets the model of the package.
        /// </summary>
        [JsonProperty(PropertyName = "EntityData")]
        public BaseModel EntityData { get; set; }

        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        [JsonProperty(PropertyName = "AssassinImage")]
        public AssassinImage AssassinImage { get; set; }

        /// <summary>
        /// Gets or sets the information needed for a specific fetch(get) request
        /// </summary>
        [JsonProperty(PropertyName = "FetchData")]
        public FetchData FetchData { get; set; }
    }
}
