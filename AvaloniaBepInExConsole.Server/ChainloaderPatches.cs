using BepInEx.Bootstrap;
using Cysharp.Threading.Tasks;
using HarmonyLib;

namespace Sigurd.AvaloniaBepInExConsole;

[HarmonyPatch(typeof(Chainloader))]
public static class ChainloaderPatches
{
    private static UniTaskCompletionSource _chainloaderInitializedCompletionSource = new();
    public static UniTask ChainloaderInitialized => _chainloaderInitializedCompletionSource.Task;

    [HarmonyPatch(nameof(Chainloader.Initialize))]
    [HarmonyPostfix]
    public static void OnChainloaderInitialized() => _chainloaderInitializedCompletionSource.TrySetResult();
}
