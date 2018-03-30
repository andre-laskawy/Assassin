using Assassin.Common;
using Assassin.Common.Enumerations;
using Assassin.Common.Models;
using Assassin.Data.Api;
using Assassin.Data.Repository;
using Assassin.Data.SocketService;
using Assassin.Models.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Quant.Data.Api
{
    public class AssassinWebServer
    {
        /// <summary>
        /// The web server
        /// </summary>
        private IWebServerService<WebPackage> webServer = null;

        public Func<WebPackage, Task<WebPackage>> AuthenticateUser { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinWebServer" /> class.
        /// </summary>
        /// <param name="apiConnectionType">Type of the API connection.</param>
        /// <param name="socketServerEndpoint">The socket server endpoint.</param>
        public AssassinWebServer(ApiConnectionType apiConnectionType, string serverEndpoint)
        {
            this.webServer = SocketServer.Create(serverEndpoint);
            this.webServer = SocketServer.Create(serverEndpoint);
            this.webServer.OnNotify = AssassinDataService.OnNotify;
            this.webServer.OnAuthenticate = async (package) =>
            {
                return await OnAuthenticate(package);
            };
            this.webServer.OnStreaming = async (package, stream) =>
            {
                await OnStreaming(package, stream);
            };
            this.webServer.OnFetch = async (package) =>
            {
                return await OnFetchRequest(package);
            };
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            webServer.Start();
        }

        private async Task<WebPackage> OnAuthenticate(WebPackage package)
        {
            return await AuthenticateUser?.Invoke(package);
        }

        /// <summary>
        /// Called when [fetch request].
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        private async Task<WebPackage> OnFetchRequest(WebPackage package)
        {
            WebPackage webPackage = new WebPackage();
            webPackage.Id = package.Id;
            webPackage.PackageType = PackageType.FetchResult;
            try
            {
                if (package.FetchData.Type == typeof(AssassinImage).AssemblyQualifiedName)
                {
                    package.EntityData.LoadImages();
                    webPackage.FetchData.Result_Images = new List<AssassinImage>() { package.EntityData.GetImage() ?? new AssassinImage() };
                }
                else
                {
                    if (package.FetchData.IdList)
                    {
                        webPackage.FetchData.Result_IdList = new DataRepository().GetIdList(package.FetchData.Type, package.FetchData.Query, package.FetchData.TakeCount).ToList();
                    }
                    else if (package.FetchData.TakeCount > 0)
                    {
                        IList result = new DataRepository().GetFirst(webPackage.FetchData.Type, webPackage.FetchData.TakeCount, webPackage.FetchData.Skip, webPackage.FetchData.Query, webPackage.FetchData.IncludeAll);
                        webPackage.FetchData.Result_Entities = result.Cast<BaseModel>().ToList();
                    }
                    else if (!string.IsNullOrEmpty(webPackage.FetchData.Query))
                    {
                        IList result = new DataRepository().GetListByQuery(webPackage.FetchData.Type, webPackage.FetchData.Query, webPackage.FetchData.IncludeAll);
                        AssassinDataService.OnNotify?.Invoke(this, "returning " + result.Count + " items", LogLevel.Debug);
                        webPackage.FetchData.Result_Entities = result.Cast<BaseModel>().ToList();
                    }
                }

                return webPackage;
            }
            catch (Exception ex)
            {
                webPackage.FetchData.Result_Exception = ex;
                AssassinDataService.OnNotify.Invoke(this, ex.ToText("Fetch error"), LogLevel.Error);
            }

            await Task.Delay(1);
            return webPackage;
        }

        /// <summary>
        /// Called when a streaming package was received
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>a task</returns>
        private async Task OnStreaming(WebPackage package, IStream stream)
        {
            try
            {
                AssassinDataService.OnNotify?.Invoke(this, "Package received: " + package.Method, LogLevel.Debug);
                if (package.Method == PackageMethod.Insert)
                {
                    if (package.AssassinImage != null)
                    {
                        package.AssassinImage.Save();
                    }
                    else
                    {
                        await DataHandler.Insert(package.EntityData);
                    }
                }
                else if (package.Method == PackageMethod.Update)
                {
                    if (package.AssassinImage != null)
                    {
                        package.AssassinImage.Save();
                    }
                    else
                    {
                        await DataHandler.Update(package.EntityData);
                    }
                }
                else if (package.Method == PackageMethod.Delete)
                {
                    if (package.AssassinImage != null)
                    {
                        package.AssassinImage.Delete();
                    }
                    else
                    {
                        package.EntityData.Archived = true;
                        await DataHandler.Update(package.EntityData);
                    }
                }
            }
            catch (Exception ex)
            {
                AssassinDataService.OnNotify?.Invoke(this, ex.ToText("Package error"), LogLevel.Error);
            }
        }
    }
}
