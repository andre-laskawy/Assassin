///-----------------------------------------------------------------
///   File:     DataHandler.cs
///   Author:   Andre Laskawy           
///   Date:	    25.11.2017 16:12:47
///   Revision History: 
///   Name:  Andre Laskawy         Date:  25.11.2017 16:12:47      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Data.Repository
{
    using Assassin.Common.Models;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="DataHandler" />
    /// </summary>
    public class DataHandler
    {
        /// <summary>
        /// Defines the repositories
        /// </summary>
        private static DataRepository storage = new DataRepository();

        /// <summary>
        /// Initializes the specified application identifier.
        /// </summary>
        /// <param name="appId">The application identifier.</param>
        /// <param name="socketSynchronizationEndpoint">The socket synchronization endpoint.</param>
        public static void Initialize()
        {
            new DataRepository().EnsureDatabase();
            new ImageRepository().EnsureDatabase();
        }

        /// <summary>
        /// Gets or sets the last synchronization date
        /// </summary>
        public static DateTime LastSync
        {
            get
            {
                var config = storage.GetFirstOrDefault(typeof(SystemConfig).AssemblyQualifiedName)?.CastAs<SystemConfig>();
                if (config == null)
                {
                    config = new SystemConfig()
                    {
                        LastSync = DateTime.MinValue
                    };

                    new Task(async () =>
                    {
                        await storage.Create(config);
                    }).Start();
                }
                return config.LastSync;
            }
            set
            {
                var config = storage.GetFirstOrDefault(typeof(SystemConfig).AssemblyQualifiedName)?.CastAs<SystemConfig>();
                if (config == null)
                {
                    new Task(async () =>
                    {
                        await storage.Create(config);
                    }).Start();
                }
                else
                {
                    new Task(async () =>
                    {
                        await storage.Update(config);
                    }).Start();
                }
            }
        }

        /// <summary>
        /// The SaveData
        /// </summary>
        /// <param name="type">The <see cref="Type"/></param>
        /// <param name="data">The <see cref="object"/></param>
        /// <returns>The <see cref="Task{object}"/></returns>
        internal static async Task Save<T>(T data) where T : BaseModel
        {
            BaseModel entity = data.CastAs<BaseModel>();
            object result = storage.Get(data.CastAs<BaseModel>(false), false);
            if (result == null)
            {
                await Insert(data);
            }
            else
            {
                await Update(data);
            }

            data = GetById<T>(data.Id, true);
        }

        /// <summary>
        /// The InsertData
        /// </summary>
        /// <param name="type">The <see cref="Type"/></param>
        /// <param name="data">The <see cref="object"/></param>
        /// <returns>The <see cref="Task{object}"/></returns>
        public static async Task Insert<T>(T data) where T : BaseModel
        {
            await storage.Create(data.CastAs<BaseModel>(false));
        }

        /// <summary>
        /// The UpdateData
        /// </summary>
        /// <param name="type">The <see cref="Type"/></param>
        /// <param name="data">The <see cref="object"/></param>
        /// <returns>the updated entity</returns>
        public static async Task Update<T>(T data) where T : BaseModel
        {
            await storage.Update(data.CastAs<BaseModel>(false));
            data = GetById<T>(data.Id, true);
        }

        /// <summary>
        /// Delete by model
        /// </summary>
        /// <param name="type">The <see cref="Type"/></param>
        /// <param name="data">The <see cref="object"/></param>
        /// <returns>a task</returns>
        internal static async Task Delete<T>(T data) where T : BaseModel
        {
            await storage.Delete(data.CastAs<BaseModel>(false));
        }

        /// <summary>
        /// Delete by id
        /// </summary>
        /// <param name="type">The <see cref="Type"/></param>
        /// <param name="id">The <see cref="Guid"/></param>
        /// <returns>a task</returns>
        internal static async Task DeleteById(Guid id)
        {
            await storage.Delete(id);
        }

        /// <summary>
        /// Deletes the data range.
        /// </summary>
        /// <param name="type">The <see cref="Type" /></param>
        /// <param name="data">The <see cref="object" /></param>
        /// <returns>
        /// a task
        /// </returns>
        internal static async Task DeleteRange<T>(IEnumerable<T> list) where T : BaseModel
        {
            await storage.DeleteRange(list.Select(p => p.CastAs<BaseModel>(false)).ToList());
        }

        /// <summary>
        /// Gets a list by identifier list.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="idList">The identifier list.</param>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <returns>
        /// the requested entities
        /// </returns>
        internal static IEnumerable<T> GetByIdList<T>(List<Guid> idList, bool includeAll) where T : BaseModel
        {
            var BaseModelList = storage.GetByIdList(idList, includeAll);
            return BaseModelList.Cast<BaseModel>().Select(p => p.CastAs<T>());
        }

        /// <summary>
        /// Gets the identifier list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <returns></returns>
        internal static IEnumerable<Guid> GetIdList(string type, int maxItems = 1000)
        {
            return storage.GetIdList(type, null, maxItems);
        }

        /// <summary>
        /// The GetData
        /// </summary>
        /// <param name="type">The <see cref="Type" /></param>
        /// <param name="id">The <see cref="Guid" /></param>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <returns>
        /// the requested entity
        /// </returns>
        public static T GetById<T>(Guid id, bool includeAll) where T : BaseModel
        {
            T model = null;
            var entity = storage.Get(id, includeAll);
            if (entity != null)
            {
                model = entity.CastAs<T>();
            }
            return model;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="data">The data.</param>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <returns>
        /// the requested entity
        /// </returns>
        public static IEnumerable<T> GetAll<T>(bool includeAll) where T : BaseModel
        {
            var list = GetByQuery<T>(null, includeAll);
            return list.Cast<BaseModel>().Select(p => p.CastAs<T>());
        }

        /// <summary>
        /// Gets the first.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="takeCount">The take count.</param>
        /// <param name="orderFunc">The order function.</param>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <returns></returns>
        internal static IEnumerable<T> GetFirst<T>(int takeCount, int skip, string query, bool includeAll = false) where T : BaseModel
        {
            var list = storage.GetFirst(typeof(T).AssemblyQualifiedName, takeCount, skip, query, includeAll);
            return list.Cast<BaseModel>().Select(p => p.CastAs<T>());
        }

        /// <summary>
        /// Gets the first or default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <returns></returns>
        internal static T GetFirstOrDefault<T>(bool includeAll = false) where T : BaseModel
        {
            var entity = storage.GetFirstOrDefault(typeof(T).AssemblyQualifiedName, includeAll) as BaseModel;
            return entity.CastAs<T>();
        }

        /// <summary>
        /// Gets a list of entities
        /// </summary>
        /// <param name="type">The <see cref="Type"/></param>
        /// <param name="query">The query"/></param>
        /// <param name="includeAll">The <see cref="bool"/></param>
        /// <returns>The <see cref="IList"/></returns>
        internal static IEnumerable<T> GetByQuery<T>(string query, bool includeAll) where T : BaseModel
        {
            var list = storage.GetListByQuery(typeof(T).AssemblyQualifiedName, query, includeAll);
            return list.Cast<BaseModel>().Select(p => p.CastAs<T>());
        }

        /// <summary>
        /// Gets a list of entities
        /// </summary>
        /// <param name="type">The <see cref="Type"/></param>
        /// <param name="query">The query"/></param>
        /// <param name="includeAll">The <see cref="bool"/></param>
        /// <returns>The <see cref="IList"/></returns>
        internal static IEnumerable GetByQuery(string type, string query, bool includeAll)
        {
            var list = storage.GetListByQuery(type, query, includeAll);
            return list.Cast<BaseModel>().Select(p => p.CastAs(Type.GetType(type)));
        }
    }
}
