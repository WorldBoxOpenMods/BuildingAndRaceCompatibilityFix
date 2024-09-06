using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ai.behaviours;
using HarmonyLib;
using NeoModLoader.api;

namespace BuildingAndRaceCompatibilityFix {
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public class BuildingAndRaceCompatibilityFix : BasicMod<BuildingAndRaceCompatibilityFix> {
    protected override void OnModLoad() {
      LogInfo("BuildingAndRaceCompatibilityFix loading...");
      try {
        Harmony harmony = new Harmony("key.worldbox.buildingandracecompatibilityfix");
        harmony.Patch(
          AccessTools.Method(typeof(CityBehBuild), nameof(CityBehBuild.canUseBuildAsset)),
          finalizer: new HarmonyMethod(typeof(BuildingAndRaceCompatibilityFix), nameof(CityBehBuild_canUseBuildAsset_Finalizer))
        );
      } catch (Exception e) {
        LogError("BuildingAndRaceCompatibilityFix failed to load!");
        LogError(e.ToString());
      }
      LogInfo("BuildingAndRaceCompatibilityFix loaded!");
    }

    private static Exception CityBehBuild_canUseBuildAsset_Finalizer(Exception __exception, ref bool __result) {
      if (__exception is KeyNotFoundException || __exception is NullReferenceException) {
        __result = false;
        return null;
      }
      return __exception;
    }
  }
}