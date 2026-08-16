using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems;
using CommonAPI.Systems.ModLocalization;
using xiaoye97;
using crecheng.DSPModSave;
using HarmonyLib.Tools;

namespace MultipleDelivery_MOD.src
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(DSPModSavePlugin.MODGUID)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [BepInDependency(LDBToolPlugin.MODGUID)]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(TabSystem), nameof(LocalizationModule))]
    [ModSaveSettings(LoadOrder = LoadOrder.Preload)]
    public class MultipleDelivery: BaseUnityPlugin, IModCanSave
    {
        public const string MODGUID = "org.ProfessorCat305.MultipleDelivery";
        public const string MODNAME = "MultipleDelivery";
        public const string VERSION = "1.0.0";
        public const string DEBUGVERSION = "";

        public static bool LoadCompleted;

        internal static ManualLogSource logger;

        internal static ConfigEntry<bool> LDBToolCacheEntry, HideTechModeEntry, ShowMessageBoxEntry;

        internal static ConfigEntry<int> ProductOverflowEntry;

        internal static ConfigEntry<KeyboardShortcut> QToolsHotkey;

        private Harmony Harmony;

        public void Awake()
        {
            #region Logger

            logger = Logger;
            logger.Log(LogLevel.Info, "MultipleDelivery Awake");

            #endregion Logger

            Harmony = new Harmony(MODGUID);

            var executingAssembly = Assembly.GetExecutingAssembly();

            foreach (Type type in executingAssembly.GetTypes()) {
                if (type.Namespace?.StartsWith("MultipleDelivery_MOD.src", StringComparison.Ordinal) == true) { Harmony.PatchAll(type); }
            }

            LDBTool.PreAddDataAction += PreAddDataAction;
            LDBTool.PostAddDataAction += PostAddDataAction;

            LoadCompleted = true;
        }

        public void Export(BinaryWriter w)
        {
            DispenserMutiFilterManager.Export(w);
        }
        public void Import(BinaryReader r)
        {
            DispenserMutiFilterManager.Import(r);
        }
        public void IntoOtherSave()
        {
            DispenserMutiFilterManager.IntoOtherSave();
        }

        public string Version => VERSION;

        private void PreAddDataAction()
        {
        }
        private void PostAddDataAction()
        {

        }

        internal static void LogInfo(object data) => logger.LogInfo(data);
        internal static void LogWarning(object data) => logger.LogWarning(data);
        internal static void LogError(object data) => logger.LogError(data);

        internal static int VersionNumber()
        {
            var version = new Version();
            version.FromFullString(VERSION);
            return version.sig;
        }
    }
}
