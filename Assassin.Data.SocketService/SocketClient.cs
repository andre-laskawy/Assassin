///-----------------------------------------------------------------
///   File:     SocketClient.cs
///   Author:   Andre Laskawy           
///   Date:	    21.01.2018 12:45:51
///   Revision History: 
///   Name:  Andre Laskawy         Date:  21.01.2018 12:45:51      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Data.SocketService
{ 
    using Assassin.Data.SocketService.Models;
    using Assassin.Common;
    using Assassin.Models.Api;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Assassin.Common.Enumerations;
    using Assassin.Common.Models;

    /// <summary>
    /// Defines the <see cref="SocketClient" />
    /// </summary>
    public class SocketClient : IWebClientService<WebPackage>
    {
        /// <summary>
        /// The is busy
        /// </summary>
        private bool isBusy = false;

        /// <summary>
        /// Defines the 
        /// </summary>
        private string remoteHost;

        /// <summary>
        /// Defines the fetchResult
        /// </summary>
        private ConcurrentDictionary<Guid, WebPackage> fetchRequests = new ConcurrentDictionary<Guid, WebPackage>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketClient" /> class.
        /// </summary>
        /// <param name="host">The host address.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        internal SocketClient(string host, int bufferSize = 40000)
        {
            this.remoteHost = host;
            this.BufferSize = bufferSize;
        }

        /// <inheritdoc />
        public IStream Stream { get; set; }

        /// <inheritdoc />
        public Action OnConnected { get; set; }

        /// <inheritdoc />
        public Action OnDisconnected { get; set; }

        /// <inheritdoc />
        public Func<object, string, bool, WebPackage> OnPrepareCommand { get; set; }

        /// <inheritdoc />
        public Func<List<string>, string, bool, WebPackage> OnPrepareFetch { get; set; }

        /// <inheritdoc />
        public Action<WebPackage> OnStreaming { get; set; }

        /// <inheritdoc />
        public Action<string, string, LogLevel> OnNotify { get; set; }

        /// <summary>
        /// Gets or sets the Socket
        /// </summary>
        public Models.Socket Socket { get; set; }

        /// <summary>
        /// Gets or sets the BufferSize
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// Creates the specific communication client
        /// </summary>
        /// <param name="host">The <see cref="IPEndPoint"/></param>
        /// <returns>The <see cref="IClient{IPackageData}"/></returns>
        public static IWebClientService<WebPackage> Create(string serverEndPoint)
        {
            return new SocketClient(serverEndPoint);
        }

        /// <inheritdoc />
        public async Task<bool> Connect(string id, bool tryReconnect = false)
        {
            TcpClient client = new TcpClient();
            try
            {
                IPEndPoint host = this.remoteHost.ToIpEndpoint();
                IAsyncResult ar = client.BeginConnect(host.Address, host.Port, null, null);
                var result = ar.AsyncWaitHandle.WaitOne(5000, false);

                if (!result || !client.Connected)
                    return false;

                NetworkStream stream = client.GetStream();

                this.Stream = new SocketStream(client, stream, id);
                this.Stream.OnException += (ex) => this.Log("Stream Exception", ex);
                this.Socket = new Models.Socket(this.Stream as SocketStream);
                this.Log(this.ToString(), "Connected to server: " + this.remoteHost.ToString() + " on port " + host.Port, LogLevel.Info);
                (this.Stream as SocketStream).Connected = true;

                // Reconnect event
                (this.Stream as SocketStream).Guard.OnConnectionLost += async (s) =>
                {
                    if (tryReconnect)
                    {
                        await this.TryReconnect();
                    }
                };

                // Start listening to stream
                this.StartListening();
                this.OnConnected?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToText("Could not connect to server"));
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<AuthenticationData> Authenticate(AuthenticationData data)
        {
            try
            {
                DateTime start = DateTime.Now;
                this.Log(this.ToString(), "start fetching data over socket", LogLevel.Debug);

                WebPackage package = new WebPackage();
                package.PackageType = PackageType.Authentication;
                package.AuthenticationData = data;

                var fetchResult = await this.Fetch(package);

                if (fetchResult.FetchData.Result_Exception != null)
                {
                    throw fetchResult.FetchData.Result_Exception;
                }

                return fetchResult.AuthenticationData;
            }
            catch (Exception ex)
            {
                data.Success = false;
                data.ErrorMessage = ex.Message;
            }

            return data;
        }

        /// <inheritdoc />
        public async Task<List<BaseModel>> Fetch(string requestedType, int takeCount, int skip, string query, bool includeAll = true, int timeout = 60)
        {
            try
            {
                DateTime start = DateTime.Now;
                this.Log(this.ToString(), "start fetching data over socket", LogLevel.Debug);

                WebPackage package = new WebPackage();
                package.PackageType = PackageType.Fetch;
                package.FetchData = new FetchData()
                {
                    IncludeAll = includeAll,
                    TakeCount = takeCount,
                    Skip = skip,
                    Type = requestedType,
                    Query = query,
                };

                var fetchResult = await this.Fetch(package);

                if (fetchResult.FetchData.Result_Exception != null)
                {
                    throw fetchResult.FetchData.Result_Exception;
                }

                return fetchResult.FetchData.Result_Entities;
            }
            catch (Exception ex)
            {
                throw new Exception("Socket fetch error", ex);
            }
        }

        /// <inheritdoc />
        public async Task<List<BaseModel>> Fetch(string requestedType, string query, bool includeAll = true, int timeout = 60)
        {
            try
            {
                DateTime start = DateTime.Now;
                this.Log(this.ToString(), "start fetching data over socket", LogLevel.Debug);

                WebPackage package = new WebPackage();
                package.PackageType = PackageType.Fetch;
                package.FetchData = new FetchData()
                {
                    IncludeAll = includeAll,
                    Query = query,
                    Type = requestedType
                };

                var fetchResult = await this.Fetch(package);

                if (fetchResult.FetchData.Result_Exception != null)
                {
                    throw fetchResult.FetchData.Result_Exception;
                }

                return fetchResult.FetchData.Result_Entities;
            }
            catch (Exception ex)
            {
                throw new Exception("Socket fetch error", ex);
            }
        }

        /// <inheritdoc />
        public async Task<List<Guid>> GetIdList(string requestedType, string query, int timeout = 60, int maxItems = 9999)
        {
            try
            {
                DateTime start = DateTime.Now;
                this.Log(this.ToString(), "start fetching data over socket", LogLevel.Debug);

                WebPackage package = new WebPackage();
                package.PackageType = PackageType.Fetch;
                package.FetchData = new FetchData()
                {
                    IdList = true,
                    Query = query,
                    Type = requestedType,
                    TakeCount = maxItems
                };

                var fetchResult = await this.Fetch(package);

                if (fetchResult.FetchData.Result_Exception != null)
                {
                    throw fetchResult.FetchData.Result_Exception;
                }

                return fetchResult.FetchData.Result_IdList;
            }
            catch (Exception ex)
            {
                throw new Exception("Socket fetch error", ex);
            }
        }

        /// <inheritdoc />
        public async Task<WebPackage> Fetch(WebPackage package)
        {
            try
            {
                while (isBusy)
                {
                    await Task.Delay(10);
                }
                isBusy = true;

                while(!this.fetchRequests.TryAdd(package.Id, null))
                {
                    await Task.Delay(10);
                }

                DateTime start = DateTime.Now;
                this.Log(this.ToString(), "start fetching data over socket", LogLevel.Debug);

                this.Socket.SendPackage(package);

                while (this.fetchRequests[package.Id] == null)
                {
                    await Task.Delay(1);
                    if (start.AddSeconds(60) < DateTime.Now)
                    {
                        throw new TimeoutException("Timeout during socket fetch");
                    }
                }

                this.Log(this.ToString(), "fetch package received", LogLevel.Debug);

                WebPackage result = null;
                while (!this.fetchRequests.TryRemove(package.Id, out result))
                {
                    await Task.Delay(10);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Socket fetch error", ex);
            }
            finally
            {
                package.PackageProcessed = true;
                isBusy = false;
            }
        }

        /// <inheritdoc />
        public WebPackage Send<G>(G data, string command, string targetId = null, bool withReceipt = false) where G : class
        {
            try
            {
                this.Log(this.ToString(), "Start send data", LogLevel.Debug);
                WebPackage message = this.OnPrepareCommand.Invoke(data, command, withReceipt);

                this.Log(this.ToString(), "Send data to client", LogLevel.Debug);
                this.Socket.SendPackage(message);

                return message;
            }
            catch (Exception ex)
            {
                throw new Exception("Socket send error", ex);
            }
        }

        /// <inheritdoc />
        public WebPackage Send(WebPackage message)
        {
            try
            {
                message.ClientId = this.Stream.ClientId;
                this.Log(this.ToString(), "Send data to client", LogLevel.Debug);
                this.Stream.AddToQueue(message);
                return message;
            }
            catch (Exception ex)
            {
                throw new Exception("Grpc send error", ex);
            }
        }

        /// <summary>
        /// The Dispose
        /// </summary>
        public async Task Disconnect()
        {
            this.Socket.SocketStream.Close();
        }

        /// <summary>
        /// Starts the listening.
        /// </summary>
        private void StartListening()
        {
            Task.Run(async () =>
            {
                while (Socket.SocketStream.Connected)
                {
                    try
                    {
                        WebPackage package = await Socket.GetPackage() as WebPackage;
                        if (package != null)
                        {
                            if (package.PackageType == PackageType.FetchResult)
                            {
                                Log(this.ToString(), "Fetch package received", LogLevel.Debug);
                                if (this.fetchRequests.ContainsKey(package.Id))
                                {
                                    this.fetchRequests[package.Id] = package;
                                }
                            }
                            else
                            {
                                Log(this.ToString(), "Streaming package received", LogLevel.Debug);
                                OnStreaming.Invoke(package);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Socket stream error", ex);
                    }
                }

                this.OnDisconnected?.Invoke();
                Log(this.ToString(), "Connection closed", LogLevel.Info);
                Socket.SocketStream.Close();
            });
        }

        /// <summary>
        /// Tries the reconnect.
        /// </summary>
        private async Task TryReconnect()
        {
            this.Log(this.ToString(), "Try reconnect", LogLevel.Debug);
            if (!await this.Connect(this.Stream.ClientId, true))
            {
                this.Log(this.ToString(), "Reconnect failed", LogLevel.Debug);
                await Task.Delay(5000);
                await this.TryReconnect();
            }
        }

        /// <summary>
        /// The Log
        /// </summary>
        /// <param name="code">The <see cref="string" /></param>
        /// <param name="ex">The <see cref="Exception" /></param>
        private void Log(string code, Exception ex)
        {
            this.Log(ex.Source, ex.ToText(code), LogLevel.Error);
        }

        /// <summary>
        /// Logs the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="level">The level.</param>
        private void Log(string source, string msg, LogLevel level)
        {
            this.OnNotify?.Invoke(source, msg, level);
        }
    }
}
