using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sigurd.AvaloniaBepInExConsole;

internal static class Bootstrap
{
    [UsedImplicitly]
    static void Start()
    {
        var managerGameObject = new GameObject("AvaloniaConsole") {
            hideFlags = HideFlags.HideAndDontSave,
        };
        managerGameObject.AddComponent<Manager>();
        Object.DontDestroyOnLoad(managerGameObject);
    }
}
