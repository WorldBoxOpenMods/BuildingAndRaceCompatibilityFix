using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ai.behaviours;
using HarmonyLib;
using NCMS;
using NeoModLoader.api;
using UnityEngine;

namespace BuildingAndRaceCompatibilityFix {
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public class BuildingAndRaceCompatibilityFix : BasicMod<BuildingAndRaceCompatibilityFix> {
    private static byte _initCounter;
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
        foreach (Type type in NeoModLoader.WorldBoxMod.LoadedMods.Select(mod => {
                   switch (mod) {
                     case AttachedModComponent ncmsMod:
                       foreach (MonoBehaviour component in ncmsMod.gameObject.GetComponents<MonoBehaviour>()) {
                         try {
                           if (HasModEntryAttribute(component)) {
                             return component.GetType();
                           }
                         } catch (Exception e) {
                           if (e is TypeLoadException) continue;
                           LogError($"Failed to check {component.GetType().FullName} for ModEntry attribute!");
                           LogError(e.ToString());
                         }
                       }
                       return null;
                     case VirtualMod virtualMod:
                       GameObject bepinexManager = GameObject.Find("BepInEx_Manager");
                       if (bepinexManager == null) {
                         return null;
                       }
                       Component[] bepinexComponents = bepinexManager.GetComponents(typeof(Component));
                       foreach (Component component in bepinexComponents.Where(component => (component.GetType().FullName ?? "").Contains(virtualMod.GetDeclaration().Name)).Where(component => IsBaseUnityPluginDerivative(component as MonoBehaviour))) {
                         return component.GetType();
                       }
                       return null;
                     default:
                       return mod.GetType();
                   }
                 }).Where(modType => modType != null).Select(modType => modType.Module).SelectMany(modModule => modModule.GetTypes())) {
          foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(method => method.Name == "getBuildingAsset_Prefix")) {
            LogInfo($"Patching {type.FullName}.{method.Name} to remove debug logs.");
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
      }
      if (_initCounter++ <= 4) { }
    }

    private static bool HasModEntryAttribute(MonoBehaviour component) {
#pragma warning disable CS0618 // Type or member is obsolete
      return Attribute.GetCustomAttribute(component.GetType(), typeof(ModEntry)) != null;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private static bool IsBaseUnityPluginDerivative(MonoBehaviour component) {
      Type type = component.GetType();
      while (type != null) {
        if (type.Name == "BaseUnityPlugin") return true;
        type = type.BaseType;
      }
      return false;
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