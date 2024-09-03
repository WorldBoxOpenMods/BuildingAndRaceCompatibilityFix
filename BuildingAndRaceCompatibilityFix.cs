using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ai.behaviours;
using EpPathFinding.cs;
using HarmonyLib;
using NeoModLoader.api;
using UnityEngine;

namespace BuildingAndRaceCompatibilityFix {
  public class BuildingAndRaceCompatibilityFix : BasicMod<BuildingAndRaceCompatibilityFix> {
    protected override void OnModLoad() {
      LogInfo("BuildingAndRaceCompatibilityFix loading...");
      try {
        
      } catch (System.Exception e) {
        LogError("BuildingAndRaceCompatibilityFix failed to load!");
        LogError(e.ToString());
      }
      LogInfo("BuildingAndRaceCompatibilityFix loaded!");
    }

    private static IEnumerable<CodeInstruction> MapAction_makeRoadBetween_Transpiler(IEnumerable<CodeInstruction> instructions) {
      foreach (CodeInstruction instruction in instructions) {
        yield return instruction;
        if (instruction.opcode == OpCodes.Stfld && instruction.operand is FieldInfo fieldInfo && fieldInfo.Name == nameof(AStarParam.roads)) {
          /* Reference:
             IL_003a: call         class MapBox World::get_world()
             IL_003f: ldfld        class EpPathFinding.cs.AStarParam MapBox::pathfindingParam
             IL_0044: ldc.i4.1
             IL_0045: stfld        bool EpPathFinding.cs.AStarParam::roads
           */
          yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(World), nameof(World.world)));
          yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(MapBox), nameof(MapBox.pathfindingParam)));
          yield return new CodeInstruction(OpCodes.Ldc_I4_1);
          yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(AStarParam), nameof(AStarParam.ocean)));
          yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(World), nameof(World.world)));
          yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(MapBox), nameof(MapBox.pathfindingParam)));
          yield return new CodeInstruction(OpCodes.Ldc_I4_1);
          yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(AStarParam), nameof(AStarParam.ground)));
        }
      }
    }

    private static IEnumerable<CodeInstruction> CityBehBuild_buildTick_Transpiler(IEnumerable<CodeInstruction> instructions) {
      bool found = false;
      bool triedAddingCall = false;
      foreach (CodeInstruction instruction in instructions) {
        yield return instruction;
        switch (found) {
          // check for this IL part:
          //    IL_0041: ldarg.0      // pCity
          case true when !triedAddingCall: {
            triedAddingCall = true;
            if (instruction.opcode == OpCodes.Ldarg_0) {
              yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuildingAndRaceCompatibilityFix), nameof(CheckBuildRoadsBetweenCities)));
              yield return new CodeInstruction(OpCodes.Ldarg_0);
            }
            break;
          }
          // check for this IL part:
          //    IL_003c: call         void ai.behaviours.CityBehBuild::makeRoadsBuildings(class City, class Building)
          case false when instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo methodInfo && methodInfo.Name == nameof(CityBehBuild.makeRoadsBuildings):
            found = true;
            break;
        }
      }
    }

    private static void CheckBuildRoadsBetweenCities(City origin) {
      List<City> otherCities = origin.kingdom.cities.Where(c => c != origin).ToList();
      List<City> viableCities = otherCities.Where(c => !c._cityTile.isSameIsland(origin._cityTile)).ToList();
      if (viableCities.Count > 0 && Random.value < 0.4f) {
        City target = viableCities.GetRandom();
        Building random = target.buildings.getSimpleList().GetRandom();
        CityBehBuild.makeRoadsBuildings(origin, random);
      }
    }
  }
}