///-----------------------------------------------------------------
///   File:     Relation.cs
///   Author:   Andre Laskawy           
///   Date:	    25.11.2017 13:35:53
///   Revision History: 
///   Name:  Andre Laskawy         Date:  25.11.2017 13:35:53      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Common.Models
{
    using Assassin.Common.Models;
    using MsgPack.Serialization;
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Defines the <see cref="Relation" />
    /// </summary>
    public class Relation
    {
        /// <summary>
        /// The relation name
        /// </summary>
        private string relationName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Relation"/> class.
        /// </summary>
        public Relation()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        [JsonProperty(PropertyName = "Id")]
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the relation (the name of the property on the current entity).
        /// </summary>
        public string RelationName
        {
            get
            {
                return relationName;
            }
            set
            {
                relationName = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity which has a relation
        /// </summary>
        [MessagePackIgnore]
        public BaseModel Entity { get; set; }

        /// <summary>
        /// Gets or sets the entity that is related to main entity
        /// </summary>
        public BaseModel RelatedEntity { get; set; }
    }
}
