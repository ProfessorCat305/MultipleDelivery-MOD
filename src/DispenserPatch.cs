using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultipleDelivery_MOD.src
{
    internal class DispenserPatch
    {
        [HarmonyPatch(typeof(PlanetTransport), nameof(PlanetTransport.RefreshDispenserTraffic))]
        [HarmonyPrefix]
        public static bool PlanetTransport_RefreshDispenserTraffic_Patch(PlanetTransport __instance, int keyId)
        {
            int logisticCourierCarries = GameMain.history.logisticCourierCarries;
            __instance.playerDeliveryPackage.ClearPairs();
            for (int i = 1; i < __instance.dispenserCursor; i++) {
                if (__instance.dispenserPool[i] != null && __instance.dispenserPool[i].id == i) {
                    __instance.dispenserPool[i].ClearPairs();
                }
            }
            if (__instance.playerDeliveryEnabled) {
                DeliveryPackage.GRID[] grids = __instance.playerDeliveryPackage.grids;
                for (int j = 0; j < grids.Length; j++) {
                    __instance.playerDeliveryPackage.gridsPairOffsets[j] = __instance.playerDeliveryPackage.pairCount;
                    if (grids[j].itemId > 0 && __instance.playerDeliveryPackage.IsGridActive(j)) {
                        int itemId = grids[j].itemId;
                        //if (itemId != 1099) {
                        for (int k = 1; k < __instance.dispenserCursor; k++) {
                            DispenserComponent dispenserComponent = __instance.dispenserPool[k];
                            if (dispenserComponent != null && dispenserComponent.id == k) {
                                if ((dispenserComponent.playerMode == EPlayerDeliveryMode.Supply || dispenserComponent.playerMode == EPlayerDeliveryMode.Both) && itemId == dispenserComponent.filter) {
                                    dispenserComponent.AddPair(k, 0, -(j + 1), j);
                                    __instance.playerDeliveryPackage.AddGridPair(k, 0, -(j + 1), j);
                                }
                                if ((dispenserComponent.playerMode == EPlayerDeliveryMode.Recycle || dispenserComponent.playerMode == EPlayerDeliveryMode.Both) && (itemId == dispenserComponent.filter || (dispenserComponent.filter < 0 && itemId > 0))) {
                                    dispenserComponent.AddPair(-(j + 1), j, k, 0);
                                    __instance.playerDeliveryPackage.AddGridPair(-(j + 1), j, k, 0);
                                }
                                Dictionary<int, int[]> planetDispenserData = DispenserMutiFilterManager.Instance.GetMutiFilterdata(__instance.planet.id);
                                if (planetDispenserData == null) {
                                    continue;
                                }
                                if (!planetDispenserData.ContainsKey(dispenserComponent.id)) {
                                    continue;
                                }
                                int[] filterData = planetDispenserData[dispenserComponent.id];
                                for (int x = 0; x < 5; x++) {
                                    if ((dispenserComponent.playerMode == EPlayerDeliveryMode.Supply || dispenserComponent.playerMode == EPlayerDeliveryMode.Both) && itemId == filterData[x]) {
                                        dispenserComponent.AddPair(k, 0, -(j + 1), j);
                                        __instance.playerDeliveryPackage.AddGridPair(k, 0, -(j + 1), j);
                                    }
                                    if ((dispenserComponent.playerMode == EPlayerDeliveryMode.Recycle || dispenserComponent.playerMode == EPlayerDeliveryMode.Both) && (itemId == filterData[x] || (filterData[x] < 0 && itemId > 0))) {
                                        dispenserComponent.AddPair(-(j + 1), j, k, 0);
                                        __instance.playerDeliveryPackage.AddGridPair(-(j + 1), j, k, 0);
                                    }
                                }
                            }
                        }
                        //}
                    }
                }
            }
            for (int l = 1; l < __instance.dispenserCursor; l++) {
                DispenserComponent dispenserComponent2 = __instance.dispenserPool[l];
                if (dispenserComponent2 != null && dispenserComponent2.id == l) {
                    Dictionary<int, int[]> planetDispenserData = DispenserMutiFilterManager.Instance.GetMutiFilterdata(__instance.planet.id);
                    if (planetDispenserData == null) {
                        continue;
                    }
                    if (!planetDispenserData.ContainsKey(dispenserComponent2.id)) {
                        continue;
                    }
                    int[] filterData2 = planetDispenserData[dispenserComponent2.id];
                    // 遍历A的6个数字，挨个和B的6个比对
                    int filter;
                    for (int ai = 0; ai < 6; ai++) {
                        filter = ai == 0 ? dispenserComponent2.filter : filterData2[ai - 1];
                        if (filter > 0) {
                            if (dispenserComponent2.storageMode == EStorageDeliveryMode.Supply) {
                                for (int m = l + 1; m < __instance.dispenserCursor; m++) {
                                    DispenserComponent dispenserComponent3 = __instance.dispenserPool[m];
                                    if (dispenserComponent3 != null && dispenserComponent3.id == m && dispenserComponent3.storageMode == EStorageDeliveryMode.Demand) {
                                        int[] filterData3 = DispenserMutiFilterManager.Instance.GetMutiFilterdata(__instance.planet.id)[dispenserComponent3.id];
                                        for (int bi = 0; bi < 6; bi++) {
                                            int filterB = bi == 0 ? dispenserComponent3.filter : filterData3[bi - 1];
                                            if (filter == filterB) {
                                                dispenserComponent2.AddPair(l, ai, m, bi);
                                                dispenserComponent3.AddPair(l, ai, m, bi);
                                            }
                                        }
                                    }
                                }
                            } else if (dispenserComponent2.storageMode == EStorageDeliveryMode.Demand) {
                                for (int n = l + 1; n < __instance.dispenserCursor; n++) {
                                    DispenserComponent dispenserComponent4 = __instance.dispenserPool[n];
                                    if (dispenserComponent4 != null && dispenserComponent4.id == n && dispenserComponent4.storageMode == EStorageDeliveryMode.Supply) {
                                        int[] filterData4 = DispenserMutiFilterManager.Instance.GetMutiFilterdata(__instance.planet.id)[dispenserComponent4.id];
                                        for (int bi = 0; bi < 6; bi++) {
                                            int filterB = bi == 0 ? dispenserComponent4.filter : filterData4[bi - 1];
                                            if (filter == filterB) {
                                                dispenserComponent2.AddPair(n, bi, l, ai);
                                                dispenserComponent4.AddPair(n, bi, l, ai);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    dispenserComponent2.OnRematchPairs(__instance.factory, __instance.dispenserPool, keyId, logisticCourierCarries);
                }
            }
            if (PlanetTransport.onFactoryRefreshDispenserTraffic != null) {
                PlanetTransport.onFactoryRefreshDispenserTraffic.Invoke(__instance.factory, keyId);
            }
            return false;
        }

        //[HarmonyPatch(typeof(DispenserComponent), nameof(DispenserComponent.InternalTick))]
        //[HarmonyTranspiler]
        //public static IEnumerable<CodeInstruction> DispenserComponent_InternalTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var matcher = new CodeMatcher(instructions);

        //    matcher.MatchForward(true,
        //        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(DispenserComponent), nameof(DispenserComponent.filter))));
        //    matcher.Advance(10);

        //    matcher.MatchForward(true,
        //        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(DispenserComponent), nameof(DispenserComponent.filter))));

        //    object itemId = matcher.Advance(-2).Operand;
        //    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop));
        //    object IL_081b = matcher.Advance(2).Operand;

        //    matcher.Advance(-1).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_1));
        //    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, itemId));
        //    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
        //        AccessTools.Method(typeof(DispenserPatch), nameof(CheckItemId))));
        //    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Brfalse, IL_081b));

        //    matcher.MatchForward(true,
        //        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(DispenserComponent), nameof(DispenserComponent.filter))));
        //    matcher.Advance(10);

        //    matcher.MatchForward(true,
        //        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(DispenserComponent), nameof(DispenserComponent.filter))));

        //    object itemId2 = matcher.Advance(-2).Operand;
        //    object IL_0533 = matcher.Advance(4).Operand;

        //    matcher.Advance(-2).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_1));
        //    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, itemId2));
        //    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
        //        AccessTools.Method(typeof(DispenserPatch), nameof(CheckItemId))));
        //    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Brtrue, IL_0533));

        //    matcher.MatchForward(true,
        //        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(DispenserComponent), nameof(DispenserComponent.filter))));
        //    matcher.Advance(10);

        //    matcher.MatchForward(true,
        //        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(DispenserComponent), nameof(DispenserComponent.filter))));

        //    object IL_1049 = matcher.Advance(2).Operand;

        //    matcher.Advance(-2).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_1));
        //    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
        //        AccessTools.Method(typeof(DispenserPatch), nameof(CheckHasFilter))));
        //    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Brfalse, IL_1049));




        //    //matcher.LogInstructionEnumeration();
        //    return matcher.InstructionEnumeration();
        //}

        public static bool CheckItemId(DispenserComponent dispenser, PlanetFactory factory, int itemId)
        {
            if (itemId == dispenser.filter) return true;
            int[] Filterdata = DispenserMutiFilterManager.Instance.GetMutiFilterdata(factory.planet.id)[dispenser.id];
            for (int i = 0; i < 5; i++) {
                if (itemId == Filterdata[i]) {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckHasFilter(DispenserComponent dispenser, PlanetFactory factory)
        {
            if (dispenser.filter > 0) return true;
            int[] Filterdata = DispenserMutiFilterManager.Instance.GetMutiFilterdata(factory.planet.id)[dispenser.id];
            for (int i = 0; i < 5; i++) {
                if (Filterdata[i] > 0) {
                    return true;
                }
            }
            return false;
        }

        public static int GetItemPlayerOrdered(DispenserComponent dispenser, PlanetFactory factory, int itemId)
        {
            if (itemId == dispenser.filter) {
                return dispenser.playerOrdered;
            }
            int[] Filterdata = DispenserMutiFilterManager.Instance.GetMutiFilterdata(factory.planet.id)[dispenser.id];
            for (int i = 0; i < 5; i++) {
                if (Filterdata[i] == itemId) {
                    int[] orderedData = DispenserMutiFilterManager.Instance.GetPlayerOrderdata(factory.planet.id)[dispenser.id];
                    return orderedData[i];
                }
            }
            return 0;
        }
        public static int GetItemStorageOrdered(DispenserComponent dispenser, PlanetFactory factory, int itemId)
        {
            if (itemId == dispenser.filter) {
                return dispenser.storageOrdered;
            }
            int[] Filterdata = DispenserMutiFilterManager.Instance.GetMutiFilterdata(factory.planet.id)[dispenser.id];
            for (int i = 0; i < 5; i++) {
                if (Filterdata[i] == itemId) {
                    int[] orderedData = DispenserMutiFilterManager.Instance.GetStorageOrderdata(factory.planet.id)[dispenser.id];
                    return orderedData[i];
                }
            }
            return 0;
        }

        public static void AddPlayerOrdered(DispenserComponent dispenser, PlanetFactory factory, int itemId, int orderCount)
        {
            if (itemId == dispenser.filter) {
                dispenser.playerOrdered += orderCount;
                return;
            }
            int[] Filterdata = DispenserMutiFilterManager.Instance.GetMutiFilterdata(factory.planet.id)[dispenser.id];
            int[] orderedData = DispenserMutiFilterManager.Instance.GetPlayerOrderdata(factory.planet.id)[dispenser.id];
            for (int i = 0; i < 5; i++) {
                if (Filterdata[i] == itemId) {
                    orderedData[i] += orderCount;
                    return;
                } else if (Filterdata[i] == 0 && orderedData[i] != 0) {
                    orderedData[i] += orderCount;
                    return;
                }
            }
        }
        public static void AddStorageOrdered(DispenserComponent dispenser, PlanetFactory factory, int itemId, int orderCount)
        {
            if (itemId == dispenser.filter) {
                dispenser.storageOrdered += orderCount;
                return;
            }
            int[] Filterdata = DispenserMutiFilterManager.Instance.GetMutiFilterdata(factory.planet.id)[dispenser.id];
            int[] orderedData = DispenserMutiFilterManager.Instance.GetStorageOrderdata(factory.planet.id)[dispenser.id];
            for (int i = 0; i < 5; i++) {
                if (Filterdata[i] == itemId) {
                    orderedData[i] += orderCount;
                    return;
                } else if (Filterdata[i] == 0 && orderedData[i] != 0) {
                    orderedData[i] += orderCount;
                    return;
                }
            }
        }

        // 缓存私有字段反射信息，只查找一次（性能关键！不要写在方法内）
        private static readonly FieldInfo _tmp_iter_Field = AccessTools.Field(typeof(DispenserComponent), "_tmp_iter");

        [HarmonyPatch(typeof(DispenserComponent), nameof(DispenserComponent.InternalTick))]
        [HarmonyPrefix]
        public static bool DispenserComponent_InternalTick(DispenserComponent __instance, PlanetFactory factory, EntityData[] entityPool, DispenserComponent[] dispenserPool, Vector3 playerPos, long time, float power, float courierSpeed, int courierCarries, double deliveryRange)
        {
            //if (__instance.filter == 1099) {
            //    factory.transport.SetDispenserFilter(__instance.id, 0);
            //}
            __instance.energy += (long)((int)((float)__instance.energyPerTick * power));
            __instance.energy -= 300L;
            if (__instance.energy > __instance.energyMax) {
                __instance.energy = __instance.energyMax;
            } else if (__instance.energy < 0L) {
                __instance.energy = 0L;
            }
            if (__instance.storage == null) {
                return false;
            }
            int num = __instance.storage.bottomStorage.entityId;
            Vector3 pos = entityPool[__instance.entityId].pos;
            DeliveryPackage.GRID[] grids = __instance.deliveryPackage.grids;
            int num2 = (int)(time % 3L);
            if (num2 < 0) {
                num2 += 3;
            }
            if (num2 == __instance.gene) {
                __instance.playerDeliveryCondition = DispenserComponent.EPlayerDeliveryCondition.Traversed;

                int currentGene = (int)_tmp_iter_Field.GetValue(__instance);
                // 业务逻辑：满足条件就自增（和原版 this._tmp_iter++ 一致）
                currentGene++;
                // ========== 写 ==========
                _tmp_iter_Field.SetValue(__instance, currentGene);

                if (__instance.idleCourierCount > 0) {
                    __instance.playerDeliveryCondition = DispenserComponent.EPlayerDeliveryCondition.HasCourier;
                    bool flag = false;
                    if (__instance.playerPairCount > 0) {
                        __instance.playerDeliveryCondition = DispenserComponent.EPlayerDeliveryCondition.HasPair;
                        double num3;
                        bool flag2 = __instance.CheckDeliveryRange(pos, playerPos, deliveryRange, out num3);
                        long num4 = (long)(num3 * 10000.0 * 2.0 + 100000.0);
                        if (flag2) {
                            __instance.playerDeliveryCondition = DispenserComponent.EPlayerDeliveryCondition.InRange;
                        }
                        if (__instance.energy >= num4 && flag2) {
                            __instance.playerDeliveryCondition = DispenserComponent.EPlayerDeliveryCondition.EnergyEnough;
                            for (int i = 0; i < __instance.playerPairCount; i++) {
                                ref SupplyDemandPair ptr = ref __instance.pairs[i];
                                ptr.runtimeState = 0;
                                if (ptr.supplyId == __instance.id) {
                                    int demandIndex = ptr.demandIndex;
                                    if (demandIndex >= 100) {
                                        Assert.CannotBeReached();
                                    } else {
                                        Assert.True(-(ptr.demandId + 1) == demandIndex);
                                        int itemId = grids[demandIndex].itemId;
                                        // 修改部分============
                                        if (CheckItemId(__instance, factory, itemId)) {
                                            // ================
                                            ptr.runtimeState = 1;
                                            int packageItemCountIncludeHandItem = __instance.packageUtility.GetPackageItemCountIncludeHandItem(itemId);
                                            int num5 = grids[demandIndex].modifiedCount + packageItemCountIncludeHandItem;
                                            int num6 = grids[demandIndex].count + packageItemCountIncludeHandItem;
                                            int num7 = (num5 > num6) ? num5 : num6;
                                            int num8 = grids[demandIndex].stackSizeModified - grids[demandIndex].modifiedCount + __instance.packageUtility.GetPackageItemCapacity(itemId);
                                            int clampedRequireCount = grids[demandIndex].clampedRequireCount;
                                            if (num7 < clampedRequireCount) {
                                                ptr.runtimeState = 2;
                                                if (num8 > 0) {
                                                    ptr.runtimeState = 3;
                                                    int num9 = clampedRequireCount - num7;
                                                    num9 = ((num9 > courierCarries) ? courierCarries : num9);
                                                    num9 = ((num9 > num8) ? num8 : num9);

                                                    int storageOrdered = GetItemStorageOrdered(__instance, factory, itemId);

                                                    int num10 = (storageOrdered > 0) ? 0 : storageOrdered;
                                                    num9 = __instance.PickFromStoragePrecalc(itemId, num9 - num10);
                                                    num9 += num10;
                                                    if (num9 > 0) {
                                                        int inc;
                                                        int num11 = factory.PickFromStorage(num, itemId, num9, out inc);
                                                        if (num11 > 0) {
                                                            ptr.runtimeState = 4;
                                                            __instance.workCourierDatas[__instance.workCourierCount].begin = pos;
                                                            __instance.workCourierDatas[__instance.workCourierCount].end = pos;
                                                            __instance.workCourierDatas[__instance.workCourierCount].endId = ptr.demandId;
                                                            __instance.workCourierDatas[__instance.workCourierCount].direction = 1f;
                                                            __instance.workCourierDatas[__instance.workCourierCount].maxt = 1f;
                                                            __instance.workCourierDatas[__instance.workCourierCount].t = 0f;
                                                            __instance.workCourierDatas[__instance.workCourierCount].itemId = itemId;
                                                            __instance.workCourierDatas[__instance.workCourierCount].itemCount = num11;
                                                            __instance.workCourierDatas[__instance.workCourierCount].inc = inc;
                                                            // 修改部分============
                                                            __instance.workCourierDatas[__instance.workCourierCount].gene = currentGene;
                                                            //=====================
                                                            __instance.orders[__instance.workCourierCount].itemId = itemId;
                                                            __instance.orders[__instance.workCourierCount].otherId = ptr.demandId;
                                                            __instance.orders[__instance.workCourierCount].thisOrdered = 0;
                                                            __instance.orders[__instance.workCourierCount].otherOrdered = num11;
                                                            __instance.playerOrdered = __instance.playerOrdered;
                                                            DeliveryPackage.GRID[] array = grids;
                                                            int num12 = demandIndex;
                                                            array[num12].ordered = array[num12].ordered + num11;
                                                            __instance.workCourierCount++;
                                                            __instance.idleCourierCount--;
                                                            __instance.energy -= num4;
                                                            __instance.pulseSignal = 2;
                                                            flag = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                } else if (ptr.demandId == __instance.id) {
                                    ptr.runtimeState = 1;
                                    if (__instance.holdupItemCount < 6) {
                                        int supplyIndex = ptr.supplyIndex;
                                        if (supplyIndex >= 100) {
                                            Assert.CannotBeReached();
                                        } else {
                                            Assert.True(-(ptr.supplyId + 1) == supplyIndex);
                                            int itemId2 = grids[supplyIndex].itemId;
                                            // 修改部分============
                                            if (!CheckItemId(__instance, factory, itemId2)) {
                                                //=================
                                                Assert.CannotBeReached();
                                            } else if (__instance.holdupItemCount <= 0 || (itemId2 != __instance.holdupPackage[0].itemId && itemId2 != __instance.holdupPackage[1].itemId && itemId2 != __instance.holdupPackage[2].itemId && itemId2 != __instance.holdupPackage[3].itemId && itemId2 != __instance.holdupPackage[4].itemId)) {
                                                ptr.runtimeState = 2;
                                                int packageItemCount = __instance.packageUtility.GetPackageItemCount(itemId2);
                                                int num13 = grids[supplyIndex].modifiedCount + packageItemCount;
                                                int num14 = grids[supplyIndex].count + packageItemCount;
                                                if (((num13 < num14) ? num13 : num14) > grids[supplyIndex].recycleCount) {
                                                    ptr.runtimeState = 3;

                                                    int storageOrdered = GetItemStorageOrdered(__instance, factory, itemId2);
                                                    int playerOrdered = GetItemPlayerOrdered(__instance, factory, itemId2);

                                                    int num15 = (storageOrdered < 0) ? 0 : storageOrdered;
                                                    int num16 = __instance.InsertIntoStoragePrecalc(itemId2, courierCarries + playerOrdered + num15, false) - playerOrdered - num15;
                                                    if (num16 > 0) {
                                                        ptr.runtimeState = 4;
                                                        __instance.workCourierDatas[__instance.workCourierCount].begin = pos;
                                                        __instance.workCourierDatas[__instance.workCourierCount].end = pos;
                                                        __instance.workCourierDatas[__instance.workCourierCount].endId = ptr.supplyId;
                                                        __instance.workCourierDatas[__instance.workCourierCount].direction = 1f;
                                                        __instance.workCourierDatas[__instance.workCourierCount].maxt = 1f;
                                                        __instance.workCourierDatas[__instance.workCourierCount].t = 0f;
                                                        __instance.workCourierDatas[__instance.workCourierCount].itemId = itemId2;
                                                        __instance.workCourierDatas[__instance.workCourierCount].itemCount = 0;
                                                        __instance.workCourierDatas[__instance.workCourierCount].inc = 0;
                                                        // 修改部分============
                                                        __instance.workCourierDatas[__instance.workCourierCount].gene = currentGene;
                                                        //=====================
                                                        __instance.orders[__instance.workCourierCount].itemId = itemId2;
                                                        __instance.orders[__instance.workCourierCount].otherId = ptr.supplyId;
                                                        __instance.orders[__instance.workCourierCount].thisOrdered = num16;
                                                        __instance.orders[__instance.workCourierCount].otherOrdered = -num16;
                                                        //=============================
                                                        AddPlayerOrdered(__instance, factory, itemId2, num16);
                                                        //__instance.playerOrdered += num16;

                                                        DeliveryPackage.GRID[] array2 = grids;
                                                        int num17 = supplyIndex;
                                                        array2[num17].ordered = array2[num17].ordered - num16;
                                                        __instance.workCourierCount++;
                                                        __instance.idleCourierCount--;
                                                        __instance.energy -= num4;
                                                        __instance.pulseSignal = 2;
                                                        flag = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (!flag) {
                        if (__instance.pairProcess < __instance.playerPairCount) {
                            __instance.pairProcess = __instance.playerPairCount;
                        } else if (__instance.pairProcess >= __instance.pairCount) {
                            __instance.pairProcess = __instance.playerPairCount;
                        }
                        if (__instance.storageMode != EStorageDeliveryMode.None && CheckHasFilter(__instance, factory) && __instance.pairCount > __instance.playerPairCount) {
                            int num18 = __instance.pairProcess;
                            int num19 = 10;
                            SupplyDemandPair ptr2;
                            DispenserComponent dispenserComponent;
                            Vector3 pos2;
                            double num20;
                            long num21;
                            int inc2;
                            int num26;
                            int num28;
                            DispenserComponent dispenserComponent2;
                            Vector3 pos3;
                            double num30;
                            long num31;
                            int thisPairFilter;
                            for (; ; )
                            {
                                ptr2 = __instance.pairs[__instance.pairProcess];
                                __instance.pairProcess++;
                                if (__instance.pairProcess == __instance.pairCount) {
                                    __instance.pairProcess = __instance.playerPairCount;
                                }
                                num19--;
                                if (ptr2.supplyId == __instance.id) {
                                    if (ptr2.demandId <= 0) {
                                        Assert.Positive(ptr2.demandId);
                                    } else {
                                        dispenserComponent = dispenserPool[ptr2.demandId];
                                        // 修改部分============
                                        thisPairFilter = ptr2.supplyIndex == 0 ? __instance.filter : DispenserMutiFilterManager.Instance.GetMutiFilterdata(factory.planet.id)[__instance.id][ptr2.supplyIndex - 1];
                                        //=====================
                                        //thisPairFilter替换下面所有this.filter
                                        if (dispenserComponent == null || dispenserComponent.id != ptr2.demandId) {
                                            Assert.CannotBeReached();
                                        } else if (dispenserComponent.holdupItemCount < 6 && (dispenserComponent.holdupItemCount <= 0 || (dispenserComponent.holdupPackage[0].itemId != thisPairFilter && dispenserComponent.holdupPackage[1].itemId != thisPairFilter && dispenserComponent.holdupPackage[2].itemId != thisPairFilter && dispenserComponent.holdupPackage[3].itemId != thisPairFilter && dispenserComponent.holdupPackage[4].itemId != thisPairFilter))) {
                                            pos2 = entityPool[dispenserComponent.entityId].pos;
                                            if (__instance.CheckDeliveryRange(pos, pos2, deliveryRange, out num20)) {
                                                num21 = (long)(num20 * 10000.0 * 2.0 + 100000.0);
                                                if (__instance.energy >= num21) {

                                                    int dispenserComponentStorageOrdered = ptr2.demandIndex == 0 ? dispenserComponent.storageOrdered : DispenserMutiFilterManager.Instance.GetStorageOrderdata(factory.planet.id)[dispenserComponent.id][ptr2.demandIndex - 1];
                                                    int playerOrdered = ptr2.demandIndex == 0 ? dispenserComponent.playerOrdered : DispenserMutiFilterManager.Instance.GetPlayerOrderdata(factory.planet.id)[dispenserComponent.id][ptr2.demandIndex - 1];
                                                    int storageOrdered = ptr2.supplyIndex == 0 ? __instance.storageOrdered : DispenserMutiFilterManager.Instance.GetStorageOrderdata(factory.planet.id)[__instance.id][ptr2.supplyIndex - 1];

                                                    int num22 = (dispenserComponentStorageOrdered < 0) ? 0 : dispenserComponentStorageOrdered;
                                                    int num23 = dispenserComponent.InsertIntoStoragePrecalc(thisPairFilter, courierCarries + num22 + playerOrdered, true);
                                                    num23 = num23 - num22 - playerOrdered;
                                                    if (num23 > 0) {
                                                        int num24 = (storageOrdered > 0) ? 0 : storageOrdered;
                                                        int num25 = __instance.PickFromStoragePrecalc(thisPairFilter, num23 - num24);
                                                        num25 += num24;
                                                        if (num25 > 0) {
                                                            num26 = factory.PickFromStorage(num, thisPairFilter, num25, out inc2);
                                                            if (num26 > 0) {
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                } else if (ptr2.demandId == __instance.id) {
                                    // 修改部分============
                                    thisPairFilter = ptr2.demandIndex == 0 ? __instance.filter : DispenserMutiFilterManager.Instance.GetMutiFilterdata(factory.planet.id)[__instance.id][ptr2.demandIndex - 1];
                                    //=====================
                                    //thisPairFilter替换下面所有this.filter
                                    if (ptr2.supplyId <= 0) {
                                        Assert.Positive(ptr2.supplyId);
                                    } else if (__instance.holdupItemCount < 6 && (__instance.holdupItemCount <= 0 || (__instance.holdupPackage[0].itemId != thisPairFilter && __instance.holdupPackage[1].itemId != thisPairFilter && __instance.holdupPackage[2].itemId != thisPairFilter && __instance.holdupPackage[3].itemId != thisPairFilter && __instance.holdupPackage[4].itemId != thisPairFilter))) {

                                        int storageOrdered = ptr2.demandIndex == 0 ? __instance.storageOrdered : DispenserMutiFilterManager.Instance.GetStorageOrderdata(factory.planet.id)[__instance.id][ptr2.demandIndex - 1];
                                        int playerOrdered = ptr2.demandIndex == 0 ? __instance.playerOrdered : DispenserMutiFilterManager.Instance.GetPlayerOrderdata(factory.planet.id)[__instance.id][ptr2.demandIndex - 1];

                                        int num27 = (storageOrdered < 0) ? 0 : storageOrdered;
                                        num28 = __instance.InsertIntoStoragePrecalc(thisPairFilter, courierCarries + num27 + playerOrdered, true);
                                        num28 = num28 - num27 - playerOrdered;
                                        if (num28 > 0) {
                                            dispenserComponent2 = dispenserPool[ptr2.supplyId];
                                            if (dispenserComponent2 == null || dispenserComponent2.id != ptr2.supplyId) {
                                                Assert.CannotBeReached();
                                            } else {
                                                int dispenserComponent2StorageOrdered = ptr2.supplyIndex == 0 ? dispenserComponent2.storageOrdered : DispenserMutiFilterManager.Instance.GetStorageOrderdata(factory.planet.id)[dispenserComponent2.id][ptr2.supplyIndex - 1];
                                                int num29 = (dispenserComponent2StorageOrdered > 0) ? 0 : dispenserComponent2StorageOrdered;
                                                if (dispenserComponent2.PickFromStoragePrecalc(thisPairFilter, num28 - num29) + num29 > 0) {
                                                    pos3 = entityPool[dispenserComponent2.entityId].pos;
                                                    if (__instance.CheckDeliveryRange(pos, pos3, deliveryRange, out num30)) {
                                                        num31 = (long)(num30 * 10000.0 * 2.0 + 100000.0);
                                                        if (__instance.energy >= num31) {
                                                            goto Block_73;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (num18 == __instance.pairProcess || num19 <= 0) {
                                    goto IL_1049;
                                }
                            }
                            __instance.workCourierDatas[__instance.workCourierCount].begin = pos;
                            __instance.workCourierDatas[__instance.workCourierCount].end = pos2;
                            __instance.workCourierDatas[__instance.workCourierCount].endId = ptr2.demandId;
                            __instance.workCourierDatas[__instance.workCourierCount].direction = 1f;
                            __instance.workCourierDatas[__instance.workCourierCount].maxt = (float)num20;
                            __instance.workCourierDatas[__instance.workCourierCount].t = 0f;
                            __instance.workCourierDatas[__instance.workCourierCount].itemId = thisPairFilter;
                            __instance.workCourierDatas[__instance.workCourierCount].itemCount = num26;
                            __instance.workCourierDatas[__instance.workCourierCount].inc = inc2;
                            __instance.workCourierDatas[__instance.workCourierCount].gene = currentGene;
                            __instance.orders[__instance.workCourierCount].itemId = thisPairFilter;
                            __instance.orders[__instance.workCourierCount].otherId = ptr2.demandId;
                            __instance.orders[__instance.workCourierCount].thisOrdered = 0;
                            __instance.orders[__instance.workCourierCount].otherOrdered = num26;
                            __instance.storageOrdered = __instance.storageOrdered;
                            //=================
                            AddStorageOrdered(dispenserComponent, factory, thisPairFilter, num26);
                            //dispenserComponent.storageOrdered += num26;

                            factory.gameData.statistics.traffic.RegisterPlanetInternalStat(factory.planetId, thisPairFilter, num26);
                            __instance.workCourierCount++;
                            __instance.idleCourierCount--;
                            __instance.energy -= num21;
                            __instance.pulseSignal = 2;
                            goto IL_1049;
                        Block_73:
                            __instance.workCourierDatas[__instance.workCourierCount].begin = pos;
                            __instance.workCourierDatas[__instance.workCourierCount].end = pos3;
                            __instance.workCourierDatas[__instance.workCourierCount].endId = ptr2.supplyId;
                            __instance.workCourierDatas[__instance.workCourierCount].direction = 1f;
                            __instance.workCourierDatas[__instance.workCourierCount].maxt = (float)num30;
                            __instance.workCourierDatas[__instance.workCourierCount].t = 0f;
                            __instance.workCourierDatas[__instance.workCourierCount].itemId = thisPairFilter;
                            __instance.workCourierDatas[__instance.workCourierCount].itemCount = 0;
                            __instance.workCourierDatas[__instance.workCourierCount].inc = 0;
                            __instance.workCourierDatas[__instance.workCourierCount].gene = currentGene;
                            __instance.orders[__instance.workCourierCount].itemId = thisPairFilter;
                            __instance.orders[__instance.workCourierCount].otherId = ptr2.supplyId;
                            __instance.orders[__instance.workCourierCount].thisOrdered = num28;
                            __instance.orders[__instance.workCourierCount].otherOrdered = -num28;
                            //==============
                            AddStorageOrdered(__instance, factory, thisPairFilter, num28);
                            AddStorageOrdered(dispenserComponent2, factory, thisPairFilter, -num28);
                            //__instance.storageOrdered += num28;
                            //dispenserComponent2.storageOrdered -= num28;

                            __instance.workCourierCount++;
                            __instance.idleCourierCount--;
                            __instance.energy -= num31;
                            __instance.pulseSignal = 2;
                        }
                    }
                }
            }
        IL_1049:
            float num32 = 0.016666668f * courierSpeed;
            for (int j = 0; j < __instance.workCourierCount; j++) {
                if (__instance.workCourierDatas[j].maxt > 0f) {
                    if (__instance.workCourierDatas[j].endId < 0 && __instance.workCourierDatas[j].direction > 0f) {
                        Vector3 pos4 = entityPool[__instance.entityId].pos;
                        ref Vector3 ptr3 = ref __instance.workCourierDatas[j].end;
                        ref Vector3 ptr4 = ref playerPos;
                        Vector3 vector = new Vector3(ptr4.x - ptr3.x, ptr4.y - ptr3.y, ptr4.z - ptr3.z);
                        Vector3 vector2 = new Vector3(ptr4.x - pos4.x, ptr4.y - pos4.y, ptr4.z - pos4.z);
                        float num33 = (float)Math.Sqrt((double)(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z));
                        float num34 = (float)Math.Sqrt((double)(vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z));
                        float num35 = (float)Math.Sqrt((double)(ptr3.x * ptr3.x + ptr3.y * ptr3.y + ptr3.z * ptr3.z));
                        float num36 = (float)Math.Sqrt((double)(ptr4.x * ptr4.x + ptr4.y * ptr4.y + ptr4.z * ptr4.z));
                        if (num33 < 1.4f) {
                            double num37 = Math.Sqrt((double)(pos4.x * pos4.x + pos4.y * pos4.y + pos4.z * pos4.z));
                            double num38 = Math.Sqrt((double)(ptr4.x * ptr4.x + ptr4.y * ptr4.y + ptr4.z * ptr4.z));
                            double num39 = (double)(pos4.x * ptr4.x + pos4.y * ptr4.y + pos4.z * ptr4.z) / (num37 * num38);
                            if (num39 < -1.0) {
                                num39 = -1.0;
                            } else if (num39 > 1.0) {
                                num39 = 1.0;
                            }
                            __instance.workCourierDatas[j].begin = pos4;
                            __instance.workCourierDatas[j].maxt = (float)(Math.Acos(num39) * ((num37 + num38) * 0.5));
                            __instance.workCourierDatas[j].maxt = (float)Math.Sqrt((double)(__instance.workCourierDatas[j].maxt * __instance.workCourierDatas[j].maxt) + (num37 - num38) * (num37 - num38));
                            __instance.workCourierDatas[j].t = __instance.workCourierDatas[j].maxt;
                        } else {
                            __instance.workCourierDatas[j].begin = ptr3;
                            float num40 = courierSpeed * 0.016666668f / num33;
                            if (num40 > 1f) {
                                num40 = 1f;
                            }
                            Vector3 vector3 = new Vector3(vector.x * num40, vector.y * num40, vector.z * num40);
                            float num41 = num33 / courierSpeed;
                            if (num41 < 0.03333333f) {
                                num41 = 0.03333333f;
                            }
                            float num42 = (num36 - num35) / num41 * 0.016666668f;
                            ptr3.x += vector3.x;
                            ptr3.y += vector3.y;
                            ptr3.z += vector3.z;
                            ptr3 = ptr3.normalized * (num35 + num42);
                            if (num34 > __instance.workCourierDatas[j].maxt) {
                                __instance.workCourierDatas[j].maxt = num34;
                            }
                            __instance.workCourierDatas[j].t = num33;
                            if (__instance.workCourierDatas[j].t >= __instance.workCourierDatas[j].maxt * 0.99f) {
                                __instance.workCourierDatas[j].t = __instance.workCourierDatas[j].maxt * 0.99f;
                            }
                        }
                    } else {
                        CourierData[] array3 = __instance.workCourierDatas;
                        int num43 = j;
                        array3[num43].t = array3[num43].t + num32 * __instance.workCourierDatas[j].direction;
                    }
                    if (__instance.workCourierDatas[j].t >= __instance.workCourierDatas[j].maxt) {
                        __instance.workCourierDatas[j].t = __instance.workCourierDatas[j].maxt;
                        int endId = __instance.workCourierDatas[j].endId;
                        if (__instance.workCourierDatas[j].itemCount > 0) {
                            int itemId3 = __instance.workCourierDatas[j].itemId;
                            int itemCount = __instance.workCourierDatas[j].itemCount;
                            if (endId < 0) {
                                int num44 = -(endId + 1);
                                DeliveryPackage.GRID[] array4 = grids;
                                int num45 = num44;
                                array4[num45].ordered = array4[num45].ordered - __instance.orders[j].otherOrdered;
                                __instance.orders[j].otherOrdered = 0;
                                int num46 = grids[num44].clampedRequireCount - (grids[num44].count + __instance.packageUtility.GetPackageItemCountIncludeHandItem(itemId3));
                                int num47 = grids[num44].stackSizeModified - grids[num44].count + __instance.packageUtility.GetPackageItemCapacity(itemId3);
                                if (num46 > 0 && num47 > 0) {
                                    num46 = ((num46 > num47) ? num47 : num46);
                                    int num48 = 0;
                                    int num49;
                                    if (num46 < itemCount) {
                                        int inc3 = DispenserPatch.split_inc(ref __instance.workCourierDatas[j].itemCount, ref __instance.workCourierDatas[j].inc, num46);
                                        num49 = __instance.packageUtility.AddItemToAllPackages(itemId3, num46, num44, inc3, out num48, 0);
                                        CourierData[] array5 = __instance.workCourierDatas;
                                        int num50 = j;
                                        array5[num50].inc = array5[num50].inc + num48;
                                    } else {
                                        num49 = __instance.packageUtility.AddItemToAllPackages(itemId3, itemCount, num44, __instance.workCourierDatas[j].inc, out num48, 0);
                                        __instance.workCourierDatas[j].inc = num48;
                                    }
                                    __instance.packageUtility.player.NotifyReplenishPreferred(itemId3, num49);
                                    __instance.workCourierDatas[j].itemCount = itemCount - num49;
                                    __instance.orders[j].thisOrdered = __instance.workCourierDatas[j].itemCount;

                                    AddPlayerOrdered(__instance, factory, __instance.workCourierDatas[j].itemId, __instance.workCourierDatas[j].itemCount);
                                    //__instance.playerOrdered += __instance.workCourierDatas[j].itemCount;
                                }
                            } else {
                                DispenserComponent dispenserComponent3 = dispenserPool[endId];
                                AddStorageOrdered(dispenserComponent3, factory, __instance.orders[j].itemId, -__instance.orders[j].otherOrdered);
                                //dispenserComponent3.storageOrdered -= __instance.orders[j].otherOrdered;
                                __instance.orders[j].otherOrdered = 0;
                                int num52;
                                int num51 = factory.InsertIntoStorage(dispenserComponent3.storage.bottomStorage.entityId, itemId3, itemCount, __instance.workCourierDatas[j].inc, out num52, true);
                                int num53 = itemCount - num51;
                                if (num53 > 0) {
                                    bool flag3 = true;
                                    DispenserStore[] array6 = dispenserComponent3.holdupPackage;
                                    for (int k = 0; k < dispenserComponent3.holdupItemCount; k++) {
                                        if (array6[k].itemId == itemId3) {
                                            DispenserStore[] array7 = array6;
                                            int num54 = k;
                                            array7[num54].count = array7[num54].count + num53;
                                            DispenserStore[] array8 = array6;
                                            int num55 = k;
                                            array8[num55].inc = array8[num55].inc + num52;
                                            flag3 = false;
                                            break;
                                        }
                                    }
                                    if (flag3) {
                                        int num56 = dispenserComponent3.holdupItemCount;
                                        Assert.True(array6.Length >= num56);
                                        array6[num56].itemId = itemId3;
                                        array6[num56].count = num53;
                                        array6[num56].inc = num52;
                                        dispenserComponent3.holdupItemCount++;
                                    }
                                }
                                factory.gameData.statistics.traffic.RegisterPlanetInternalStat(factory.planetId, __instance.workCourierDatas[j].itemId, __instance.workCourierDatas[j].itemCount);
                                __instance.workCourierDatas[j].itemCount = 0;
                                __instance.workCourierDatas[j].inc = 0;
                                dispenserComponent3.pulseSignal = 2;
                            }
                            __instance.workCourierDatas[j].direction = -1f;
                        } else {
                            int itemId4 = __instance.orders[j].itemId;
                            int num57 = __instance.orders[j].thisOrdered;
                            if (endId < 0) {
                                int num58 = -(endId + 1);
                                AddPlayerOrdered(__instance, factory, __instance.orders[j].itemId, -__instance.orders[j].thisOrdered);
                                //__instance.playerOrdered -= __instance.orders[j].thisOrdered;
                                __instance.orders[j].thisOrdered = 0;
                                DeliveryPackage.GRID[] array9 = grids;
                                int num59 = num58;
                                array9[num59].ordered = array9[num59].ordered - __instance.orders[j].otherOrdered;
                                __instance.orders[j].otherOrdered = 0;
                                int num60 = grids[num58].count + __instance.packageUtility.GetPackageItemCount(itemId4);
                                if (num60 > grids[num58].recycleCount && num60 > 0) {
                                    int num61 = num60 - grids[num58].recycleCount;
                                    if (num61 > 0) {
                                        if (num61 < num57) {
                                            num57 = num61;
                                        }
                                        int inc4;
                                        __instance.packageUtility.TakeItemFromAllPackages(num58, ref itemId4, ref num57, out inc4, false);
                                        __instance.workCourierDatas[j].itemId = itemId4;
                                        __instance.workCourierDatas[j].itemCount = num57;
                                        __instance.workCourierDatas[j].inc = inc4;
                                        AddPlayerOrdered(__instance, factory, itemId4, num57);
                                        //__instance.playerOrdered += num57;
                                        __instance.orders[j].thisOrdered = num57;
                                    }
                                }
                                if (__instance.workCourierDatas[j].itemCount == 0 && (__instance.playerMode == EPlayerDeliveryMode.Recycle || __instance.playerMode == EPlayerDeliveryMode.Both) && CheckHasFilter(__instance, factory) && __instance.holdupItemCount < 6) {
                                    for (int l = 0; l < __instance.playerPairCount; l++) {
                                        if (__instance.pairs[l].supplyId < 0) {
                                            num58 = __instance.pairs[l].supplyIndex;
                                            int itemId5 = grids[num58].itemId;
                                            // 修改部分============
                                            if (!CheckItemId(__instance, factory, itemId5)) {
                                                //=================
                                                Assert.CannotBeReached();
                                            } else if (__instance.holdupItemCount <= 0 || (itemId5 != __instance.holdupPackage[0].itemId && itemId5 != __instance.holdupPackage[1].itemId && itemId5 != __instance.holdupPackage[2].itemId && itemId5 != __instance.holdupPackage[3].itemId && itemId5 != __instance.holdupPackage[4].itemId)) {
                                                int packageItemCount2 = __instance.packageUtility.GetPackageItemCount(itemId5);
                                                int num62 = grids[num58].modifiedCount + packageItemCount2;
                                                num60 = grids[num58].count + packageItemCount2;
                                                int num63 = (num62 < num60) ? num62 : num60;
                                                if (num63 > grids[num58].recycleCount && num63 > 0) {
                                                    int playerOrdered = GetItemPlayerOrdered(__instance, factory, itemId5);
                                                    int num64 = __instance.InsertIntoStoragePrecalc(itemId5, courierCarries + playerOrdered, false) - playerOrdered;
                                                    if (num64 > 0) {
                                                        int inc5;
                                                        __instance.packageUtility.TakeItemFromAllPackages(num58, ref itemId5, ref num64, out inc5, false);
                                                        if (num64 > 0) {
                                                            __instance.workCourierDatas[j].itemId = itemId5;
                                                            __instance.workCourierDatas[j].itemCount = num64;
                                                            __instance.workCourierDatas[j].inc = inc5;
                                                            __instance.workCourierDatas[j].endId = __instance.pairs[l].supplyId;
                                                            __instance.orders[j].itemId = itemId4;
                                                            __instance.orders[j].otherId = __instance.pairs[l].supplyId;
                                                            __instance.orders[j].thisOrdered = num64;
                                                            AddPlayerOrdered(__instance, factory, itemId5, num64);
                                                            //__instance.playerOrdered += num64;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            } else {
                                DispenserComponent dispenserComponent4 = dispenserPool[endId];
                                AddStorageOrdered(__instance, factory, __instance.orders[j].itemId, -__instance.orders[j].thisOrdered);
                                //__instance.storageOrdered -= __instance.orders[j].thisOrdered;
                                __instance.orders[j].thisOrdered = 0;
                                AddStorageOrdered(dispenserComponent4, factory, __instance.orders[j].itemId, -__instance.orders[j].otherOrdered);
                                //dispenserComponent4.storageOrdered -= __instance.orders[j].otherOrdered;
                                __instance.orders[j].otherOrdered = 0;
                                int inc6;
                                int num65 = factory.PickFromStorage(dispenserComponent4.storage.bottomStorage.entityId, itemId4, num57, out inc6);
                                __instance.workCourierDatas[j].itemId = itemId4;
                                __instance.workCourierDatas[j].itemCount = num65;
                                __instance.workCourierDatas[j].inc = inc6;
                                factory.gameData.statistics.traffic.RegisterPlanetInternalStat(factory.planetId, itemId4, num65);
                                AddStorageOrdered(__instance, factory, itemId4, num65);
                                //__instance.storageOrdered += num65;
                                __instance.orders[j].thisOrdered = num65;
                                dispenserComponent4.pulseSignal = 2;
                            }
                            __instance.workCourierDatas[j].direction = -1f;
                        }
                    } else if (__instance.workCourierDatas[j].t <= 0f) {
                        int itemId6 = __instance.workCourierDatas[j].itemId;
                        int itemCount2 = __instance.workCourierDatas[j].itemCount;
                        if (itemId6 > 0 && itemCount2 > 0) {
                            factory.gameData.statistics.traffic.RegisterPlanetInternalStat(factory.planetId, itemId6, itemCount2);
                            bool useBan = __instance.orders[j].otherId >= 0;
                            int num67;
                            int num66 = factory.InsertIntoStorage(num, itemId6, itemCount2, __instance.workCourierDatas[j].inc, out num67, useBan);
                            int num68 = itemCount2 - num66;
                            if (num68 > 0) {
                                bool flag4 = true;
                                for (int m = 0; m < __instance.holdupItemCount; m++) {
                                    if (__instance.holdupPackage[m].itemId == itemId6) {
                                        DispenserStore[] array10 = __instance.holdupPackage;
                                        int num69 = m;
                                        array10[num69].count = array10[num69].count + num68;
                                        DispenserStore[] array11 = __instance.holdupPackage;
                                        int num70 = m;
                                        array11[num70].inc = array11[num70].inc + num67;
                                        flag4 = false;
                                        break;
                                    }
                                }
                                if (flag4) {
                                    __instance.holdupPackage[__instance.holdupItemCount].itemId = itemId6;
                                    __instance.holdupPackage[__instance.holdupItemCount].count = num68;
                                    __instance.holdupPackage[__instance.holdupItemCount].inc = num67;
                                    __instance.holdupItemCount++;
                                }
                            }
                        }
                        if (__instance.orders[j].otherId < 0) {
                            AddPlayerOrdered(__instance, factory, __instance.orders[j].itemId, -__instance.orders[j].thisOrdered);
                            //__instance.playerOrdered -= __instance.orders[j].thisOrdered;
                        } else if (__instance.orders[j].otherId > 0) {
                            AddStorageOrdered(__instance, factory, __instance.orders[j].itemId, -__instance.orders[j].thisOrdered);
                            //__instance.storageOrdered -= __instance.orders[j].thisOrdered;
                        }
                        __instance.orders[j].thisOrdered = 0;
                        Array.Copy(__instance.workCourierDatas, j + 1, __instance.workCourierDatas, j, __instance.workCourierDatas.Length - j - 1);
                        Array.Copy(__instance.orders, j + 1, __instance.orders, j, __instance.orders.Length - j - 1);
                        __instance.workCourierCount--;
                        __instance.idleCourierCount++;
                        Array.Clear(__instance.workCourierDatas, __instance.workCourierCount, __instance.workCourierDatas.Length - __instance.workCourierCount);
                        Array.Clear(__instance.orders, __instance.workCourierCount, __instance.orders.Length - __instance.workCourierCount);
                        j--;
                        __instance.pulseSignal = 2;
                    }
                }
            }
            for (int n = 0; n < __instance.holdupItemCount; n++) {
                int count = __instance.holdupPackage[n].count;
                int inc7 = __instance.holdupPackage[n].inc;
                int inc8;
                int num71 = factory.InsertIntoStorage(__instance.storage.bottomStorage.entityId, __instance.holdupPackage[n].itemId, count, inc7, out inc8, true);
                __instance.holdupPackage[n].count = count - num71;
                __instance.holdupPackage[n].inc = inc8;
                if (__instance.holdupPackage[n].count == 0) {
                    Assert.Zero(__instance.holdupPackage[n].inc);
                    __instance.RemoveHoldupItem(n);
                    n--;
                }
            }
            if (__instance.filter > 0) {
                num2 = (int)(time % 600L);
                if (num2 < 0) {
                    num2 += 600;
                }
                if (num2 == __instance.gene) {
                    if (__instance.storage.bottomStorage.grids[0].itemId == __instance.filter) {
                        __instance.pickStorageSearchStart = __instance.storage.bottomStorage;
                        __instance.pickGridSearchStart = 0;
                    }
                    if (__instance.storage.topStorage.grids[__instance.storage.size - 1].itemId == 0) {
                        __instance.insertStorageSearchStart = __instance.storage;
                        __instance.insertGridSearchStart = __instance.storage.size - 1;
                        if (__instance.insertGridSearchStart < 0) {
                            __instance.insertGridSearchStart = 0;
                        }
                    }
                }
            }
            __instance.pulseSignal--;
            return false;
        }

        public static int split_inc(ref int n, ref int m, int p)
        {
            int num = m / n;
            int num2 = m - num * n;
            n -= p;
            num2 -= n;
            num = ((num2 > 0) ? (num * p + num2) : (num * p));
            m -= num;
            return num;
        }



        [HarmonyPatch(typeof(DispenserComponent), nameof(DispenserComponent.OnRematchPairs))]
        [HarmonyPrefix]
        public static bool DispenserComponent_OnRematchPairs_Patch(DispenserComponent __instance, PlanetFactory factory, DispenserComponent[] dispenserPool, int keyId, int courierCarries)
        {
            if (__instance.pairProcess < __instance.playerPairCount) {
                __instance.pairProcess = __instance.playerPairCount;
            } else if (__instance.pairProcess > __instance.pairCount - 1) {
                __instance.pairProcess = __instance.pairCount - 1;
            }
            DeliveryPackage.GRID[] grids = __instance.deliveryPackage.grids;
            for (int i = 0; i < __instance.workCourierCount; i++) {
                if (keyId == __instance.id) {
                    if (__instance.workCourierDatas[i].itemCount == 0 && __instance.workCourierDatas[i].direction > 0f) {
                        if (__instance.orders[i].otherId > 0) {
                            DispenserComponent dispenserComponent = dispenserPool[__instance.orders[i].otherId];
                            if (__instance.storageMode != EStorageDeliveryMode.Demand || dispenserComponent.storageMode != EStorageDeliveryMode.Supply) {
                                int itemid = __instance.orders[i].itemId;
                                if (!CheckItemId(__instance, factory, itemid) || !CheckItemId(dispenserComponent, factory, itemid)) {
                                    AddStorageOrdered(__instance, factory, itemid, -__instance.orders[i].thisOrdered);
                                    //__instance.storageOrdered -= __instance.orders[i].thisOrdered;
                                    __instance.orders[i].thisOrdered = 0;
                                    AddStorageOrdered(dispenserComponent, factory, itemid, -__instance.orders[i].otherOrdered);
                                    //dispenserPool[__instance.orders[i].otherId].storageOrdered -= __instance.orders[i].otherOrdered;
                                    __instance.orders[i].otherOrdered = 0;
                                    __instance.workCourierDatas[i].endId = 0;
                                    __instance.workCourierDatas[i].direction = -1f;
                                }
                            }
                        } else if (__instance.orders[i].otherId < 0) {
                            if (!CheckItemId(__instance, factory, __instance.orders[i].itemId)) {
                                AddPlayerOrdered(__instance, factory, __instance.orders[i].itemId, -__instance.orders[i].thisOrdered);
                               // __instance.playerOrdered -= __instance.orders[i].thisOrdered;
                                __instance.orders[i].thisOrdered = 0;
                                DeliveryPackage.GRID[] grids2 = __instance.deliveryPackage.grids;
                                int num = -(__instance.orders[i].otherId + 1);
                                grids2[num].ordered = grids2[num].ordered - __instance.orders[i].otherOrdered;
                                __instance.orders[i].otherOrdered = 0;
                                bool flag = true;
                                if ((__instance.playerMode == EPlayerDeliveryMode.Recycle || __instance.playerMode == EPlayerDeliveryMode.Both) && CheckHasFilter(__instance, factory) && __instance.holdupItemCount < 6) {
                                    for (int j = 0; j < __instance.playerPairCount; j++) {
                                        if (__instance.pairs[j].supplyId < 0) {
                                            int supplyIndex = __instance.pairs[j].supplyIndex;
                                            int itemId = grids[supplyIndex].itemId;
                                            if (!CheckItemId(__instance, factory, itemId)) {
                                                Assert.CannotBeReached();
                                            } else if (__instance.holdupItemCount <= 0 || (itemId != __instance.holdupPackage[0].itemId && itemId != __instance.holdupPackage[1].itemId && itemId != __instance.holdupPackage[2].itemId && itemId != __instance.holdupPackage[3].itemId && itemId != __instance.holdupPackage[4].itemId)) {
                                                int packageItemCount = __instance.packageUtility.GetPackageItemCount(itemId);
                                                int num2 = grids[supplyIndex].modifiedCount + packageItemCount;
                                                int num3 = grids[supplyIndex].count + packageItemCount;
                                                int num4 = (num2 < num3) ? num2 : num3;
                                                if (num4 > grids[supplyIndex].recycleCount && num4 > 0) {

                                                    int playerOrdered = GetItemPlayerOrdered(__instance, factory, itemId);

                                                    int num5 = __instance.InsertIntoStoragePrecalc(itemId, courierCarries + playerOrdered, false) - playerOrdered;
                                                    if (num5 > 0) {
                                                        __instance.workCourierDatas[i].itemId = itemId;
                                                        __instance.workCourierDatas[i].direction = 1f;
                                                        __instance.workCourierDatas[i].endId = __instance.pairs[j].supplyId;
                                                        __instance.orders[i].itemId = itemId;
                                                        __instance.orders[i].otherId = -(supplyIndex + 1);
                                                        __instance.orders[i].thisOrdered = num5;
                                                        __instance.orders[i].otherOrdered = -num5;
                                                        AddPlayerOrdered(__instance, factory, itemId, num5);
                                                        //__instance.playerOrdered += num5;
                                                        DeliveryPackage.GRID[] array = grids;
                                                        int num6 = supplyIndex;
                                                        array[num6].ordered = array[num6].ordered - num5;
                                                        flag = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (flag) {
                                    CourierTurnbackFromPlayer(factory, ref __instance.workCourierDatas[i], __instance.entityId);
                                }
                            }
                        }
                    }
                    if (__instance.workCourierDatas[i].itemCount != 0 && __instance.workCourierDatas[i].direction < 0f && !CheckItemId(__instance, factory, __instance.orders[i].itemId) && __instance.workCourierDatas[i].itemId > 0) {
                        if (__instance.orders[i].otherId > 0) {

                            AddStorageOrdered(__instance, factory, __instance.orders[i].itemId, -__instance.orders[i].thisOrdered);
                            //__instance.storageOrdered -= __instance.orders[i].thisOrdered;
                            __instance.orders[i].thisOrdered = 0;
                            AddStorageOrdered(dispenserPool[__instance.orders[i].otherId], factory, __instance.orders[i].itemId, -__instance.orders[i].otherOrdered);
                            //dispenserPool[__instance.orders[i].otherId].storageOrdered -= __instance.orders[i].otherOrdered;
                            __instance.orders[i].otherOrdered = 0;
                        } else if (__instance.orders[i].otherId < 0) {
                            AddPlayerOrdered(__instance, factory, __instance.orders[i].itemId, -__instance.orders[i].thisOrdered);
                            //__instance.playerOrdered -= __instance.orders[i].thisOrdered;
                            __instance.orders[i].thisOrdered = 0;
                            DeliveryPackage.GRID[] grids3 = __instance.deliveryPackage.grids;
                            int num7 = -(__instance.orders[i].otherId + 1);
                            grids3[num7].ordered = grids3[num7].ordered - __instance.orders[i].otherOrdered;
                            __instance.orders[i].otherOrdered = 0;
                        }
                    }
                } else if ((keyId == __instance.orders[i].otherId || (keyId < -100 && __instance.orders[i].otherId < 0)) && __instance.workCourierDatas[i].direction > 0f) {
                    if (__instance.orders[i].otherId > 0 && (dispenserPool[__instance.orders[i].otherId] == null || dispenserPool[__instance.orders[i].otherId].id == 0)) {
                        AddStorageOrdered(__instance, factory, __instance.orders[i].itemId, -__instance.orders[i].thisOrdered);
                        //__instance.storageOrdered -= __instance.orders[i].thisOrdered;
                        __instance.orders[i].thisOrdered = 0;
                        __instance.orders[i].otherOrdered = 0;
                        __instance.workCourierDatas[i].endId = 0;
                        __instance.workCourierDatas[i].direction = -1f;
                    } else if (__instance.workCourierDatas[i].itemCount > 0) {
                        if (__instance.orders[i].otherId > 0) {
                            DispenserComponent dispenserComponent2 = dispenserPool[__instance.orders[i].otherId];
                            if (__instance.storageMode != EStorageDeliveryMode.Supply || dispenserComponent2.storageMode != EStorageDeliveryMode.Demand) {
                                int itemid = __instance.orders[i].itemId;
                                if (!CheckItemId(__instance, factory, itemid) || !CheckItemId(dispenserComponent2, factory, itemid)) {
                                    AddStorageOrdered(__instance, factory, __instance.orders[i].itemId, -__instance.orders[i].thisOrdered);
                                    //__instance.storageOrdered -= __instance.orders[i].thisOrdered;
                                    __instance.orders[i].thisOrdered = 0;
                                    AddStorageOrdered(dispenserComponent2, factory, __instance.orders[i].itemId, -__instance.orders[i].otherOrdered);
                                    //dispenserComponent2.storageOrdered -= __instance.orders[i].otherOrdered;
                                    __instance.orders[i].otherOrdered = 0;
                                    __instance.workCourierDatas[i].endId = 0;
                                    __instance.workCourierDatas[i].direction = -1f;
                                }
                            }
                        } else {
                            AddPlayerOrdered(__instance, factory, __instance.orders[i].itemId, -__instance.orders[i].thisOrdered);
                            //__instance.playerOrdered -= __instance.orders[i].thisOrdered;
                            __instance.orders[i].thisOrdered = 0;
                            if (__instance.deliveryPackage.grids[-(__instance.orders[i].otherId + 1)].itemId == __instance.orders[i].itemId) {
                                DeliveryPackage.GRID[] grids4 = __instance.deliveryPackage.grids;
                                int num8 = -(__instance.orders[i].otherId + 1);
                                grids4[num8].ordered = grids4[num8].ordered - __instance.orders[i].otherOrdered;
                            }
                            __instance.orders[i].otherOrdered = 0;
                            CourierTurnbackFromPlayer(factory, ref __instance.workCourierDatas[i], __instance.entityId);
                        }
                    } else if (__instance.workCourierDatas[i].itemCount == 0) {
                        if (__instance.orders[i].otherId > 0) {
                            DispenserComponent dispenserComponent3 = dispenserPool[__instance.orders[i].otherId];
                            if (__instance.storageMode != EStorageDeliveryMode.Demand || dispenserComponent3.storageMode != EStorageDeliveryMode.Supply) {
                                int itemid = __instance.orders[i].itemId;
                                if (!CheckItemId(__instance, factory, itemid) || !CheckItemId(dispenserComponent3, factory, itemid)) {
                                    AddStorageOrdered(__instance, factory, __instance.orders[i].itemId, -__instance.orders[i].thisOrdered);
                                    //__instance.storageOrdered -= __instance.orders[i].thisOrdered;
                                    __instance.orders[i].thisOrdered = 0;
                                    AddStorageOrdered(dispenserComponent3, factory, __instance.orders[i].itemId, -__instance.orders[i].otherOrdered);
                                    //dispenserComponent3.storageOrdered -= __instance.orders[i].otherOrdered;
                                    __instance.orders[i].otherOrdered = 0;
                                    __instance.workCourierDatas[i].endId = 0;
                                    __instance.workCourierDatas[i].direction = -1f;
                                }
                            }
                        } else {
                            AddPlayerOrdered(__instance, factory, __instance.orders[i].itemId, -__instance.orders[i].thisOrdered);
                            //__instance.playerOrdered -= __instance.orders[i].thisOrdered;
                            __instance.orders[i].thisOrdered = 0;
                            if (__instance.deliveryPackage.grids[-(__instance.orders[i].otherId + 1)].itemId == __instance.orders[i].itemId) {
                                DeliveryPackage.GRID[] grids5 = __instance.deliveryPackage.grids;
                                int num9 = -(__instance.orders[i].otherId + 1);
                                grids5[num9].ordered = grids5[num9].ordered - __instance.orders[i].otherOrdered;
                            }
                            __instance.orders[i].otherOrdered = 0;
                            bool flag2 = true;
                            if ((__instance.playerMode == EPlayerDeliveryMode.Recycle || __instance.playerMode == EPlayerDeliveryMode.Both) && CheckHasFilter(__instance, factory) && __instance.holdupItemCount < 6) {
                                for (int k = 0; k < __instance.playerPairCount; k++) {
                                    if (__instance.pairs[k].supplyId < 0) {
                                        int supplyIndex2 = __instance.pairs[k].supplyIndex;
                                        int itemId2 = grids[supplyIndex2].itemId;
                                        if (!CheckItemId(__instance, factory, itemId2)) {
                                            Assert.CannotBeReached();
                                        } else if (__instance.holdupItemCount <= 0 || (itemId2 != __instance.holdupPackage[0].itemId && itemId2 != __instance.holdupPackage[1].itemId && itemId2 != __instance.holdupPackage[2].itemId && itemId2 != __instance.holdupPackage[3].itemId && itemId2 != __instance.holdupPackage[4].itemId)) {
                                            int packageItemCount2 = __instance.packageUtility.GetPackageItemCount(itemId2);
                                            int num10 = grids[supplyIndex2].modifiedCount + packageItemCount2;
                                            int num11 = grids[supplyIndex2].count + packageItemCount2;
                                            int num12 = (num10 < num11) ? num10 : num11;
                                            if (num12 > grids[supplyIndex2].recycleCount && num12 > 0) {
                                                
                                                int playerOrdered = GetItemPlayerOrdered(__instance, factory, itemId2);

                                                int num13 = __instance.InsertIntoStoragePrecalc(itemId2, courierCarries + playerOrdered, false) - playerOrdered;
                                                if (num13 > 0) {
                                                    __instance.workCourierDatas[i].itemId = itemId2;
                                                    __instance.workCourierDatas[i].direction = 1f;
                                                    __instance.workCourierDatas[i].endId = __instance.pairs[k].supplyId;
                                                    __instance.orders[i].itemId = itemId2;
                                                    __instance.orders[i].otherId = -(supplyIndex2 + 1);
                                                    __instance.orders[i].thisOrdered = num13;
                                                    __instance.orders[i].otherOrdered = -num13;
                                                    AddPlayerOrdered(__instance, factory, itemId2, num13);
                                                    //__instance.playerOrdered += num13;
                                                    DeliveryPackage.GRID[] array2 = grids;
                                                    int num14 = supplyIndex2;
                                                    array2[num14].ordered = array2[num14].ordered - num13;
                                                    flag2 = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (flag2) {
                                CourierTurnbackFromPlayer(factory, ref __instance.workCourierDatas[i], __instance.entityId);
                            }
                        }
                    }
                }
            }
            return false;
        }
        private static void CourierTurnbackFromPlayer(PlanetFactory factory, ref CourierData workCourier, int entityId)
        {
            ref Vector3 pos = ref factory.entityPool[entityId].pos;
            ref Vector3 end = ref workCourier.end;
            double num = Math.Sqrt(pos.x * pos.x + pos.y * pos.y + pos.z * pos.z);
            double num2 = Math.Sqrt(end.x * end.x + end.y * end.y + end.z * end.z);
            double num3 = (double)(pos.x * end.x + pos.y * end.y + pos.z * end.z) / (num * num2);
            if (num3 < -1.0) {
                num3 = -1.0;
            } else if (num3 > 1.0) {
                num3 = 1.0;
            }

            workCourier.begin = pos;
            workCourier.maxt = (float)(Math.Acos(num3) * ((num + num2) * 0.5));
            workCourier.maxt = (float)Math.Sqrt((double)(workCourier.maxt * workCourier.maxt) + (num - num2) * (num - num2));
            workCourier.t = workCourier.maxt;
            workCourier.endId = 0;
            workCourier.direction = -1f;
        }
    }
}
