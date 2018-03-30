///-----------------------------------------------------------------
///   File:     Attribute.cs
///   Author:   Andre Laskawy           
///   Date:	    25.11.2017 13:51:19
///   Revision History: 
///   Name:  Andre Laskawy         Date:  25.11.2017 13:51:19      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Common.Models
{
    /// <summary>
    /// Defines the <see cref="Attribute" />
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        public dynamic Value { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public string Type { get; set; }
    }
}
