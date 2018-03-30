///-----------------------------------------------------------------
///   File:     SocketStream.cs
///   Author:   Andre Laskawy           
///   Date:	    21.01.2018 12:58:53
///   Revision History: 
///   Name:  Andre Laskawy         Date:  21.01.2018 12:58:53      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Data.SocketService.Models
{
    using Assassin.Common;
    using Assassin.Models.Api;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="SocketStream" /></summary>
    /// <seealso cref="IStream" />
    public class SocketStream : IStream
    {
        /// <summary>
        /// Defines the streamQueueWorker = null
        /// </summary>
        private StreamQueueWorker<WebPackage> streamQueueWorker = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketStream" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="id">The identifier.</param>
        public SocketStream(TcpClient client, NetworkStream stream, string id)
        {
            this.ClientId = id;
            this.MainStream = stream;
            this.Client = client;
            this.Locked = false;
            this.Guard = new SocketGuard(this);
            this.streamQueueWorker = new StreamQueueWorker<WebPackage>(this);
            this.streamQueueWorker.OnException = this.OnException;
        }

        /// <inheritdoc />
        public Action<Exception> OnException { get; set; }

        /// <inheritdoc />
        public bool Locked { get; set; }

        /// <inheritdoc />
        public bool Connected { get; internal set; }

        /// <inheritdoc />
        public TcpClient Client { get; set; }

        /// <inheritdoc />
        public NetworkStream MainStream { get; set; }

        /// <inheritdoc />
        public SocketGuard Guard { get; set; }

        /// <inheritdoc />
        public string ClientId { get; set; }

        /// <inheritdoc />
        public void StartGuard()
        {
            this.Guard.StartHeartBeat();
        }

        /// <summary>
        /// Checks the connection.
        /// </summary>
        /// <returns></returns>
        public bool CheckConnection()
        {
            return this.Guard.IsConnected();
        }

        /// <inheritdoc />
        public void Close()
        {
            try
            {
                this.Client.Dispose();
            }
            catch { }
            this.MainStream.Flush();
            this.MainStream.Dispose();
        }

        /// <inheritdoc />
        public void AddToQueue<T>(T package) where T : WebPackage
        {
            this.streamQueueWorker.Send(package);
        }

        /// <inheritdoc />
        public async Task<bool> WriteAsync<T>(T package) where T : WebPackage
        {
            try
            {
                DateTime dt = DateTime.Now;
                byte[] data = (package as WebPackage).ToByte();
                await this.SendHeader(data);

                if (data.Length > this.Client.SendBufferSize)
                {
                    // Streaming
                    var byteArrayList = this.Split(data, this.Client.SendBufferSize);
                    foreach (var item in byteArrayList)
                    {
                        await this.MainStream.WriteAsync(item.ToArray(), 0, item.Count());
                    }
                }
                else
                {
                    // One package
                    await this.MainStream.WriteAsync(data, 0, data.Length);
                }
                Console.WriteLine("Send " + data.Length / 1024 + " kbytes in " + (DateTime.Now - dt).TotalSeconds + " seconds");
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Sends the header.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>a task</returns>
        private async Task SendHeader(byte[] data)
        {
            int packageSize = data.Length;
            byte[] header = new byte[32];
            byte[] temp = BitConverter.GetBytes(packageSize);
            temp.CopyTo(header, 0);
            await this.MainStream.WriteAsync(header, 0, header.Length);
        }

        /// <summary>
        /// Splits the specified array.
        /// </summary>
        /// <typeparam name="T">can be any type</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="size">The size.</param>
        /// <returns>a collection of the specific type</returns>
        private IEnumerable<IEnumerable<T>> Split<T>(T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }
    }
}
