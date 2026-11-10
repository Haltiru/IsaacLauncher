
using System;
using System.Threading;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;

namespace IsaacLauncherGUI.Classes
{
    public class SingleInstanceManager : IDisposable
    {
        private Mutex _mutex;
        private bool _ownsMutex = false;
        private bool _disposed = false;
        public event Action ShowWindowRequested;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private NamedPipeServerStream _activePipe;
        private Task _listeningTask;
        private readonly object _pipeLock = new object();

        private const string MutexName = @"Local\IsaacLauncher.SingleInstance";
        private const string PipeName = "IsaacLauncher.Pipe";

        public bool TryAcquire()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SingleInstanceManager));
            if (_mutex != null) return _ownsMutex;

            bool createdNew;

            _mutex = new Mutex(true, MutexName, out createdNew);
            _ownsMutex = createdNew;

            return createdNew;
        }

        public void StartListening()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SingleInstanceManager));
            if (!_ownsMutex) return;

            lock (_pipeLock)
            {
                if (_listeningTask != null || _cts.IsCancellationRequested) return;

                _listeningTask = Task.Run(() => ListenAsync());
            }
        }

        private async Task ListenAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                NamedPipeServerStream pipe = null;

                try
                {
                    PipeSecurity security = new PipeSecurity();
                    SecurityIdentifier user = WindowsIdentity.GetCurrent().User;

                    security.AddAccessRule(new PipeAccessRule(user, PipeAccessRights.FullControl, AccessControlType.Allow));

                    pipe = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.In,
                        1,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous,
                        0,
                        0,
                        security
                    );

                    lock (_pipeLock)
                    {
                        if (_cts.IsCancellationRequested) return;

                        _activePipe = pipe;
                    }

                    await pipe.WaitForConnectionAsync().ConfigureAwait(false);

                    using (var reader = new StreamReader(pipe))
                    {
                        string message = await reader.ReadLineAsync().ConfigureAwait(false);

                        if (message == "SHOW_WINDOW" && !_cts.IsCancellationRequested)
                            ShowWindowRequested?.Invoke();
                    }
                }
                catch (ObjectDisposedException) when (_cts.IsCancellationRequested)
                {
                    break;
                }
                catch (IOException) when (_cts.IsCancellationRequested)
                {
                    break;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"Single instance pipe error: {ex}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Single instance listener error: {ex}");
                    break;
                }
                finally
                {
                    lock (_pipeLock)
                    {
                        if (ReferenceEquals(_activePipe, pipe)) _activePipe = null;
                    }

                    pipe?.Dispose();
                }
            }
        }

        public static async Task<bool> NotifyExistingInstanceAsync()
        {
            try
            {
                using (var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous))
                {
                    await pipe.ConnectAsync(500).ConfigureAwait(false);

                    using (var writer = new StreamWriter(pipe))
                    {
                        writer.AutoFlush = true;
                        await writer.WriteLineAsync("SHOW_WINDOW").ConfigureAwait(false);
                    }
                }

                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is UnauthorizedAccessException)
            {
                Debug.WriteLine($"Failed to notify existing instance: {ex}");
                return false;
            }
        }

        public async Task StopListeningAsync()
        {
            Task listeningTask;

            lock (_pipeLock)
            {
                _cts.Cancel();
                _activePipe?.Dispose();
                listeningTask = _listeningTask;
            }

            if (listeningTask != null)
                await listeningTask.ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_listeningTask != null && !_listeningTask.IsCompleted)
                throw new InvalidOperationException("Stop the pipe listener before disposing the manager.");

            _disposed = true;

            _cts.Cancel();
            _cts.Dispose();

            if (_ownsMutex)
            {
                _mutex.ReleaseMutex();
                _ownsMutex = false;
            }

            _mutex?.Dispose();
            _mutex = null;
        }
    }
}