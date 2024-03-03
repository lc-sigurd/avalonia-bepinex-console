using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.LowLevel;
using Object = UnityEngine.Object;

namespace Sigurd.AvaloniaBepInExConsole;

internal static class Bootstrap
{
    [UsedImplicitly]
    static void Start()
    {
        var loop = PlayerLoop.GetCurrentPlayerLoop();
        Cysharp.Threading.Tasks.PlayerLoopHelper.Initialize(ref loop);

        var harmony = new Harmony(ConsoleServerInfo.PRODUCT_GUID);
        harmony.PatchAll(typeof(ChainloaderPatches));

        var managerGameObject = new GameObject("AvaloniaConsole") {
            hideFlags = HideFlags.HideAndDontSave,
        };
        managerGameObject.AddComponent<Manager>();
        Object.DontDestroyOnLoad(managerGameObject);
    }
}
