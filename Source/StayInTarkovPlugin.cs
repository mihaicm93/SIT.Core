using Aki.Custom.Patches;
using BepInEx;
using BepInEx.Bootstrap;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using EFT.Communications;
using EFT.UI;
using Newtonsoft.Json;
using SIT.Core.AI.PMCLogic.Roaming;
using SIT.Core.AI.PMCLogic.RushSpawn;
using SIT.Core.AkiSupport.Airdrops;
using SIT.Core.AkiSupport.Custom;
using SIT.Core.AkiSupport.Singleplayer;
using SIT.Core.AkiSupport.SITFixes;
using SIT.Core.Configuration;
using SIT.Core.Coop;
using SIT.Core.Coop.AI;
using SIT.Core.Core;
using SIT.Core.Core.FileChecker;
using SIT.Core.Core.Web;
using SIT.Core.Misc;
using SIT.Core.Other;
using SIT.Core.SP.Menus;
using SIT.Core.SP.PlayerPatches;
using SIT.Core.SP.PlayerPatches.Health;
using SIT.Core.SP.Raid;
using SIT.Core.SP.ScavMode;
using SIT.Tarkov.Core;
using SIT.Tarkov.Core.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SIT.Core
{
    [BepInPlugin("com.sit.core", "SIT.Core", "1.9.0")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class StayInTarkovPlugin : BaseUnityPlugin
    {
        public static StayInTarkovPlugin Instance;
        public static PluginConfigSettings Settings { get; private set; }

        private bool ShownDependancyError { get; set; }
        public static string EFTVersionMajor { get; internal set; }
        public static string EFTAssemblyVersion { get; internal set; }
        public static string EFTEXEFileVersion { get; internal set; }

        public static Dictionary<string, string> LanguageDictionary { get; } = new Dictionary<string, string>();    

        private void Awake()
        {
            Instance = this;
            Settings = new PluginConfigSettings(Logger, Config);
            LogDependancyErrors();


            // Gather the Major/Minor numbers of EFT ASAP
            new VersionLabelPatch(Config).Enable();
            StartCoroutine(VersionChecks());

            ReadInLanguageDictionary();

            EnableCorePatches();
            EnableSPPatches();
            EnableCoopPatches();
            OtherPatches.Run(Config, this);

            Logger.LogInfo($"Stay in Tarkov is loaded!");
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void ReadInLanguageDictionary()
        {

            Logger.LogDebug(Thread.CurrentThread.CurrentCulture);

            var languageFiles = new List<string>();
            foreach (var mrs in typeof(StayInTarkovPlugin).Assembly.GetManifestResourceNames().Where(x => x.StartsWith("SIT.Core.Resources.Language")))
            {
                languageFiles.Add(mrs);
                Logger.LogDebug(mrs);
            }

            Logger.LogDebug(Thread.CurrentThread.CurrentCulture.Name);
            var firstPartOfLang = Thread.CurrentThread.CurrentCulture.Name.ToLower().Substring(0, 2);
            Logger.LogDebug(firstPartOfLang);
            Stream stream = null;
            StreamReader sr = null;
            string str = null;
            Dictionary<string, string> resultLocaleDictionary = null;
            switch (firstPartOfLang)
            {
                case "zh":
                    switch (Thread.CurrentThread.CurrentCulture.Name.ToLower())
                    {
                        case "zh_TW":
                            stream = typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(languageFiles.First(x => x.EndsWith("TraditionalChinese.json")));
                            break;
                        case "zh_CN":
                        default:
                            stream = typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(languageFiles.First(x => x.EndsWith("SimplifiedChinese.json")));
                            break;
                    }
                    break;
                case "ja":
                    stream = typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(languageFiles.First(x => x.EndsWith("Japanese.json")));
                    break;
                case "de":
                    stream = typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(languageFiles.First(x => x.EndsWith("German.json")));
                    break;
                case "en":
                default:
                    stream = typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(languageFiles.First(x => x.EndsWith("English.json")));
                    break;

            }

            if (stream == null)
                return;

            // Load Language Stream in
            using (sr = new StreamReader(stream))
            {
                str = sr.ReadToEnd();

                resultLocaleDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);

                if (resultLocaleDictionary == null)
                    return;

                foreach (var kvp in resultLocaleDictionary)
                {
                    LanguageDictionary.Add(kvp.Key, kvp.Value);
                }

               
            }

            // Load English Language Stream to Fill any missing expected statements in the Dictionary
            using (sr = new StreamReader(typeof(StayInTarkovPlugin).Assembly.GetManifestResourceStream(languageFiles.First(x => x.EndsWith("English.json")))))
            {
                foreach (var kvp in JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd()))
                {
                    if(!LanguageDictionary.ContainsKey(kvp.Key))
                        LanguageDictionary.Add(kvp.Key, kvp.Value);
                }
            }

            Logger.LogDebug("Loaded in the following Language Dictionary");
            Logger.LogDebug(LanguageDictionary.ToJson());
        }

        private IEnumerator VersionChecks()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);

                if (!string.IsNullOrEmpty(EFTVersionMajor))
                {
                    Logger.LogInfo("Version Check: Detected:" + EFTVersionMajor);
                    if (EFTVersionMajor.Split('.').Length > 4)
                    {
                        var majorN1 = EFTVersionMajor.Split('.')[0]; // 0
                        var majorN2 = EFTVersionMajor.Split('.')[1]; // 13
                        var majorN3 = EFTVersionMajor.Split('.')[2]; // 1
                        var majorN4 = EFTVersionMajor.Split('.')[3]; // 1
                        var majorN5 = EFTVersionMajor.Split('.')[4]; // build number

                        // 0.13.5.2.26282
                        // 0.13.9.0.26921
                        if (majorN1 != "0" || majorN2 != "13" || majorN3 != "9" || majorN4 != "1")
                        {
                            Logger.LogError("Version Check: This version of SIT is not designed to work with this version of EFT.");
                        }
                        else
                        {
                            Logger.LogInfo("Version Check: OK.");
                        }
                    }

                    yield break;
                }
            }
        }

        private void EnableCorePatches()
        {
            Logger.LogInfo($"{nameof(EnableCorePatches)}");
            try
            {
                // SIT Legal Game Checker
                var lcRemover = Config.Bind<bool>("Debug Settings", "LC Remover", false).Value;
                if (!lcRemover)
                {
                    LegalGameCheck.LegalityCheck();
                }

                // File Checker
                new ConsistencySinglePatch().Enable();
                new ConsistencyMultiPatch().Enable();
                new RunFilesCheckingPatch().Enable();
                // BattlEye
                new BattlEyePatch().Enable();
                new BattlEyePatchFirstPassRun().Enable();
                new BattlEyePatchFirstPassUpdate().Enable();
                // Web Requests
                new SslCertificatePatch().Enable();
                new UnityWebRequestPatch().Enable();
                new SendCommandsPatch().Enable();

                //https to http | wss to ws
                var url = BackendConnection.GetBackendConnection().BackendUrl;
                if (!url.Contains("https"))
                {
                    new TransportPrefixPatch().Enable();
                    new WebSocketPatch().Enable();
                }

                //new TarkovTransportHttpMethodDebugPatch2().Enable();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            Logger.LogDebug($"{nameof(EnableCorePatches)} Complete");
        }

        private void EnableSPPatches()
        {
            try
            {
                var enabled = Config.Bind<bool>("SIT.SP", "Enable", true);
                if (!enabled.Value) // if it is disabled. stop all SIT SP Patches.
                {
                    Logger.LogInfo("SIT SP Patches has been disabled! Ignoring Patches.");
                    return;
                }

                //// --------- PMC Dogtags -------------------
                new UpdateDogtagPatch().Enable();

                //// --------- Player Init & Health -------------------
                EnableSPPatches_PlayerHealth(Config);

                //// --------- SCAV MODE ---------------------
                new DisableScavModePatch().Enable();

                //// --------- Airdrop -----------------------
                //new AirdropPatch().Enable();

                //// --------- Screens ----------------
                EnableSPPatches_Screens(Config);

                //// --------- Progression -----------------------
                EnableSPPatches_PlayerProgression();

                ////--------- Trader ------------------
                new PostRaidHealingPricePatch().Enable();

                //// --------------------------------------
                // Bots
                EnableSPPatches_Bots(Config);

                new QTEPatch().Enable();
                new TinnitusFixPatch().Enable();

                try
                {
                    BundleManager.GetBundles();
                    new EasyAssetsPatch().Enable();
                    new EasyBundlePatch().Enable();
                }
                catch (Exception ex)
                {
                    Logger.LogError("// --- ERROR -----------------------------------------------");
                    Logger.LogError("Bundle System Failed!!");
                    Logger.LogError(ex.ToString());
                    Logger.LogError("// --- ERROR -----------------------------------------------");
                }

                new WavesSpawnScenarioInitPatch(Config).Enable();
                new WavesSpawnScenarioMethodPatch().Enable();
            }
            catch(Exception ex)
            {
                Logger.LogError($"{nameof(EnableSPPatches)} failed.");
                Logger.LogError(ex);
            }
        }

        private static void EnableSPPatches_Screens(BepInEx.Configuration.ConfigFile config)
        {
            //new OfflineRaidMenuPatch().Enable();
            new OfflineSettingsScreenPatch().Enable();
            new InsuranceScreenPatch().Enable();
            new MatchmakerLocationScreen_DisableReadyButton_Patch().Enable();

            //try
            //{
            //    new MatchmakerLocationScreen_DisableLevelLock_Patch().Enable();
            //}
            //catch(Exception ex) { Plugin.Instance.Logger.LogError(ex.Message); }

            new LighthouseBridgePatch().Enable();
            new LighthouseTransmitterPatch().Enable();
            new PostRaidHealScreenPatch().Enable();
        }

        private static void EnableSPPatches_PlayerProgression()
        {
            new OfflineSaveProfile().Enable();
            new ExperienceGainPatch().Enable();
        }

        private void EnableSPPatches_PlayerHealth(BepInEx.Configuration.ConfigFile config)
        {
            var enabled = config.Bind<bool>("SIT.SP", "EnableHealthPatches", true);
            if (!enabled.Value)
                return;

            new Player_Init_SP_Patch().Enable();
            new ChangeHealthPatch().Enable();
            new ChangeHydrationPatch().Enable();
            new ChangeEnergyPatch().Enable();
            new OnDeadPatch(Config).Enable();
            new MainMenuControllerForHealthListenerPatch().Enable();
        }

        private static void EnableSPPatches_Bots(BepInEx.Configuration.ConfigFile config)
        {
            new CoreDifficultyPatch().Enable();
            new BotDifficultyPatch().Enable();
            new GetNewBotTemplatesPatch().Enable();
            new BotSettingsRepoClassIsFollowerFixPatch().Enable();
            new BotSelfEnemyPatch().Enable();
            new PmcFirstAidPatch().Enable();
            new SpawnProcessNegativeValuePatch().Enable();
            //new CustomAiPatch().Enable();
            new LocationLootCacheBustingPatch().Enable();

            var enabled = config.Bind<bool>("SIT.SP", "EnableBotPatches", true);
            if (!enabled.Value)
                return;

            BrainManager.AddCustomLayer(typeof(RoamingLayer), new List<string>() { "Assault", "PMC" }, 2);
            //BrainManager.AddCustomLayer(typeof(PMCRushSpawnLayer), new List<string>() { "Assault", "PMC" }, 9999);


        }

        private void EnableCoopPatches()
        {
            CoopPatches.Run(Config);
        }

        public static GameWorld gameWorld { get; private set; }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            //GetPoolManager();
            //GetBackendConfigurationInstance();

            if (Singleton<GameWorld>.Instantiated)
                gameWorld = Singleton<GameWorld>.Instance;
        }

        //private void GetBackendConfigurationInstance()
        //{
        //    //if (
        //    //    PatchConstants.BackendStaticConfigurationType != null &&
        //    //    PatchConstants.BackendStaticConfigurationConfigInstance == null)
        //    //{
        //    //    PatchConstants.BackendStaticConfigurationConfigInstance = ReflectionHelpers.GetPropertyFromType(PatchConstants.BackendStaticConfigurationType, "Config").GetValue(null);
        //    //    //Logger.LogInfo($"BackendStaticConfigurationConfigInstance Type:{ PatchConstants.BackendStaticConfigurationConfigInstance.GetType().Name }");
        //    //}

        //    if (PatchConstants.BackendStaticConfigurationConfigInstance != null
        //        && PatchConstants.CharacterControllerSettings.CharacterControllerInstance == null
        //        )
        //    {
        //        PatchConstants.CharacterControllerSettings.CharacterControllerInstance
        //            = ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(PatchConstants.BackendStaticConfigurationConfigInstance, "CharacterController", false);
        //        //Logger.LogInfo($"PatchConstants.CharacterControllerInstance Type:{PatchConstants.CharacterControllerSettings.CharacterControllerInstance.GetType().Name}");
        //    }

        //    if (PatchConstants.CharacterControllerSettings.CharacterControllerInstance != null
        //        && PatchConstants.CharacterControllerSettings.ClientPlayerMode == null
        //        )
        //    {
        //        PatchConstants.CharacterControllerSettings.ClientPlayerMode
        //            = ReflectionHelpers.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "ClientPlayerMode", false);

        //        PatchConstants.CharacterControllerSettings.ObservedPlayerMode
        //            = ReflectionHelpers.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "ObservedPlayerMode", false);

        //        PatchConstants.CharacterControllerSettings.BotPlayerMode
        //            = ReflectionHelpers.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "BotPlayerMode", false);
        //    }

        //}


        private void LogDependancyErrors()
        {
            // Skip if we've already shown the message, or there are no errors
            if (ShownDependancyError || Chainloader.DependencyErrors.Count == 0)
            {
                return;
            }

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("Errors occurred during plugin loading");
            stringBuilder.AppendLine("-------------------------------------");
            stringBuilder.AppendLine();
            foreach (string error in Chainloader.DependencyErrors)
            {
                stringBuilder.AppendLine(error);
                stringBuilder.AppendLine();
            }
            string errorMessage = stringBuilder.ToString();

            DisplayMessageNotifications.DisplayMessageNotification($"{errorMessage}", ENotificationDurationType.Infinite, ENotificationIconType.Alert, UnityEngine.Color.red);

            // Show an error in the BepInEx console/log file
            Logger.LogError(errorMessage);

            // Show an error in the in-game console, we have to write this in reverse order because of the nature of the console output
            foreach (string line in errorMessage.Split('\n').Reverse())
            {
                if (line.Trim().Length > 0)
                {
                    ConsoleScreen.LogError(line);
                }
            }

            ShownDependancyError = true;


        }

       

    }
}
