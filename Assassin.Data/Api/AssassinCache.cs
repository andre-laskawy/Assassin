///-----------------------------------------------------------------
///   File:     QuantCache.cs
///   Author:   Andre Laskawy           
///   Date:	    07.02.2018 22:00:06
///   Revision History: 
///   Name:  Andre Laskawy         Date:  07.02.2018 22:00:06      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Data.Api
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;
    using Assassin.Common.Models;

    /// <summary>
    /// Defines the <see cref="AssassinCache" />
    /// </summary>
    public class AssassinCache
    {
        /// <summary>
        /// The BaseModels cache
        /// </summary>
        private static ConcurrentDictionary<Guid,BaseModel> BaseModelsCache = new ConcurrentDictionary<Guid,BaseModel>();

        /// <summary>
        /// Gets specific items from the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>a list of the requested item</returns>
        public static List<T> Get<T>(int skip = 0, int load = 9999) where T : BaseModel
        {
            List<T> result = new List<T>();
            var BaseModels = BaseModelsCache.Values.ToList().Where(p => p.TypeDiscriptor == typeof(T).AssemblyQualifiedName && !p.Archived).Skip(skip).Take(load);
            foreach (var BaseModel in BaseModels)
            {
                result.Add(BaseModel.CastAs<T>());
            }
            return result.OrderByDescending(p => p.CreatedDT).ToList();
        }

        /// <summary>
        /// Adds or update.
        /// </summary>
        /// <param name="item">The item.</param>
        public static void AddOrUpdate<T>(T item) where T : BaseModel
        {
            if (BaseModelsCache.ContainsKey(item.Id))
            {
                Remove(item);
            } 
             
            while (!BaseModelsCache.TryAdd(item.Id, item))
            {
                Task.Delay(10).Wait();
            }
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public static void Remove<T>(T item) where T : BaseModel
        {
            BaseModel i = null;
            while (!BaseModelsCache.TryRemove(item.Id, out i))
            {
                Task.Delay(10).Wait();
            }
        }

        /// <summary>
        /// Initializes a cache for the instances of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static async Task Init<T>(AssassinDataService service, int numberOfEntities = 100) where T : BaseModel
        {
            try
            {
                var items = (await service.GetPage<T>(numberOfEntities, 0, true)).ToList();
                foreach (var item in items) 
                {
                    AddOrUpdate(item); 
                    var result = item.LoadImages(true);
                    if(!result && service.QuantWebClient != null)
                    {
                        await item.RequestImageFromServer(service);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
