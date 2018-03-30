///-----------------------------------------------------------------
///   File:     BaseModel.cs
///   Author:   Andre Laskawy           
///   Date:	    25.11.2017 14:32:08
///   Revision History: 
///   Name:  Andre Laskawy         Date:  25.11.2017 14:32:08      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Common.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// This is the base class for all common models.
    /// </summary>
    public class BaseModel
    {
        public static int MaxHierarchie = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseModel"/> class.
        /// </summary>
        public BaseModel()
        {
            this.Id = Guid.NewGuid();
            this.CreatedDT = DateTime.UtcNow;
            this.ModifiedDT = DateTime.UtcNow;
            this.Relations = new HashSet<Relation>();
            this.Attributes = new ConcurrentDictionary<string, Attribute>();
            this.TypeDiscriptor = this.GetType().AssemblyQualifiedName;
        }

        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        [JsonProperty(PropertyName = "Id")]
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the type discriptor.
        /// </summary>
        [JsonProperty(PropertyName = "TypeDiscriptor")]
        public string TypeDiscriptor { get; set; }

        /// <summary>
        /// Gets or sets the creation date and time.
        /// </summary>
        [JsonProperty(PropertyName = "CreatedDT")]
        public DateTime CreatedDT { get; set; }

        /// <summary>
        /// Gets or sets the last modified date and time.
        /// </summary>
        [JsonProperty(PropertyName = "ModifiedDT")]
        public DateTime ModifiedDT { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="BaseModel"/> is archived.
        /// </summary>
        /// <value>
        ///   <c>true</c> if archived; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty(PropertyName = "Archived")]
        public bool Archived { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="BaseModel" /> is synced.
        /// </summary>
        /// <value>
        /// {D255958A-8513-4226-94B9-080D98F904A1}  <c>true</c> if synced; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty(PropertyName = "Synced")]
        public bool Synced { get; set; }

        /// <summary>
        /// Gets or sets the relations to other base models
        /// </summary>
        [JsonProperty(PropertyName = "Relations")]
        public ICollection<Relation> Relations { get; set; }

        /// <summary>
        /// Gets or sets the json attributes.
        /// </summary>
        /// <value>
        /// The json attributes.
        /// </value>
        public string JsonAttributes
        {
            get
            {
                try
                {
                    return this.Attributes.Serialize();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                return null;
            }
            set
            {
                try
                {
                    this.Attributes = value.Deserialize(typeof(ConcurrentDictionary<string, Models.Attribute>)) as ConcurrentDictionary<string, Models.Attribute>;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        internal ConcurrentDictionary<string, Attribute> Attributes { get; set; }

        /// <summary>
        /// Sets a properties value
        /// </summary>
        /// <typeparam name="T">any kind of type</typeparam>
        /// <param name="value">The <see cref="T"/></param>
        /// <param name="propertyName">The <see cref="string"/></param>
        protected virtual void SetValue<T>(T value, [CallerMemberName]string propertyName = nameof(this.Id))
        {
            if (this.Attributes.ContainsKey(propertyName))
            {
                this.Attributes[propertyName].Value = value;
            }
            else
            {
                while (!this.Attributes.TryAdd(propertyName, new Attribute()
                {
                    Name = propertyName,
                    Type = typeof(T).AssemblyQualifiedName,
                    Value = value
                }))
                {
                    Task.Delay(1).Wait();
                };
            }
        }

        /// <summary>
        /// Gets a properties value
        /// </summary>
        /// <typeparam name="T">any kind of type</typeparam>
        /// <param name="propertyName">The <see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        protected virtual T GetValue<T>([CallerMemberName]string propertyName = nameof(this.Id))
        {
            return this.Attributes.ContainsKey(propertyName) ? (T)this.Attributes[propertyName].Value : default(T);
        }

        /// <summary>
        /// Adds a relation model to this entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <param name="propertyName">Name of the property.</param>
        public void AddRelation<T>(T model, string propertyName, string bidirectionalPropertyName = null) where T : BaseModel
        {
            Relation relation = this.Relations.FirstOrDefault(p => p.RelationName == propertyName && p.RelatedEntity.Id == model?.Id);
            if (relation != null)
            {
                relation.Entity = this;
                relation.RelatedEntity = model;
            }
            else if (model != null)
            {
                relation = new Relation();
                relation.RelationName = propertyName;
                relation.Entity = this;
                relation.RelatedEntity = model;
                this.Relations.Add(relation);
            }

            if (!string.IsNullOrEmpty(bidirectionalPropertyName))
            {
                model.AddRelation(this, bidirectionalPropertyName);
            }
        }

        /// <summary>
        /// Removes the relation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="bidirectionalPropertyName">Name of the bidirectional property.</param>
        public void RemoveRelation(Guid id, string propertyName, string bidirectionalPropertyName = null)
        {
            Relation relation = this.Relations.FirstOrDefault(p => p.RelationName == propertyName && p.RelatedEntity.Id == id);
            if (relation != null)
            {
                this.Relations.Remove(relation);
            }
        }

        /// <summary>
        /// Gets a properties value
        /// </summary>
        /// <typeparam name="T">any kind of type</typeparam>
        /// <param name="propertyName">The <see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        protected T GetRelation<T>([CallerMemberName]string propertyName = nameof(this.Id)) where T : BaseModel
        {
            Relation relation = this.Relations.FirstOrDefault(p => p.RelationName == propertyName
                                                    && p.RelatedEntity.TypeDiscriptor == typeof(T).AssemblyQualifiedName);
            if (relation != null)
            {
                return relation.RelatedEntity.CastAs<T>();
            }

            return null;
        }

        /// <summary>
        /// Gets the list relation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>-
        protected T GetListRelation<T>([CallerMemberName]string propertyName = nameof(this.Id)) where T : class
        {
            var relationType = typeof(T).GenericTypeArguments.FirstOrDefault();
            var listType = typeof(List<>);
            var constructedListType = listType.MakeGenericType(relationType);
            var result = Activator.CreateInstance(constructedListType) as T;

            IEnumerable<Relation> relations = this.Relations.Where(p => p.RelationName == propertyName
                                                        && p.RelatedEntity?.TypeDiscriptor == relationType.AssemblyQualifiedName);
            foreach (var relation in relations)
            {
                (result as IList).Add(relation.RelatedEntity.CastAs(relationType));
            }

            return result;
        }

        /// <summary>
        /// To the quant model.
        /// </summary>
        /// <returns></returns>
        public T CastAs<T>(bool downCast = true) where T : BaseModel
        {
            try
            {
                if (this.GetType() != typeof(T))
                {
                    var reference = this;
                    T result = null;
                    result = this.Serialize().Deserialize(typeof(T)) as T;
                    result.Relations.Clear();
                    foreach (var relation in (this as BaseModel).Relations)
                    {
                        var rel = relation.Serialize().Deserialize(typeof(Relation)) as Relation;
                        rel.Entity = result;
                        if (downCast)
                        {
                            var type = Type.GetType(relation.RelatedEntity.TypeDiscriptor);
                            rel.RelatedEntity = rel.RelatedEntity.CastAs(type, downCast) as BaseModel;
                        }
                        else
                        {
                            rel.RelatedEntity = rel.RelatedEntity.CastAs(typeof(BaseModel), downCast) as BaseModel;
                        }
                        result.Relations.Add(rel);
                    }

                    return result;
                }
                else
                {
                    return this as T;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// The CastAs
        /// </summary>
        /// <param name="type">The <see cref="Type"/></param>
        /// <returns>The <see cref="dynamic"/></returns>
        public dynamic CastAs(Type type, bool downCast = true, int currentHierarchie = 1)
        {
            try
            {
                if (currentHierarchie == MaxHierarchie)
                {
                    return this as dynamic;
                }
                dynamic result = this as dynamic;
                if (this.GetType() != type)
                {
                    result = this.Serialize().Deserialize(type);
                }

                (result as BaseModel).Relations.Clear();
                foreach (var relation in (this as BaseModel).Relations)
                {
                    var rel = relation.Serialize().Deserialize(typeof(Relation)) as Relation;
                    rel.Entity = result as BaseModel;
                    if (downCast)
                    {
                        rel.RelatedEntity = rel.RelatedEntity.Serialize().Deserialize(Type.GetType(relation.RelatedEntity.TypeDiscriptor)) as BaseModel;
                        rel.RelatedEntity.CastAs(Type.GetType(relation.RelatedEntity.TypeDiscriptor), downCast, currentHierarchie++);
                    }
                    else
                    {
                        rel.RelatedEntity = rel.RelatedEntity.Serialize().Deserialize(typeof(BaseModel)) as BaseModel;
                        rel.RelatedEntity.CastAs(typeof(BaseModel), downCast, currentHierarchie++);
                    }

                    (result as BaseModel).Relations.Add(rel);
                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
