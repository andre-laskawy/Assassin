namespace Assassin.Data.Api
{
    using Assassin.Common;
    using Assassin.Common.Enumerations;
    using Assassin.Common.Models;
    using Assassin.Data.SocketService;
    using Assassin.Models.Api;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="AssassinWebClient" />
    /// </summary>
    public class AssassinWebClient
    {
        /// <summary>
        /// Defines the webservice
        /// </summary>
        private IWebClientService<WebPackage> webservice;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinWebClient"/> class.
        /// </summary>
        public AssassinWebClient() : this ("127.0.0.1", ApiConnectionType.Socket) { }

        /// <summary>
        /// Prevents a default instance of the <see cref="AssassinWebClient"/> class from being created.
        /// </summary>
        /// <param name="webservice">The <see cref="IWebservice"/></param>
        internal AssassinWebClient(string endPoint, ApiConnectionType apiConnectionType)
        {
            if (apiConnectionType == ApiConnectionType.Socket)
            {
                this.webservice = SocketClient.Create(endPoint);
            }
            this.webservice.OnConnected = () => { Debug.WriteLine("Connected to server."); };
            this.webservice.OnDisconnected = () => { Debug.WriteLine("Disconnected from server."); };
        }

        /// <summary>
        /// Connects to the endpoint with the specified application identifier.
        /// </summary>
        /// <param name="appId">The application identifier.</param>
        /// <param name="serverEndpint">The server endpint.</param>
        /// <returns></returns>
        internal async Task<bool> Connect(string appId)
        {
            try
            {
                return await this.webservice.Connect(appId, false);
            }
            catch (Exception ex)
            {
                throw new Exception("Unkown client connection error: ", ex);
            }
        }

        internal bool CheckConnetion()
        {
            try
            {
                return this.webservice.Stream.CheckConnection();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Authenticates the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="pass">The pass.</param>
        /// <returns></returns>
        internal async Task<AuthenticationData> Authenticate(string user, string pass, string userModelType)
        {
            try
            {
                AuthenticationData authenticationData = new AuthenticationData()
                {
                    UserName = user,
                    Password = pass,
                    UserModelType = userModelType
                };
                return await this.webservice.Authenticate(authenticationData);
            }
            catch (Exception ex)
            {
                AssassinDataService.OnNotify?.Invoke(this, ex.ToText("Error during authentication."), LogLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <returns></returns>
        internal async Task<IEnumerable<T>> GetAll<T>(bool includeAll) where T : BaseModel
        {
            try
            {
                List<T> result = new List<T>();
                var BaseModels = await this.webservice.Fetch(typeof(T).AssemblyQualifiedName, null, includeAll);
                if (BaseModels.Count > 0)
                {
                    foreach (var BaseModel in BaseModels)
                    {
                        result.Add(BaseModel.CastAs<T>());
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                AssassinDataService.OnNotify?.Invoke(this, ex.ToText("Error fetching data from server - method GetAll failed."), LogLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// Gets the first.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="takeCount">The take count.</param>
        /// <param name="orderFunc">The order function.</param>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <returns>a task</returns>
        internal async Task<IEnumerable<BaseModel>> GetFirst(string type, int takeCount, int skip, string query, bool includeAll = false)
        {
            try
            {
                return await this.webservice.Fetch(type, takeCount, skip, query, includeAll);
            }
            catch (Exception ex)
            {
                AssassinDataService.OnNotify?.Invoke(this, ex.ToText("Error fetching data from server - method GetFirst failed."), LogLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <returns>a task</returns>
        internal async Task<IEnumerable<BaseModel>> GetById(string type, Guid id, bool includeAll)
        {
            try
            {
                return await this.webservice.Fetch(type, $"Id ='{id}'", includeAll);
            }
            catch (Exception ex)
            {
                AssassinDataService.OnNotify?.Invoke(this, ex.ToText("Error fetching data from server - method GetAll failed."), LogLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <returns>a task</returns>
        internal async Task<IEnumerable<BaseModel>> GetByQuery(string type, string query, bool includeAll)
        {
            try
            {
                return await this.webservice.Fetch(type, query, includeAll);
            }
            catch (Exception ex)
            {
                AssassinDataService.OnNotify?.Invoke(this, ex.ToText("Error fetching data from server - method GetAll failed."), LogLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// Gets a id list from the server for a specific type and query
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="query">The query.</param>
        /// <param name="maxItems">The maximum items.</param>
        /// <returns>the id list</returns>
        internal async Task<List<Guid>> GetIdList(string type, string query, int maxItems = 9999)
        {
            try
            {
                return await this.webservice.GetIdList(type, query, 60, maxItems);
            }
            catch (Exception ex)
            {
                AssassinDataService.OnNotify?.Invoke(this, ex.ToText("Error fetching data from server - method GetAll failed."), LogLevel.Error);
            }
            return new List<Guid>();
        }

        /// <summary>
        /// Adds the specific data model on the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The <see cref="object" /></param>
        /// <returns>a task</returns>
        public async Task Insert<T>(T data) where T : BaseModel
        {
            try
            {
                WebPackage package = new WebPackage()
                {
                    PackageType = PackageType.Command,
                    EntityData = data,
                    Method = PackageMethod.Insert
                };
                this.webservice.Send(package);
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                AssassinDataService.OnNotify?.Invoke(this, ex.ToText("Error sending data to server - method Insert failed."), LogLevel.Error);
            }
        }

        /// <summary>
        /// Updates the specifc data model on the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        internal async Task Update<T>(T data) where T : BaseModel
        {
            try
            {
                WebPackage package = new WebPackage()
                {
                    PackageType = PackageType.Command,
                    EntityData = data,
                    Method = PackageMethod.Update
                };
                this.webservice.Send(package);
                data.Synced = true;
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                AssassinDataService.OnNotify?.Invoke(this, ex.ToText("Error sending data to server - method Update failed."), LogLevel.Error);
            }
        }

        /// <summary>
        /// Deletes the specific data model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns>a task</returns>
        internal async Task Delete<T>(T data) where T : BaseModel
        {
            try
            {
                WebPackage package = new WebPackage()
                {
                    PackageType = PackageType.Command,
                    EntityData = data,
                    Method = PackageMethod.Delete
                };
                this.webservice.Send(package);
                data.Synced = true;
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                AssassinDataService.OnNotify?.Invoke(this, ex.ToText("Error sending data to server - method Delete failed."), LogLevel.Error);
            }
        }

        /// <summary>
        /// Requests the images for a specific entity from the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="BaseModel">The BaseModel.</param>
        /// <returns>a task</returns>
        internal async Task<T> GetImage<T>(T data) where T : BaseModel
        {
            WebPackage package = new WebPackage()
            {
                PackageType = PackageType.Fetch,
                EntityData = data
            };

            try
            {
                List<T> result = new List<T>();

                package.FetchData = new FetchData()
                {
                    Type = typeof(AssassinImage).AssemblyQualifiedName
                };

                var fetchResult = await this.webservice.Fetch(package);

                if (fetchResult.FetchData.Result_Exception != null)
                {
                    throw fetchResult.FetchData.Result_Exception;
                }

                if (fetchResult.FetchData.Result_Images != null)
                {
                    data.SetImage(fetchResult.FetchData.Result_Images.FirstOrDefault());
                    fetchResult.FetchData.Result_Images.FirstOrDefault().Save();
                }

                return data;
            }
            catch (Exception ex)
            {
                AssassinDataService.OnNotify?.Invoke(this, ex.ToText("Error fetching data from server - method GetAll failed."), LogLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// Adds a image on the server.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>a task</returns>
        internal async Task SaveImage(AssassinImage image)
        {
            try
            {
                WebPackage package = new WebPackage()
                {
                    PackageType = PackageType.Command,
                    AssassinImage = image,
                    Method = PackageMethod.Update
                };
                this.webservice.Send(package);
                image.Synced = true;
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                AssassinDataService.OnNotify?.Invoke(this, ex.ToText("Error sending data to server - method Update failed."), LogLevel.Error);
            }
        }

        /// <summary>
        /// Deletes a image from the server.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>a task</returns>
        internal async Task DeleteImage(AssassinImage image)
        {
            try
            {
                WebPackage package = new WebPackage()
                {
                    PackageType = PackageType.Command,
                    AssassinImage = image,
                    Method = PackageMethod.Delete
                };
                this.webservice.Send(package);
                image.Synced = true;
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                AssassinDataService.OnNotify?.Invoke(this, ex.ToText("Error sending data to server - method Delete failed."), LogLevel.Error);
            }
        }
    }
}
