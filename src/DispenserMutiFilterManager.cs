using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static MultipleDelivery_MOD.src.MultipleDelivery;

namespace MultipleDelivery_MOD.src
{
    internal class DispenserMutiFilterManager
    {
        // 静态只读实例，通过属性暴露
        private static readonly DispenserMutiFilterManager _instance = new DispenserMutiFilterManager();

        // 私有构造函数，禁止外部实例化
        private DispenserMutiFilterManager() { }

        // 全局访问点,太空电梯轨道设施配对管理器
        public static DispenserMutiFilterManager Instance => _instance;

        private static Dictionary<int, Dictionary<int, int[]>> DispenserMutiFilterdata = new Dictionary<int, Dictionary<int, int[]>>();
        private static Dictionary<int, Dictionary<int, int[]>> DispenserPlayerOrderCountdata = new Dictionary<int, Dictionary<int, int[]>>();
        private static Dictionary<int, Dictionary<int, int[]>> DispenserStorageOrderCountdata = new Dictionary<int, Dictionary<int, int[]>>();

        public Dictionary<int, int[]> GetMutiFilterdata(int planetId) => DispenserMutiFilterdata.GetValueSafe(planetId);
        public Dictionary<int, int[]> GetPlayerOrderdata(int planetId) => DispenserPlayerOrderCountdata.GetValueSafe(planetId);
        public Dictionary<int, int[]> GetStorageOrderdata(int planetId) => DispenserStorageOrderCountdata.GetValueSafe(planetId);

        [HarmonyPatch(typeof(PlanetTransport), nameof(PlanetTransport.NewDispenserComponent))]
        [HarmonyPostfix]
        public static void PlanetTransport_NewDispenserComponent_Patch(PlanetTransport __instance, int __result)
        {
            int planetId = __instance.planet.id;
            if (!DispenserMutiFilterdata.ContainsKey(planetId)) {
                DispenserMutiFilterdata[planetId] = new Dictionary<int, int[]>();
            }
            DispenserMutiFilterdata[planetId][__result] = new int[] { 0, 0, 0, 0, 0 };
            if (!DispenserPlayerOrderCountdata.ContainsKey(planetId)) {
                DispenserPlayerOrderCountdata[planetId] = new Dictionary<int, int[]>();
            }
            DispenserPlayerOrderCountdata[planetId][__result] = new int[] { 0, 0, 0, 0, 0 };
            if (!DispenserStorageOrderCountdata.ContainsKey(planetId)) {
                DispenserStorageOrderCountdata[planetId] = new Dictionary<int, int[]>();
            }
            DispenserStorageOrderCountdata[planetId][__result] = new int[] { 0, 0, 0, 0, 0 };
        }

        [HarmonyPatch(typeof(PlanetTransport), nameof(PlanetTransport.RemoveDispenserComponent))]
        [HarmonyPostfix]
        public static void PlanetTransport_RemoveDispenserComponent_Patch(PlanetTransport __instance, int id)
        {
            int planetId = __instance.planet.id;
            if (!DispenserMutiFilterdata.ContainsKey(planetId)) {
                return;
            }
            if (!DispenserMutiFilterdata[planetId].ContainsKey(id)) {
                return;
            }
            DispenserMutiFilterdata[planetId].Remove(id);
            if (!DispenserPlayerOrderCountdata.ContainsKey(planetId)) {
                return;
            }
            if (!DispenserPlayerOrderCountdata[planetId].ContainsKey(id)) {
                return;
            }
            DispenserPlayerOrderCountdata[planetId].Remove(id);
            if (!DispenserStorageOrderCountdata.ContainsKey(planetId)) {
                return;
            }
            if (!DispenserStorageOrderCountdata[planetId].ContainsKey(id)) {
                return;
            }
            DispenserStorageOrderCountdata[planetId].Remove(id);
        }


        public void SetDispenserFilter(int planetId, int dispenserId, int filterId, int filter)
        {
            if (planetId == 0 || dispenserId == 0) {
                return;
            }
            if (filter != 0) {
                int[] ints = DispenserMutiFilterdata.GetValueSafe(planetId).GetValueSafe(dispenserId);
                for (int i = 0; i < ints.Length; i++) {
                    if (i != filterId) {
                        if (ints[i] == filter) {
                            return;
                        }
                    }
                }
            }
            if (DispenserMutiFilterdata[planetId][dispenserId][filterId] != filter) {
                DispenserMutiFilterdata[planetId][dispenserId][filterId] = filter;
            }
            LogError($"SetDispenserFilter planetId {planetId} dispenserId {dispenserId} filterId {filterId} filter {filter}");
        }
        [HarmonyPatch(typeof(PlanetTransport), nameof(PlanetTransport.SetDispenserFilter))]
        [HarmonyPrefix]
        public static bool PlanetTransport_SetDispenserFilter_Patch(PlanetTransport __instance, int dispenserId, int filter)
        {
            if (filter == 0) {
                return true;
            }
            int planetId = __instance.planet.id;
            if (!DispenserMutiFilterdata.ContainsKey(planetId)) {
                return true;
            }
            if (!DispenserMutiFilterdata[planetId].ContainsKey(dispenserId)) {
                return true;
            }
            int[] ints = DispenserMutiFilterdata.GetValueSafe(planetId).GetValueSafe(dispenserId);
            for (int i = 0; i < 5; i++) {
                if (ints[i] == filter) {
                    return false;
                }
            }
            return true;
        }


        internal static void Export(BinaryWriter w)
        {
            w.Write(DispenserMutiFilterdata.Count);
            foreach (var outerPair in DispenserMutiFilterdata) {
                // 写入planetId
                w.Write(outerPair.Key);
                // 写入总配送器数
                w.Write(outerPair.Value.Count);
                foreach (var pair in outerPair.Value) {
                    // 写入dispenserId
                    w.Write(pair.Key);
                    for (int i = 0; i < 5; i++) {
                        // 写入每个过滤器的值
                        w.Write(pair.Value[i]);
                    }
                }
            }
            w.Write(DispenserPlayerOrderCountdata.Count);
            foreach (var outerPair in DispenserPlayerOrderCountdata) {
                // 写入planetId
                w.Write(outerPair.Key);
                // 写入总配送器数
                w.Write(outerPair.Value.Count);
                foreach (var pair in outerPair.Value) {
                    // 写入dispenserId
                    w.Write(pair.Key);
                    for (int i = 0; i < 5; i++) {
                        // 写入每个物品的playerorder数量
                        w.Write(pair.Value[i]);
                    }
                }
            }
            w.Write(DispenserStorageOrderCountdata.Count);
            foreach (var outerPair in DispenserStorageOrderCountdata) {
                // 写入planetId
                w.Write(outerPair.Key);
                // 写入总配送器数
                w.Write(outerPair.Value.Count);
                foreach (var pair in outerPair.Value) {
                    // 写入dispenserId
                    w.Write(pair.Key);
                    for (int i = 0; i < 5; i++) {
                        // 写入每个物品的StorageOrder数量
                        w.Write(pair.Value[i]);
                    }
                }
            }
        }

        internal static void Import(BinaryReader r)
        {
            IntoOtherSave();
            try {
                int DispenserMutiFilterdataCount = r.ReadInt32();
                for (int i = 0; i < DispenserMutiFilterdataCount; i++) {
                    int planetId = r.ReadInt32();
                    int dispenserCount = r.ReadInt32();
                    for (int j = 0; j < dispenserCount; j++) {
                        int dispenserId = r.ReadInt32();
                        int[] filters = new int[5];
                        for (int k = 0; k < 5; k++) {
                            filters[k] = r.ReadInt32();
                        }
                        if (!DispenserMutiFilterdata.ContainsKey(planetId)) {
                            DispenserMutiFilterdata[planetId] = new Dictionary<int, int[]>();
                        }
                        DispenserMutiFilterdata[planetId][dispenserId] = filters;
                    }
                }
                int DispenserPlayerOrderCountdataCount = r.ReadInt32();
                for (int i = 0; i < DispenserPlayerOrderCountdataCount; i++) {
                    int planetId = r.ReadInt32();
                    int dispenserCount = r.ReadInt32();
                    for (int j = 0; j < dispenserCount; j++) {
                        int dispenserId = r.ReadInt32();
                        int[] filters = new int[5];
                        for (int k = 0; k < 5; k++) {
                            filters[k] = r.ReadInt32();
                        }
                        if (!DispenserPlayerOrderCountdata.ContainsKey(planetId)) {
                            DispenserPlayerOrderCountdata[planetId] = new Dictionary<int, int[]>();
                        }
                        DispenserPlayerOrderCountdata[planetId][dispenserId] = filters;
                    }
                }
                int DispenserStorageOrderCountdataCount = r.ReadInt32();
                for (int i = 0; i < DispenserStorageOrderCountdataCount; i++) {
                    int planetId = r.ReadInt32();
                    int dispenserCount = r.ReadInt32();
                    for (int j = 0; j < dispenserCount; j++) {
                        int dispenserId = r.ReadInt32();
                        int[] StorageOrdereds = new int[5];
                        for (int k = 0; k < 5; k++) {
                            StorageOrdereds[k] = r.ReadInt32();
                        }
                        if (!DispenserStorageOrderCountdata.ContainsKey(planetId)) {
                            DispenserStorageOrderCountdata[planetId] = new Dictionary<int, int[]>();
                        }
                        DispenserStorageOrderCountdata[planetId][dispenserId] = StorageOrdereds;
                    }
                }
            } catch (EndOfStreamException) {
                // ignored
            }
        }

        internal static void IntoOtherSave()
        {
            DispenserMutiFilterdata.Clear();
            DispenserPlayerOrderCountdata.Clear();
            DispenserStorageOrderCountdata.Clear();
        }

        [HarmonyPatch(typeof(PlanetTransport), nameof(PlanetTransport.Import))]
        [HarmonyPostfix]
        public static void PlanetTransport_Import_Patch(PlanetTransport __instance)
        {
            int planetId = __instance.planet.id;
            if (!DispenserMutiFilterdata.ContainsKey(planetId)) {
                DispenserMutiFilterdata[planetId] = new Dictionary<int, int[]>();
            }
            for (int m = 1; m < __instance.dispenserCursor; m++) {
                if (!DispenserMutiFilterdata[planetId].ContainsKey(m)) {
                    DispenserMutiFilterdata[planetId][m] = new int[] { 0, 0, 0, 0, 0 };
                }
            }
            if (!DispenserPlayerOrderCountdata.ContainsKey(planetId)) {
                DispenserPlayerOrderCountdata[planetId] = new Dictionary<int, int[]>();
            }
            for (int m = 1; m < __instance.dispenserCursor; m++) {
                if (!DispenserPlayerOrderCountdata[planetId].ContainsKey(m)) {
                    DispenserPlayerOrderCountdata[planetId][m] = new int[] { 0, 0, 0, 0, 0 };
                }
            }
            if (!DispenserStorageOrderCountdata.ContainsKey(planetId)) {
                DispenserStorageOrderCountdata[planetId] = new Dictionary<int, int[]>();
            }
            for (int m = 1; m < __instance.dispenserCursor; m++) {
                if (!DispenserStorageOrderCountdata[planetId].ContainsKey(m)) {
                    DispenserStorageOrderCountdata[planetId][m] = new int[] { 0, 0, 0, 0, 0 };
                }
            }
        }
    }
}
