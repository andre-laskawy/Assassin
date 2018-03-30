///-----------------------------------------------------------------
///   File:     StreamQueueWorker.cs
///   Author:   Andre Laskawy           
///   Date:	    21.01.2018 13:01:55
///   Revision History: 
///   Name:  Andre Laskawy         Date:  21.01.2018 13:01:55      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Models.Api
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="StreamQueueWorker{T}" />
    /// </summary>
    /// <typeparam name="T">any type of IPackageData</typeparam>
    public class StreamQueueWorker<T> where T : WebPackage
    {
        /// <summary>
        /// Gets or sets the on package sendy.
        /// </summary>
        /// <value>
        /// The on package sendy.
        /// </value>
        public static Action<T> OnPackageSend { get; set; }

        /// <summary>
        /// Defines the packages that need to be send
        /// </summary>
        private ConcurrentQueue<T> packages = new ConcurrentQueue<T>();

        /// <summary>
        /// Defines the stream
        /// </summary>
        private IStream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamQueueWorker{T}"/> class.
        /// </summary>
        /// <param name="stream">The <see cref="IStream"/></param>
        public StreamQueueWorker(IStream stream)
        {
            this.stream = stream;
            this.Run();
        }

        /// <summary>
        /// Gets or sets the action that is called on exception
        /// </summary>
        /// <value>
        /// The on log.
        /// </value>
        public Action<Exception> OnException { get; set; }

        /// <summary>
        /// The Send
        /// </summary>
        /// <param name="message">The <see cref="T"/></param>
        public void Send(T message)
        {
            this.packages.Enqueue(message);
        }

        /// <summary>
        /// Runs the queue worker
        /// </summary>
        public void Run()
        {
            new Task(async () =>
            {
                while (true)
                {
                    await Task.Delay(10);
                    try
                    {
                        while (this.packages.Count > 0)
                        {
                            T currentItem;
                            if (this.packages.TryPeek(out currentItem))
                            {
                                if (this.stream.Locked)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (this.stream.Connected)
                                    {
                                        if((await this.stream.WriteAsync(currentItem)))
                                        {
                                            OnPackageSend?.Invoke(currentItem);
                                        }
                                    }

                                    while (!this.packages.TryDequeue(out currentItem))
                                    {
                                        await Task.Delay(10);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.OnException?.Invoke(ex);
                    }
                }
            }, TaskCreationOptions.LongRunning).Start();
        }
    }
}
