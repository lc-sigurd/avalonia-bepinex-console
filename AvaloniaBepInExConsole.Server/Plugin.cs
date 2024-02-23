using System.Threading;
using BepInEx;
using NetMQ;
using NetMQ.Sockets;

namespace Sigurd.AvaloniaBepInExConsole;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private Thread? _publishingThread;

    private void Awake()
    {
        _publishingThread = new Thread(WeDoSomePublishing);
        _publishingThread.Start();
    }

    private void WeDoSomePublishing()
    {
        Logger.LogInfo("Starting publisher");
        using var publisher = new PublisherSocket(">tcp://localhost:38554");
        Logger.LogInfo("Publisher started");

        for (int counter = 0; counter < 100; counter++) {
            Logger.LogInfo("Sleeping");
            Thread.Sleep(5000);
            Logger.LogInfo("Attempting send");
            var success = publisher.TrySendFrame($"Hello, world! [{counter}]");
            Logger.LogInfo($"Tried to send message {counter}. Success? {success}");
        }
    }

    private void OnApplicationQuit()
    {
        if (_publishingThread is { IsAlive: true })
            _publishingThread.Abort();
    }
}
