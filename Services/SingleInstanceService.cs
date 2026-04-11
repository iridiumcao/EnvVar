using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnvVar.Services;

public sealed class SingleInstanceService : IDisposable
{
    private const string MutexName = @"Local\EnvVar.SingleInstance";
    private const string PipeName = "EnvVar.SingleInstance.Activation";
    private const int ActivationRetryCount = 8;
    private const int ActivationRetryDelayMs = 250;
    private const int ActivationConnectTimeoutMs = 500;

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Mutex? _mutex;
    private Task? _listenerTask;
    private bool _ownsMutex;

    public event EventHandler? ActivationRequested;

    public bool TryAcquirePrimaryInstance()
    {
        if (_mutex is not null)
        {
            throw new InvalidOperationException("Single-instance ownership has already been checked.");
        }

        _mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
        if (!createdNew)
        {
            return false;
        }

        _ownsMutex = true;
        _listenerTask = ListenForActivationAsync(_cancellationTokenSource.Token);
        return true;
    }

    public bool SignalPrimaryInstance()
    {
        for (var attempt = 0; attempt < ActivationRetryCount; attempt++)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(ActivationConnectTimeoutMs);
                return true;
            }
            catch (TimeoutException) when (attempt < ActivationRetryCount - 1)
            {
                Thread.Sleep(ActivationRetryDelayMs);
            }
            catch (IOException) when (attempt < ActivationRetryCount - 1)
            {
                Thread.Sleep(ActivationRetryDelayMs);
            }
            catch (TimeoutException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }

        return false;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();

        if (_listenerTask is not null)
        {
            try
            {
                _listenerTask.Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(inner => inner is OperationCanceledException or ObjectDisposedException))
            {
            }
        }

        _cancellationTokenSource.Dispose();

        if (_ownsMutex && _mutex is not null)
        {
            _mutex.ReleaseMutex();
            _ownsMutex = false;
        }

        _mutex?.Dispose();
        _mutex = null;
        _listenerTask = null;
    }

    private async Task ListenForActivationAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using var server = new NamedPipeServerStream(
                PipeName,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            try
            {
                await server.WaitForConnectionAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (IOException) when (!cancellationToken.IsCancellationRequested)
            {
                continue;
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                ActivationRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
