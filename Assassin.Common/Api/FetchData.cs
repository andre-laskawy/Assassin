///-----------------------------------------------------------------
///   File:     FetchData.cs
///   Author:   Andre Laskawy           
///   Date:	    21.01.2018 19:05:15
///   Revision History: 
///   Name:  Andre Laskawy         Date:  21.01.2018 19:05:15      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Models.Api
{
    using Assassin.Common.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="FetchData" />
    /// </summary>
    public class FetchData
    {
        /// <summary>
        /// Gets or sets a value indicating whether IncludeAll
        /// </summary>
        [JsonProperty(PropertyName = "IncludeAll")]
        public bool IncludeAll { get; set; }

        /// <summary>
        /// Gets or sets the Query
        /// </summary>
        [JsonProperty(PropertyName = "Query")]
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the Types
        /// </summary>
        [JsonProperty(PropertyName = "Type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the take count.
        /// </summary>
        /// <value>
        /// The take count.
        /// </value>
        [JsonProperty(PropertyName = "TakeCount")]
        public int TakeCount { get; set; }

        /// <summary>
        /// Gets or sets the skip.
        /// </summary>
        /// <value>
        /// The skip.
        /// </value>
        [JsonProperty(PropertyName = "Skip")]
        public int Skip { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FetchData"/> is count.
        /// </summary>
        /// <value>
        ///   <c>true</c> if count; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty(PropertyName = "Count")]
        public bool IdList { get; set; }

        /// <summary>
        /// Gets or sets the result id list.
        /// </summary>
        [JsonProperty(PropertyName = "Result_IdList")]
        public List<Guid> Result_IdList { get; set; }

        /// <summary>
        /// Gets or sets the result entity list.
        /// </summary>
        [JsonProperty(PropertyName = "Result_Entities")]
        public List<BaseModel> Result_Entities { get; set; }

        /// <summary>
        /// Gets or sets the result image list.
        /// </summary>
        [JsonProperty(PropertyName = "Result_Images")]
        public List<AssassinImage> Result_Images { get; set; }

        /// <summary>
        /// Gets or sets the result exception.
        /// </summary>
        [JsonProperty(PropertyName = "Result_Exception")]
        public Exception Result_Exception { get; set; }
    }
}
