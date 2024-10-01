using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ai.behaviours;
using HarmonyLib;
using NeoModLoader.api;

namespace BuildingAndRaceCompatibilityFix {
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public class BuildingAndRaceCompatibilityFix : BasicMod<BuildingAndRaceCompatibilityFix> {
    private static byte _initCounter = 0;
    protected override void OnModLoad() {
      LogInfo("BuildingAndRaceCompatibilityFix loading...");
      try {
        Harmony harmony = new Harmony("key.worldbox.buildingandracecompatibilityfix");
        harmony.Patch(
          AccessTools.Method(typeof(CityBehBuild), nameof(CityBehBuild.canUseBuildAsset)),
          finalizer: new HarmonyMethod(typeof(BuildingAndRaceCompatibilityFix), nameof(CityBehBuild_canUseBuildAsset_Finalizer))
        );
      } catch (Exception e) {
        LogError("BuildingAndRaceCompatibilityFix failed to load fully!");
        LogError(e.ToString());
      }
      LogInfo("BuildingAndRaceCompatibilityFix loaded!");
    }

    private void Update() {
      if (_initCounter == 4) {
        foreach (Type type in NeoModLoader.WorldBoxMod.LoadedMods.Select(mod => mod.GetType()).Select(modType => modType.Assembly).SelectMany(modAssembly => modAssembly.GetTypes())) {
          LogInfo($"Checking {type.FullName} for aggressive getBuildingAsset() log patches...");
          foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(method => method.Name == "getBuildingAsset_Prefix")) {
            LogInfo($"Found {type.FullName}.{method.Name}!");
            try {
              Harmony harmony = new Harmony($"key.worldbox.{type.Name}.auto_patch");
              harmony.Patch(
                method,
                transpiler: new HarmonyMethod(typeof(BuildingAndRaceCompatibilityFix), nameof(RemoveLoggingFromMethod))
              );
            } catch (Exception e) {
              LogError($"Failed to patch {type.FullName}.{method.Name}!");
              LogError(e.ToString());
            }
          }
        }
      } else if (_initCounter < 4) _initCounter++;
    }

    private static Exception CityBehBuild_canUseBuildAsset_Finalizer(Exception __exception, ref bool __result) {
      if (__exception is KeyNotFoundException || __exception is NullReferenceException) {
        __result = false;
        return null;
      }
      return __exception;
    }
    
    private static IEnumerable<CodeInstruction> RemoveLoggingFromMethod(IEnumerable<CodeInstruction> instructions) {
      foreach (CodeInstruction instruction in instructions) {
        if (instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo methodInfo && methodInfo.Name == "Log") {
          yield return new CodeInstruction(OpCodes.Pop);
        } else {
          yield return instruction;
        }
      }
    }
  }
}