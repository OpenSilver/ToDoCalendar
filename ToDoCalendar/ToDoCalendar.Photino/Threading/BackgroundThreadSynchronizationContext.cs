using System.Collections.Concurrent;
using System.Diagnostics;

namespace ToDoCalendar.Photino.Threading
{
    internal class BackgroundThreadSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<(SendOrPostCallback callback, object? state, bool isSend, ManualResetEventSlim? syncEvent)> _queue
            = new BlockingCollection<(SendOrPostCallback, object?, bool, ManualResetEventSlim?)>();

        private bool _running = true;
        private int? _threadId;

        /// <summary>
        /// Asynchronously posts a callback to this context.
        /// </summary>
        public override void Post(SendOrPostCallback d, object? state)
        {
            _queue.Add((d, state, false, null));
        }

        /// <summary>
        /// Synchronously sends a callback to this context.
        /// </summary>
        public override void Send(SendOrPostCallback d, object? state)
        {
            var syncEvent = new ManualResetEventSlim(false);
            _queue.Add((d, state, true, syncEvent));

            syncEvent.Wait();
        }

        /// <summary>
        /// The main message loop. Call this on your dedicated thread.
        /// It will continue to process items until Stop() is called.
        /// </summary>
        public void RunMessageLoop()
        {
            _threadId = Thread.CurrentThread.ManagedThreadId;

            while (_running)
            {
                var (callback, state, isSend, syncEvent) = _queue.Take();

                try
                {
                    callback(state);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception in the background synchronization context: " + ex);
                }

                // If this was a Send(...) call, release the waiting thread
                if (isSend && syncEvent != null)
                {
                    syncEvent.Set();
                }
            }
        }

        /// <summary>
        /// Stops the message loop.
        /// </summary>
        public void Stop()
        {
            _running = false;
            _queue.CompleteAdding();  // Unblock the queue.Take() if waiting
        }

        /// <summary>
        /// Check if the current thread is the same as the background thread running this context.
        /// </summary>
        public bool CheckAccess()
        {
            return _threadId == Thread.CurrentThread.ManagedThreadId;
        }
    }
}
