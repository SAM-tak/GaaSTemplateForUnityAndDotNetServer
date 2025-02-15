using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.Collections.Concurrent;

namespace YourGameServer.Explorer.Services;

public interface ILogMonitorService
{
    ConcurrentQueue<string> GetLogs();
    void AddLog(string log);
}

public class LogMonitorService : ILogMonitorService
{
    readonly ConcurrentQueue<string> _logs = new();
    readonly MemoryTarget _memoryTarget;

    public LogMonitorService()
    {
        var target = LogManager.Configuration.FindTargetByName("memory");
        if(target is AsyncTargetWrapper asyncWrapper && asyncWrapper.WrappedTarget is MemoryTarget memoryTarget) {
            _memoryTarget = memoryTarget;
        }
        else if(target is MemoryTarget directMemoryTarget) {
            _memoryTarget = directMemoryTarget;
        }
        else {
            throw new InvalidOperationException("MemoryTarget 'memory' not found in NLog configuration.");
        }
    }

    public ConcurrentQueue<string> GetLogs()
    {
        foreach(var log in _memoryTarget.Logs) {
            _logs.Enqueue(log);
        }
        _memoryTarget.Logs.Clear();
        return _logs;
    }

    public void AddLog(string log)
    {
        _logs.Enqueue(log);
    }
}
