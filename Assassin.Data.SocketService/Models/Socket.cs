///-----------------------------------------------------------------
///   File:     Socket.cs
///   Author:   Andre Laskawy           
///   Date:	    21.01.2018 12:55:01
///   Revision History: 
///   Name:  Andre Laskawy         Date:  21.01.2018 12:55:01      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Data.SocketService.Models
{
    using Assassin.Common;
    using Assassin.Models.Api;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="WebpackSocket" />
    /// </summary>
    public class Socket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Socket" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public Socket(SocketStream stream)
        {
            this.SocketStream = stream;
        }

        /// <summary>
        /// Gets or sets the SocketStream
        /// </summary>
        public SocketStream SocketStream { get; set; }

        /// <summary>
        /// The GetPackage
        /// </summary>
        /// <returns>The <see cref="Task{SocketMessage}"/></returns>
        public async Task<WebPackage> GetPackage()
        {
            while (SocketStream.Connected && !(SocketStream.MainStream as NetworkStream).DataAvailable)
            {
                await Task.Delay(1);
            }

            WebPackage package = null;
            try
            {
                if (SocketStream.Connected)
                {
                    int packageSize = 0;
                    List<byte> totalPackage = new List<byte>();
                    DateTime lastReceive = DateTime.MinValue;
                    while (package == null)
                    {
                        // Receive header
                        if (SocketStream.Client.Available >= 32)
                        {
                            byte[] sizeBuffer = new byte[32];
                            await SocketStream.MainStream.ReadAsync(sizeBuffer, 0, 32);
                            packageSize = BitConverter.ToInt32(sizeBuffer, 0);
                        }

                        // Receive package
                        while (totalPackage.Count < packageSize)
                        {
                            if (SocketStream.Client.Available > 0)
                            {
                                lastReceive = DateTime.Now;
                                int missing = packageSize - totalPackage.Count;
                                int readBuffer = SocketStream.Client.Available - missing > 0 ? missing : SocketStream.Client.Available;
                                byte[] buffer = new byte[readBuffer];
                                await SocketStream.MainStream.ReadAsync(buffer, 0, readBuffer);
                                totalPackage.AddRange(buffer);
                            }

                            // TIMEOUT CHECK
                            if (lastReceive != DateTime.MinValue && lastReceive.AddSeconds(30) < DateTime.Now)
                            {
                                throw new Exception("Stream Timeout");
                            }
                        }

                        if (totalPackage.Count > 0)
                        {
                            package = totalPackage.ToArray().FromBytes(typeof(WebPackage)) as WebPackage;
                            Debug.WriteLine("received package of size: " + (packageSize / 1000) + " kbytes");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Package receiving error: " + ex.Message);
            }

            return package;
        }

        /// <summary>
        /// Sends the package.
        /// </summary>
        /// <typeparam name="T">can be any type of IPackageData</typeparam>
        /// <param name="data">The <see cref="T" /></param>
        public void SendPackage<T>(T data) where T : WebPackage
        {
            try
            {
                SocketStream.AddToQueue(data);
            }
            catch (Exception ex)
            {
                throw new Exception("Package send error", ex);
            }
        }
    }
}
