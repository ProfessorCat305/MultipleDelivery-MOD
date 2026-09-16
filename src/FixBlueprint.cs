using HarmonyLib;
using static MultipleDelivery_MOD.src.MultipleDelivery;

namespace MultipleDelivery_MOD.src
{
    internal class FixBlueprint
    {
        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.CopyFromFactoryObject))]
        [HarmonyPostfix]
        public static void BuildingParameters_CopyFromFactoryObject_Patch(ref BuildingParameters __instance, int objectId, PlanetFactory factory)
        {
            EntityData[] entityPool = factory.entityPool;
            int dispenserId = entityPool[objectId].dispenserId;
            if (dispenserId != 0) {
                //__instance.type = BuildingType.Dispenser;
                DispenserComponent[] dispenserPool = factory.transport.dispenserPool;
                //__instance.parameters = new int[128];
                //128个int，只使用了前4个，本mod使用了120-124，分别对应5个过滤器的物品ID
                DispenserComponent dispenserComponent = factory.transport.dispenserPool[dispenserId];

                int[] filterData = DispenserMutiFilterManager.Instance.GetMutiFilterdata(factory.planet.id)[dispenserId];
                for (int i = 0; i < 5; i++) {
                    __instance.parameters[120 + i] = filterData[i];
                }
            }
        }

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.ToParamsArray))]
        [HarmonyPostfix]
        public static void BuildingParameters_ToParamsArray_Patch(ref BuildingParameters __instance, ref int[] _parameters, ref int _paramCount)
        {
            if (__instance.type == BuildingType.Dispenser) {
                if (__instance.parameters != null && __instance.parameters.Length >= 128) {
                    _parameters[120] = __instance.parameters[120];
                    _parameters[121] = __instance.parameters[121];
                    _parameters[122] = __instance.parameters[122];
                    _parameters[123] = __instance.parameters[123];
                    _parameters[124] = __instance.parameters[124];
                    //LogError($"ToParamsArray");
                }
            }
        }

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.ApplyPrebuildParametersToEntity))]
        [HarmonyPostfix]
        public static void BuildingParameters_ApplyPrebuildParametersToEntity_Patch(int entityId, int recipeId, int filterId, int[] parameters, string content, PlanetFactory factory)
        {
            //LogError($"ApplyPrebuildParametersToEntity");
            EntityData[] entityPool = factory.entityPool;
            if (entityId > 0 && entityPool[entityId].id == entityId) {
                int dispenserId = entityPool[entityId].dispenserId;
                if (dispenserId != 0 && parameters != null && parameters.Length >= 128) {
                    DispenserComponent[] dispenserPool = factory.transport.dispenserPool;
                    for (int i = 0; i < 5; i++) {
                        DispenserMutiFilterManager.Instance.SetDispenserFilter(factory.planetId, dispenserId, i, parameters[120 + i]);
                        //LogError($"ApplyPrebuildParametersToEntity i {i} {parameters[120 + i]}");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.PasteToFactoryObject))]
        [HarmonyPostfix]
        public static void BuildingParameters_PasteToFactoryObject_Patch(ref BuildingParameters __instance, int objectId, PlanetFactory factory)
        {
            //LogError($"PasteToFactoryObject");
            EntityData[] entityPool = factory.entityPool;
            if (objectId > 0 && entityPool[objectId].id == objectId) {
                int dispenserId = entityPool[objectId].dispenserId;
                if (dispenserId != 0 && __instance.type == BuildingType.Dispenser && __instance.parameters != null && __instance.parameters.Length >= 128) {
                    DispenserComponent[] dispenserPool = factory.transport.dispenserPool;
                    for (int i = 0; i < 5; i++) {
                        DispenserMutiFilterManager.Instance.SetDispenserFilter(factory.planetId, dispenserId, i, __instance.parameters[120 + i]);
                        //LogError($"PasteToFactoryObject i {i} {__instance.parameters[120 + i]}");
                    }
                }
            }
        }
    }
}
