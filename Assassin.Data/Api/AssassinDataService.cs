///-----------------------------------------------------------------
///   File:     AssassinDataService.cs
///   Author:   Andre Laskawy           
///   Date:	    30.03.2018 21:04:22
///   Revision History: 
///   Name:  Andre Laskawy         Date:  30.03.2018 21:04:22      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Data.Api
{
    using Assassin.Common;
    using Assassin.Common.Enumerations;
    using Assassin.Common.Models;
    using Assassin.Data.Repository;
    using Assassin.Models.Api;
    using System;
    using System.Collections.Async;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="AssassinDataService" />
    /// </summary>
    public class AssassinDataService
    {
        /// <summary>
        /// Gets or sets the data observer which is checking for new/updated data.
        /// </summary>
        private Timer DataObserver { get; set; }

        /// <summary>
        /// Gets or sets the notify action.
        /// </summary>
        public static Action<object, string, LogLevel> OnNotify { get; set; }

        /// <summary>
        /// The web client
        /// </summary>
        public AssassinWebClient QuantWebClient { get; set; }

        /// <summary>
        /// The list with all types that need to be observed (synced).
        /// </summary>
        private List<ObservingType> observingTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinDataService" /> class.
        /// </summary>
        /// <param name="appId">The application identifier.</param>
        public AssassinDataService(string appId)
        {
            this.AppId = appId;
            DataHandler.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinDataService" /> class.
        /// </summary>
        /// <param name="appId">The application identifier.</param>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <param name="apiConnectionType">Type of the API connection.</param>
        public AssassinDataService(string appId, string remoteEndPoint, ApiConnectionType apiConnectionType, int syncIntervalInSeconds) : this(appId)
        {
            this.QuantWebClient = new AssassinWebClient(remoteEndPoint, apiConnectionType);
            this.IsConnected = false;

            // If an entity was send to the server it will be marked as synced
            StreamQueueWorker<WebPackage>.OnPackageSend = async (package) =>
            {
                try
                {
                    OnNotify?.Invoke(this, "Package send" + package.Method, LogLevel.Debug);
                    if (package.Method == PackageMethod.Insert|| package.Method == PackageMethod.Update
                        && package.EntityData != null)
                    {
                        var unknownObject = package.EntityData.CastAs(Type.GetType(package.EntityData.TypeDiscriptor)) as BaseModel;
                        unknownObject.Synced = true;
                        await DataHandler.Update(unknownObject);
                    }
                }
                catch (Exception ex)
                {
                    OnNotify?.Invoke(this, ex.ToText("Package error."), LogLevel.Error);
                }
            };

            // Start the observer
            new Thread(() =>
            {
                this.DataObserver = new Timer(RunTimer, null, 2000, syncIntervalInSeconds * 1000);
            }).Start();
        }

        /// <summary>
        /// Gets or sets the action that will be executed on a connect event.
        /// </summary>
        public Action OnConnected { get; set; }

        /// <summary>
        /// Gets or sets the action that will be executed on a disconnect event.
        /// </summary>
        public Action OnDisconnected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AssassinDataService"/> is connected.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is currently runnning a synchronization.
        /// </summary>
        public bool IsBusy { get; set; }

        /// <summary>
        /// Gets or sets the application identifier.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize(params ObservingType[] typesToWatch)
        {
            observingTypes = typesToWatch.ToList();
        }

        /// <summary>
        /// Connects to the server with the specific user credentials.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="pass">The pass.</param>
        /// <returns>the authentication result</returns>
        public async Task<AuthenticationData> Connect<T>(string user, string pass) where T : BaseModel
        {
            if (QuantWebClient != null)
            {
                await CheckConnection();
                if (IsConnected)
                {
                    return await this.QuantWebClient.Authenticate(user, pass, typeof(T).AssemblyQualifiedName);
                }
            }
            return new AuthenticationData() { Success = false, ErrorMessage = "Connection failed!" };
        }

        /// <summary>
        /// Inserts the specific data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <param name="addImage">if set to <c>true</c> the image entity will also be added.</param>
        /// <returns>a task</returns>
        public async Task Insert<T>(T data, bool addImage = false) where T : BaseModel
        {
            try
            {
                data.CreatedDT = DateTime.UtcNow;
                data.ModifiedDT = DateTime.UtcNow;
                data.Synced = false;

                // change local data
                await DataHandler.Insert(data);
                AssassinCache.AddOrUpdate(data);

                // send to server if observed
                if (QuantWebClient != null && observingTypes.Any(p => p.TypeDescritor == typeof(T).AssemblyQualifiedName))
                {
                    await CheckConnection();
                    if (IsConnected)
                    {
                        await QuantWebClient.Insert(data);
                    }
                }

                // add the image
                if (addImage)
                {
                    CheckForImage(data);
                }
            }
            catch (Exception ex)
            {
                OnNotify?.Invoke(this, ex.ToText("Error creating data."), LogLevel.Error);
            }
        }

        /// <summary>
        /// Updates the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <param name="updateImage">if set to <c>true</c> the image entity will also be updated.</param>
        /// <returns>a task</returns>
        public async Task Update<T>(T data, bool updateImage = false) where T : BaseModel
        {
            try
            {
                data.ModifiedDT = DateTime.UtcNow;
                data.Synced = false;

                // change local data
                await DataHandler.Update(data);
                AssassinCache.AddOrUpdate(data);

                // send to server if observed
                if (QuantWebClient != null && observingTypes.Any(p => p.TypeDescritor == typeof(T).AssemblyQualifiedName))
                {
                    await CheckConnection();
                    if (IsConnected)
                    {
                        await QuantWebClient?.Update(data);
                    }
                }

                // update image
                if (updateImage)
                {
                    CheckForImage(data);
                }
            }
            catch (Exception ex)
            {
                OnNotify?.Invoke(this, ex.ToText("Error updating data."), LogLevel.Error);
            }
        }

        /// <summary>
        /// Deletes the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns>a task</returns>
        public async Task Delete<T>(T data) where T : BaseModel
        {
            try
            {
                // Change local
                await DataHandler.Delete(data);
                AssassinCache.Remove(data);

                // send data to server
                if (QuantWebClient != null && observingTypes.Any(p => p.TypeDescritor == typeof(T).AssemblyQualifiedName))
                {
                    await CheckConnection();
                    if (IsConnected)
                    {
                        await QuantWebClient?.Delete(data);
                    }
                }

                // delete image
                CheckForImage(data, true);
            }
            catch (Exception ex)
            {
                OnNotify?.Invoke(this, ex.ToText("Error updating data."), LogLevel.Error);
            }
        }

        /// <summary>
        /// Gets data from server in seperate yielded results.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="takeCount">The number of entites that need to returned</param>
        /// <param name="skip">The number of entities that need to be skiped.</param>
        /// <param name="includeAll">if set to <c>true</c> all relations will be loaded too.</param>
        /// <returns>a yielded result of the requested type</returns>
        public System.Collections.Async.IAsyncEnumerable<T> GetFromServer<T>(int takeCount, int skip, string query, bool includeAll) where T : BaseModel
        {
            return new AsyncEnumerable<T>(async yield =>
            {
                try
                {
                    await Task.Delay(1);
                    var BaseModelList = await this.QuantWebClient.GetFirst(typeof(T).AssemblyQualifiedName, takeCount, skip, query, includeAll);
                    var result = BaseModelList.Select(p => p.CastAs<T>());

                    foreach (var item in result)
                    {
                        item.Synced = true;
                        await DataHandler.Update(item);
                        await item.RequestImageFromServer(this);

                        await yield.ReturnAsync(item);
                    }
                }
                catch (Exception ex)
                {
                    OnNotify?.Invoke(this, ex.ToText("Error getting data."), LogLevel.Error);
                }
            });
        }

        /// <summary>
        /// Gets a specific data range defined by the specific parameters. Can be used for paging.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="takeCount">The number of entites that need to returned</param>
        /// <param name="skip">The number of entities that need to be skiped.</param>
        /// <param name="includeAll">if set to <c>true</c> all relations will be loaded too.</param>
        /// <returns>a result list of the requested type</returns>
        public async Task<IEnumerable<T>> GetPage<T>(int takeCount, int skip, bool includeAll, string query = null) where T : BaseModel
        {
            try
            {
                await Task.Delay(1);
                return DataHandler.GetFirst<T>(takeCount, skip, query, includeAll);
            }
            catch (Exception ex)
            {
                OnNotify?.Invoke(this, ex.ToText("Error getting data."), LogLevel.Error);
            }
            return new List<T>();
        }

        /// <summary>
        /// Gets the first or default entity of a specific type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeAll">if set to <c>true</c> all relations will be loaded too.</param>
        /// <returns></returns>
        public async Task<T> GetFirstOrDefault<T>(bool includeAll) where T : BaseModel
        {
            try
            {
                await Task.Delay(1);
                return DataHandler.GetFirstOrDefault<T>(includeAll);
            }
            catch (Exception ex)
            {
                OnNotify?.Invoke(this, ex.ToText("Error getting data."), LogLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// Gets all entities of a specific type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeAll">if set to <c>true</c> all relations will be loaded too.</param>
        /// <returns>a result list of the requested type</returns>
        public async Task<IEnumerable<T>> GetAll<T>(bool includeAll) where T : BaseModel
        {
            try
            {
                await Task.Delay(1);
                return DataHandler.GetAll<T>(includeAll);
            }
            catch (Exception ex)
            {
                OnNotify?.Invoke(this, ex.ToText("Error getting data."), LogLevel.Error);
            }
            return new List<T>();
        }

        /// <summary>
        /// Gets a data object by the identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="includeAll">if set to <c>true</c> all relations will be loaded too.</param>
        /// <returns>the specific entity of the given type</returns>
        public async Task<T> GetById<T>(Guid id, bool includeAll) where T : BaseModel
        {
            try
            {
                await Task.Delay(1);
                return DataHandler.GetById<T>(id, includeAll);
            }
            catch (Exception ex)
            {
                OnNotify?.Invoke(this, ex.ToText("Error getting data."), LogLevel.Error);
            }
            return default(T);
        }

        /// <summary>
        /// Gets all entities of a specific type defined by a specific query (where part of sql)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The where clause in sql notation, e.g "MODIFIED > GETDATE()".</param>
        /// <param name="includeAll">if set to <c>true</c> all relations will be loaded too.</param>
        /// <returns>
        /// a result list of the requested type
        /// </returns>
        public async Task<IEnumerable<T>> GetByQuery<T>(string query, bool includeAll) where T : BaseModel
        {
            try
            {
                await Task.Delay(1);
                return DataHandler.GetByQuery<T>(query, includeAll);
            }
            catch (Exception ex)
            {
                OnNotify?.Invoke(this, ex.ToText("Error getting data."), LogLevel.Error);
            }
            return new List<T>();
        }

        /// <summary>
        /// Runs the observer.
        /// </summary>
        /// <param name="state">The state.</param>
        private async void RunTimer(object state)
        {
            await CheckNewData();
        }

        /// <summary>
        /// The synchronization method
        /// </summary>
        /// <param name="state">The <see cref="object"/></param>
        private async Task<bool> CheckNewData()
        {
            try
            {
                if (IsBusy)
                {
                    return true;
                }

                IsBusy = true;
                await CheckConnection();
                if (IsConnected)
                {
                    bool syncSuccessful = true;
                    // check all given types
                    foreach (var type in observingTypes.Where(p => !p.SyncToServerOnly))
                    {
                        // check last sync date
                        DateTime lastSync = DateTime.Now.AddMonths(-12);
                        if (DataHandler.LastSync > DateTime.MinValue)
                        {
                            lastSync = DataHandler.LastSync.AddMinutes(-10);
                        }

                        int skip = 0;
                        await CheckConnection();
                        if (IsConnected)
                        {
                            List<BaseModel> itemsToAddOrUpdate = new List<BaseModel>();
                            while (skip < type.MaxItemsToSync)
                            {
                                Debug.WriteLine(DateTime.Now.ToString() + ": Start fetching entity of type:" + type.TypeDescritor);
                                var data = await this.QuantWebClient.GetFirst(type.TypeDescritor, type.MaxItemsToFetchAtOnRequest, skip, $"ModifiedDT >= CAST('{lastSync}' as DATETIME)", type.IncludeRelations);
                                Debug.WriteLine(DateTime.Now.ToString() + ": Received " + data.Count() + " entity(ies)");

                                if (data == null || data.Count() == 0)
                                {
                                    break;
                                }

                                // Add/update/delete local
                                foreach (var remoteItem in data)
                                {
                                    var currentItem = DataHandler.GetById<BaseModel>(remoteItem.Id, false);
                                    if (!AddUpdateDelete(remoteItem, currentItem))
                                    {
                                        syncSuccessful = false;
                                    }
                                }

                                skip += type.MaxItemsToFetchAtOnRequest;
                            }
                        }

                        SyncToServer(type);
                        await CheckMaxItems(type.TypeDescritor, type.MaxItemsToSync);
                    }

                    // Items that need to be synced one way
                    foreach (var type in observingTypes.Where(p => p.SyncToServerOnly))
                    {
                        SyncToServer(type);
                        await CheckMaxItems(type.TypeDescritor, type.MaxItemsToSync);
                    }

                    if (syncSuccessful)
                    {
                        DataHandler.LastSync = DateTime.UtcNow;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                OnNotify?.Invoke(this, ex.ToText("Error fetching data from server - timer method failed."), LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
            return false;
        }

        /// <summary>
        /// Checks the connection.
        /// </summary>
        /// <returns>a task</returns>
        private async Task CheckConnection()
        {
            try
            {
                if (this.QuantWebClient != null)
                {
                    this.IsConnected = this.QuantWebClient.CheckConnetion();
                    if (!this.IsConnected)
                    {
                        try
                        {
                            this.IsConnected = await this.QuantWebClient.Connect(this.AppId);
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
            catch (Exception ex)
            {
                OnNotify?.Invoke(this, ex.ToText("Error creating data."), LogLevel.Error);
            }
        }

        /// <summary>
        /// Adds, updates or deletes the specific data from the server
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>true if successful</returns>
        private bool AddUpdateDelete(BaseModel data, BaseModel currentData)
        {
            bool okay = true;
            try
            {
                data.Synced = true;
                var newObject = data.CastAs(Type.GetType(data.TypeDiscriptor));
                if (newObject != null && currentData == null)
                {
                    DataHandler.Insert(newObject);
                    AssassinCache.AddOrUpdate(data);
                }
                else if (newObject != null && currentData != null && currentData.ModifiedDT < newObject.ModifiedDT)
                {
                    if ((newObject as BaseModel).Archived)
                    {
                        DataHandler.Delete(newObject);
                        AssassinCache.Remove(data);
                    }
                    else
                    {
                        DataHandler.Update(newObject);
                        AssassinCache.AddOrUpdate(data);
                    }
                }
            }
            catch (Exception ex)
            {
                OnNotify?.Invoke(this, ex.ToText("Error updating data received from server."), LogLevel.Error);
                okay = false;
            }

            return okay;
        }

        /// <summary>
        /// Checks the maximum items for a specific type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private async Task CheckMaxItems(string type, int maxItems)
        {
            var localIdList = DataHandler.GetIdList(type); // the id list is always ordered by date
            if (localIdList.Count() > maxItems)
            {
                var oldEntities = localIdList.Skip(maxItems);
                foreach (var id in oldEntities)
                {
                    await DataHandler.DeleteById(id);
                }
            }
        }

        /// <summary>
        /// Synchronizes all items that not sync to server.
        /// </summary>
        /// <param name="type">The type.</param>
        private async void SyncToServer(ObservingType type)
        {
            var list = DataHandler.GetByQuery(type.TypeDescritor, "Synced = 0", true);
            foreach (var item in list)
            {
                await CheckConnection();
                if (IsConnected)
                {
                    await this.QuantWebClient.Update(item as BaseModel);
                }
            }
        }

        /// <summary>
        /// Checks for image.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        private async void CheckForImage<T>(T data, bool delete = false) where T : BaseModel
        {
            if (data.HasImage())
            {
                var image = data.GetImage();

                if (image == null)
                {
                    return;
                }

                if (delete)
                {
                    image.Delete();
                }
                else
                {
                    image.Save();
                }

                // Send to server
                if (QuantWebClient != null && observingTypes.Any(p => p.TypeDescritor == typeof(T).AssemblyQualifiedName))
                {
                    await CheckConnection();
                    if (IsConnected)
                    {
                        if (delete)
                        {
                            await QuantWebClient.DeleteImage(image);
                        }
                        else
                        {
                            await QuantWebClient.SaveImage(image);
                        }
                    }
                }
            }
        }
    }
}
