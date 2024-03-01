using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using Microsoft.Extensions.Hosting;
using Sigurd.AvaloniaBepInExConsole.Common;

namespace Sigurd.AvaloniaBepInExConsole.App.Logs;

public class DummyLogListener : BackgroundService, ILogListener
{
    public SourceList<LogEvent> LogMessages { get; } = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogMessages.AddRange(Enumerable.Repeat(TestLogEvent, 20));
        while (!stoppingToken.IsCancellationRequested) {
            await Task.Delay(750, stoppingToken);
            LogMessages.Add(TestLogEvent);
        }
    }

    private static LogEvent TestLogEvent => new LogEvent { Content = "\x1b[0;38;5;99mthe quick bro\x1b[0mwn fox\n jumps\n over \x1b[107;94mthe \nlazy dog 0123456\n789", Level = BepInExLogLevel.Info, SourceName = "Foobar" };
}
