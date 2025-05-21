/*
 * https://github.com/BepInEx/BepInEx/blob/6c5766d5abc230a4c9427ebb14acfca05255efb8/BepInEx.Preloader/Preloader.cs#L140C3-L222C4
 * Copyright (c) 2018 Bepis
 * Bepis licenses the basis of this file to the Sigurd Team under the MIT license.
 *
 * Copyright (c) 2024 Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Preloader;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static System.Reflection.Assembly;

namespace Sigurd.AvaloniaBepInExConsole.Patcher;

public static class AvaloniaBepInExPatcher
{
    private const string AvaloniaConsoleServerAssemblyName = "com.sigurd.avalonia_bepinex_console.server.dll";

    private static string ThisAssemblyPath {
        get {
            var path = GetExecutingAssembly().Location;
            // check for UNC path: https://stackoverflow.com/a/18907418/11045433
            if (path.StartsWith(new string(Path.DirectorySeparatorChar, 2)))
                throw new InvalidOperationException("Patcher plugin location should be a local filepath, not UNC");
            return path;
        }
    }

    private static IList<string> MetaLibraryRelativePathParts {
        get {
            var relativePath = Path.GetRelativePath(Paths.PatcherPluginPath, ThisAssemblyPath);
            if (relativePath == ThisAssemblyPath)
                throw new InvalidOperationException("Patcher plugin should be in the BepInEx patcher plugins directory");
            var relativePathParts = relativePath.Split(Path.DirectorySeparatorChar).ToList();
            relativePathParts.RemoveAt(relativePathParts.Count - 1);
            return relativePathParts;
        }
    }

    private static readonly IEnumerable<string> AvaloniaConsoleServerAssemblySearchPaths = [
        Path.Combine([Paths.BepInExAssemblyDirectory, ..MetaLibraryRelativePathParts]),
        Path.Combine(Paths.BepInExAssemblyDirectory, "AvaloniaBepInExConsole"),
        Paths.BepInExAssemblyDirectory,
    ];

    private static string? _avaloniaConsoleServerAssemblyPath;
    private static string? AvaloniaConsoleServerAssemblyPath => _avaloniaConsoleServerAssemblyPath ??= AvaloniaConsoleServerAssemblySearchPaths
        .Select(directoryPath => Path.Combine(directoryPath, AvaloniaConsoleServerAssemblyName))
        .FirstOrDefault(File.Exists);

    [UsedImplicitly]
    public static IEnumerable<string> TargetDLLs => [ Preloader.ConfigEntrypointAssembly.Value ];

    /// <summary>
    /// Inserts Avalonia BepInEx Console Server's entrypoint just before BepInEx's Chainloader.
	/// </summary>
	/// <param name="assembly">The assembly that the <see cref="AvaloniaBepInExPatcher"/> will attempt to patch.</param>
	[UsedImplicitly]
	public static void Patch(AssemblyDefinition assembly)
	{
        if (AvaloniaConsoleServerAssemblyPath is null)
            throw new InvalidOperationException("AvaloniaBepInExConsole.Server assembly could not be found. It should be in the BepInEx/core/ directory.");

		string entrypointType = Preloader.ConfigEntrypointType.Value;
		string entrypointMethod = Preloader.ConfigEntrypointMethod.Value;

		bool isCctor = entrypointMethod.IsNullOrWhiteSpace() || entrypointMethod == ".cctor";

		var entryType = assembly.MainModule.Types.FirstOrDefault(x => x.Name == entrypointType);

        if (entryType is null)
            return; // fail silently because BepInEx will throw an error anyway

        using var injected = AssemblyDefinition.ReadAssembly(AvaloniaConsoleServerAssemblyPath);
        var originalStartMethod = injected.MainModule.Types.First(x => x.Name == "Bootstrap").Methods
            .First(x => x.Name == "Start");

        var startMethod = assembly.MainModule.ImportReference(originalStartMethod);

        var methods = new List<MethodDefinition>();

        if (isCctor)
        {
            var cctor = entryType.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);

            if (cctor == null)
            {
                cctor = new MethodDefinition(".cctor",
                    MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig
                    | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    assembly.MainModule.ImportReference(typeof(void)));

                entryType.Methods.Add(cctor);
                var il = cctor.Body.GetILProcessor();
                il.Append(il.Create(OpCodes.Ret));
            }

            methods.Add(cctor);
        }
        else
        {
            methods.AddRange(entryType.Methods.Where(x => x.Name == entrypointMethod));
        }

        if (!methods.Any())
            return; // fail silently because BepInEx will throw an error anyway

        foreach (var method in methods)
        {
            var il = method.Body.GetILProcessor();

            var ins = il.Body.Instructions.First();

            il.InsertBefore(ins,
                il.Create(OpCodes.Call, startMethod));
        }
    }
}
