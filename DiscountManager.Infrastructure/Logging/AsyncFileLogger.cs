using DiscountManager.Domain.Interfaces;
using System.Runtime.CompilerServices;
using System.Threading.Channels;


namespace DiscountManager.Infrastructure.Logging;

public sealed class AsyncFileLogger : IFileLogger, IDisposable
{
    private readonly Channel<string> _channel;
    private readonly Task _consumerTask;
    private readonly string _filePath;
    private readonly Action<Exception>? _errorHandler;
    private int _disposed;

    public AsyncFileLogger(
        string filePath,
        Action<Exception>? errorHandler = null,
        int capacityHint = 0)
    {

        var directory = Path.GetDirectoryName(filePath)!;

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        _filePath = filePath;
        _errorHandler = errorHandler;

        _channel = capacityHint > 0
            ? Channel.CreateBounded<string>(capacityHint)
            : Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

        _consumerTask = Task.Run(ProcessQueueAsync)
                            .ContinueWith(t => HandleError(t.Exception),
                                TaskContinuationOptions.OnlyOnFaulted);
    }

    public bool Info(string msg, [CallerMemberName] string caller = "") => Enqueue("INFO", msg, caller);
    public bool Warn(string msg, [CallerMemberName] string caller = "") => Enqueue("WARN", msg, caller);
    public bool Error(string msg, Exception? ex = null, [CallerMemberName] string caller = "") =>
        Enqueue("ERROR", msg + (ex != null ? $" | {ex}" : ""), caller);

    private bool Enqueue(string level, string msg, string caller)
    {
        if (_disposed == 1) return false;
        var line = $"[{DateTime.UtcNow:O}] [{level}] [{caller}] {msg}";
        return _channel.Writer.TryWrite(line);
    }

    private async Task ProcessQueueAsync()
    {
        await foreach (var line in _channel.Reader.ReadAllAsync())
        {
            try
            {
                await File.AppendAllTextAsync(_filePath, line + Environment.NewLine)
                          .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
    }

    private void HandleError(Exception? ex)
    {
        if (ex is null) return;
        if (_errorHandler is not null)
        {
            try { _errorHandler.Invoke(ex); } catch { }
        }
        else
        {
            try { Console.Error.WriteLine($"[AsyncFileLogger] {ex}"); } catch { }
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
        _channel.Writer.TryComplete();
        try { _consumerTask.Wait(); }
        catch (AggregateException ag) { HandleError(ag.InnerException); }
    }
}
