namespace Assassin.Data.SocketService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Assassin.Data.SocketService.Models;
    using Assassin.Models.Api;
    using Assassin.Common;
    using Assassin.Common.Enumerations;

    /// <summary>
    /// Defines the <see cref="SocketServer" />
    /// </summary>
    public class SocketServer : IWebServerService<WebPackage>
    {
        /// <summary>
        /// The server certificate
        /// </summary>
        internal static X509Certificate serverCertificate = null;

        /// <summary>
        /// The buffer size
        /// </summary>
        private int bufferSize;

        /// <summary>
        /// Defines the server
        /// </summary>
        private TcpListener server;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketServer"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        internal SocketServer(IPEndPoint host, int bufferSize = 40000, string cert = null)
        {
            this.Host = host;
            this.bufferSize = bufferSize;
            if (!string.IsNullOrEmpty(cert))
            {
                serverCertificate = new X509Certificate(cert);
            }
        }

        /// <inheritdoc />
        public IPEndPoint Host { get; }

        /// <inheritdoc />
        public Action<object, string, LogLevel> OnNotify { get; set; }

        /// <inheritdoc />
        public Func<WebPackage, IStream, Task> OnStreaming { get; set; }

        /// <inheritdoc />
        public Func<WebPackage, Task<WebPackage>> OnFetch { get; set; }

        /// <inheritdoc />
        public Func<WebPackage, Task<WebPackage>> OnAuthenticate { get; set; }

        /// <inheritdoc />
        public Action<string> OnClientDisconnected { get; set; }

        /// <summary>
        /// Creates the specific communication server
        /// </summary>
        /// <param name="host">The <see cref="IPEndPoint" /></param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <returns>The <see cref="IServer{IPackageData}" /></returns>
        public static IWebServerService<WebPackage> Create(string host, int bufferSize = 40000, string cert = null)
        {
            return new SocketServer(host.ToIpEndpoint(), bufferSize, cert);
        }

        /// <inheritdoc />
        public void Start()
        {
            try
            {
                this.server = new TcpListener(this.Host);
                this.server.Start();
                this.Log(this.ToString(), "Server has started on " + this.Host.Address.MapToIPv4().ToString() + " - Waiting for a connection...", LogLevel.Info);
                this.WaitForConnection(this.server);
            }
            catch (Exception ex)
            {
                this.Log("Fatal socket server error", ex);
            }
        }

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            this.server.Stop();
        }

        /// <summary>
        /// Waits for connection.
        /// </summary>
        /// <param name="server">The <see cref="TcpListener" /></param>
        private void WaitForConnection(TcpListener server)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    while (true)
                    {
                        TcpClient client = await server.AcceptTcpClientAsync().ConfigureAwait(false);
                        RunClientStreaming(client);
                    }
                }
                catch (Exception ex)
                {
                    Log("Tcp client error", ex);
                }
            });
        }

        /// <summary>
        /// Start to listen to client stream in separate thread
        /// </summary>
        /// <param name="client">The <see cref="TcpClient"/></param>
        private void RunClientStreaming(TcpClient client)
        {
            this.Log(this.ToString(), "client connected", LogLevel.Info);
            Task.Factory.StartNew(async () =>
            {
                SocketStream socketStream = null;
                Models.Socket webSocket = null;
                try
                {
                    NetworkStream stream = client.GetStream();

                    socketStream = new SocketStream(client, stream, string.Empty);
                    socketStream.OnException += (ex) => this.Log("Stream error", ex);
                    webSocket = new Models.Socket(socketStream);
                    socketStream.Connected = true;

                    this.Log(this.ToString(), "start listening", LogLevel.Debug);
                    await this.StartListening(webSocket);
                }
                catch (Exception ex)
                {
                    this.Log("stream error", ex);
                }

                // Clean up
                this.CloseStream(socketStream);
            });
        }

        /// <summary>
        /// Starts the listening.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <returns>
        /// The <see cref="Task" /></returns>
        private async Task StartListening(Models.Socket socket)
        {
            List<byte> byteList = new List<byte>();
            while (socket.SocketStream.Connected)
            {
                try
                {
                    WebPackage package = await socket.GetPackage();
                    if (string.IsNullOrEmpty(socket.SocketStream.ClientId))
                    {
                        socket.SocketStream.ClientId = package.ClientId;
                    }

                    if (package != null)
                    {
                        if (package.PackageType == PackageType.Fetch)
                        {
                            try
                            {
                                this.Log(this.ToString(), "fetching data...", LogLevel.Debug);
                                WebPackage resultMessage = await this.OnFetch.Invoke(package);
                                resultMessage.Id = package.Id;
                                this.Log(this.ToString(), "Send fetch response", LogLevel.Debug);
                                socket.SendPackage(resultMessage);
                            }
                            catch (Exception ex)
                            {
                                this.Log("Socket fetch error", ex);
                            }
                        }
                        else if(package.PackageType == PackageType.Authentication)
                        {
                            new Task(async () =>
                            {
                                this.Log(this.ToString(), "authenticating...", LogLevel.Debug);
                                WebPackage resultMessage = await this.OnAuthenticate.Invoke(package);
                                resultMessage.Id = package.Id;
                                this.Log(this.ToString(), "Send auth response", LogLevel.Debug);
                                socket.SendPackage(resultMessage);
                            }).Start();
                        }
                        else
                        {
                            new Task(async () =>
                            {
                                await this.OnStreaming.Invoke(package, socket.SocketStream);
                            }).Start();
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Log("Socket stream error", ex);
                }
            }

            this.OnClientDisconnected?.Invoke(socket.SocketStream.ClientId);
            this.Log(this.ToString(), "Client connection closed", LogLevel.Info);
        }

        /// <summary>
        /// Closes the stream.
        /// </summary>
        /// <param name="stream">The <see cref="NetworkStream" /></param>
        private void CloseStream(SocketStream stream)
        {
            this.Log(this.ToString(), "flush client stream", LogLevel.Debug);
            stream.Close();
            this.OnClientDisconnected?.Invoke(stream.ClientId);
        }

        /// <summary>
        /// Logs the specified code.
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
