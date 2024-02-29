using DynamicData;
using Sigurd.AvaloniaBepInExConsole.Common;

namespace Sigurd.AvaloniaBepInExConsole.App.Logs;

public interface ILogListener
{
    SourceList<LogEvent> LogMessages { get; }
}
