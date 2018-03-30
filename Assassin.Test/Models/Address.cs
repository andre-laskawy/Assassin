///-----------------------------------------------------------------
///   File:     Address.cs
///   Author:   Andre Laskawy           
///   Date:	    04.12.2017 09:57:26
///   Revision History: 
///   Name:  Andre Laskawy         Date:  04.12.2017 09:57:26      Description: Created
///-----------------------------------------------------------------

namespace Quant.Test.Models
{
    using Assassin.Common.Models;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Defines the <see cref="Address" />
    /// </summary>
    public class Address : BaseModel
    {
        /// <summary>
        /// Gets or sets the Street
        /// </summary>
        public string Street { get => this.GetValue<string>(); set => this.SetValue(value); }

        /// <summary>
        /// Gets the Persons
        /// </summary>
        [JsonIgnore]
        public ReadOnlyCollection<Person> Persons { get => new ReadOnlyCollection<Person>(this.GetListRelation<IList<Person>>()); }
    }
}
