///-----------------------------------------------------------------
///   File:     AuthenticationData.cs
///   Author:   Andre Laskawy           
///   Date:	    30.03.2018 17:39:26
///   Revision History: 
///   Name:  Andre Laskawy         Date:  30.03.2018 17:39:26      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Models.Api
{
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the <see cref="AuthenticationData" />
    /// </summary>
    public class AuthenticationData
    {
        /// <summary>
        /// Gets or sets a value indicating whether Success
        /// </summary>
        [JsonProperty(PropertyName = "Sucess")]
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the UserName
        /// </summary>
        [JsonProperty(PropertyName = "UserName")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        [JsonProperty(PropertyName = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the ErrorMessage
        /// </summary>
        [JsonProperty(PropertyName = "ErrorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the UserModelType
        /// </summary>
        [JsonProperty(PropertyName = "UserModelType")]
        public string UserModelType { get; set; }

        /// <summary>
        /// Gets or sets the UserModel
        /// </summary>
        [JsonProperty(PropertyName = "UserModel")]
        public string UserModel { get; set; }
    }
}
