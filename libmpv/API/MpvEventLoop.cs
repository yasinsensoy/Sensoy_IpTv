using Mpv.NET.API.Interop;
using System;
using System.Threading;

namespace Mpv.NET.API
{
    public class MpvEventLoop : IMpvEventLoop, IDisposable
    {
        public bool IsRunning { get => isRunning; private set => isRunning = value; }

        public Action<MpvEvent> Callback { get; set; }

        public IntPtr MpvHandle
        {
            get => mpvHandle;
            private set
            {
                if (value == IntPtr.Zero)
                    throw new ArgumentException("Mpv handle is invalid.");

                mpvHandle = value;
            }
        }

        public IMpvFunctions Functions
        {
            get => functions;
            set
            {
                Guard.AgainstNull(value);
                functions = value;
            }
        }

        private IntPtr mpvHandle;
        private IMpvFunctions functions;
        private Thread eventLoopTask;

        private bool disposed = false;
        private volatile bool isRunning;

        public MpvEventLoop(Action<MpvEvent> callback, IntPtr mpvHandle, IMpvFunctions functions)
        {
            Callback = callback;
            MpvHandle = mpvHandle;
            Functions = functions;
        }

        public void Start()
        {
            Guard.AgainstDisposed(disposed, nameof(MpvEventLoop));
            DisposeEventLoopTask();
            IsRunning = true;
            eventLoopTask = new Thread(EventLoopTaskHandler)
            {
                IsBackground = true
            };
            eventLoopTask.Start();
        }

        public void Stop()
        {
            Guard.AgainstDisposed(disposed, nameof(MpvEventLoop));
            IsRunning = false;
            if (Thread.CurrentThread.ManagedThreadId == eventLoopTask.ManagedThreadId)
                return;
            Functions.Wakeup(mpvHandle);
            eventLoopTask.Join(100);
            eventLoopTask.Abort();
            eventLoopTask.Join();
        }

        private void EventLoopTaskHandler()
        {
            while (IsRunning)
            {
                var eventPtr = Functions.WaitEvent(mpvHandle, Timeout.Infinite);
                if (eventPtr != IntPtr.Zero)
                {
                    var @event = MpvMarshal.PtrToStructure<MpvEvent>(eventPtr);
                    if (@event.ID != MpvEventID.None)
                        Callback?.Invoke(@event);
                }
            }
        }

        private void DisposeEventLoopTask() => eventLoopTask?.Abort();

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    Stop();
                    DisposeEventLoopTask();
                }
                disposed = true;
            }
        }
    }
}