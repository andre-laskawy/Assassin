///-----------------------------------------------------------------
///   File:     Person.cs
///   Author:   Andre Laskawy           
///   Date:	    25.11.2017 16:33:54
///   Revision History: 
///   Name:  Andre Laskawy         Date:  25.11.2017 16:33:54      Description: Created
///-----------------------------------------------------------------

namespace Quant.Test.Models
{
    using Assassin.Common.Models;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Defines the <see cref="Person" />
    /// </summary>
    public class Person : BaseModel
    {
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get => this.GetValue<string>(); set => this.SetValue(value); }

        /// <summary>
        /// Gets or sets the Age
        /// </summary>
        public int Age { get => this.GetValue<int>(); set => this.SetValue(value); }

        /// <summary>
        /// Gets the Addresses
        /// </summary>
        [JsonIgnore]
        public ReadOnlyCollection<Address> Addresses { get => new ReadOnlyCollection<Address>(this.GetListRelation<IList<Address>>()); }
    }
}
