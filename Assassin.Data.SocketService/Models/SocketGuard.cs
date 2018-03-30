///-----------------------------------------------------------------
///   File:     SocketGuard.cs
///   Author:   Andre Laskawy           
///   Date:	    30.03.2018 20:30:18
///   Revision History: 
///   Name:  Andre Laskawy         Date:  30.03.2018 20:30:18      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Data.SocketService.Models
{
    using System;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// The ConnectionLost
    /// </summary>
    /// <param name="stream">The <see cref="Model.SocketStream"/></param>
    public delegate void ConnectionLost(SocketStream stream);

    /// <summary>
    /// Defines the <see cref="SocketGuard" />
    /// </summary>
    public class SocketGuard
    {
        /// <summary>
        /// The heard beat timer
        /// </summary>
        private Timer heardBeatTimer = null;

        /// <summary>
        /// The stream
        /// </summary>
        private SocketStream stream = null;

        /// <summary>
        /// Occurs when [on connection lost].
        /// </summary>
        public event ConnectionLost OnConnectionLost;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketGuard" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public SocketGuard(SocketStream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Starts the heart beat.
        /// </summary>
        /// <param name="stream">The <see cref="SocketStream" /></param>
        public void StartHeartBeat()
        {
            this.heardBeatTimer = new Timer(this.CheckConnection, null, 2000, 5000);
        }

        /// <summary>
        /// Determines whether this instance is connected.
        /// </summary>
        /// <returns>
        /// The <see cref="bool" /></returns>
        public bool IsConnected()
        {
            bool result = true;
            try
            {
                if (this.stream.Client.Client.Poll(3000, SelectMode.SelectRead))
                {
                    if (!this.stream.Client.Connected)
                    {
                        result = false;
                    }
                    else
                    {
                        byte[] b = new byte[1];
                        try
                        {
                            if (this.stream.Client.Client.Receive(b, SocketFlags.Peek) == 0)
                            {
                                result = false;
                            }
                        }
                        catch
                        {
                            result = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Checks the connection.
        /// </summary>
        /// <param name="state">The <see cref="object" /></param>
        private void CheckConnection(object state)
        {
            if (!IsConnected())
            {
                this.heardBeatTimer.Dispose();
                this.stream.Connected = false;
                this.OnConnectionLost?.Invoke(this.stream);
            }
        }
    }
}
