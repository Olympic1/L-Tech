/*
 * L-Tech Scientific Industries Continued
 * Copyright © 2015-2018, Arne Peirs (Olympic1)
 * Copyright © 2016-2018, Jonathan Bayer (linuxgurugamer)
 * 
 * Kerbal Space Program is Copyright © 2011-2018 Squad. See https://kerbalspaceprogram.com/.
 * This project is in no way associated with nor endorsed by Squad.
 * 
 * This file is part of Olympic1's L-Tech (Continued). Original author of L-Tech is 'ludsoe' on the KSP Forums.
 * This file was not part of the original L-Tech but was written by Arne Peirs.
 * Copyright © 2015-2018, Arne Peirs (Olympic1)
 * 
 * Continues to be licensed under the MIT License.
 * See <https://opensource.org/licenses/MIT> for full details.
 */

using System;
using System.Collections;
using KSP.UI.Screens;
using LtScience.Utilities;
using LtScience.Windows;
using UnityEngine;
using ToolbarControl_NS;
using ClickThroughFix;
using KSP_Log;
using SpaceTuxUtility;

using System.Collections.Generic;
using System.Linq;

namespace LtScience
{
    [KSPAddon(KSPAddon.Startup.FlightAndKSC, false)]
    internal class Addon : MonoBehaviour
    {
        #region Properties

        // Toolbar integration
        //private static IButton _blizzyButton;
        //private static ApplicationLauncherButton _stockButton;

        static ToolbarControl toolbarControl;

        // Toolbar icons
        private const string _blizzyOff = "LTech/PluginData/Buttons/LT_blizzy_off";
        private const string _blizzyOn =  "LTech/PluginData/Buttons/LT_blizzy_on";
        private const string _stockOff =  "LTech/PluginData/Buttons/LT_stock_off";
        private const string _stockOn =   "LTech/PluginData/Buttons/LT_stock_on";

        // Repeating error latch
        internal static bool FrameErrTripped;

        // Camera UI toggle
        internal static bool ShowUi = true;

        // Makes instance available via reflection
        public static Addon Instance;

        public static KSP_Log.Log Log;

        int settingsID, windowSkylabID;
        #endregion

        #region Constructor

        public Addon()
        {
            Instance = this;
        }

        #endregion

        #region Event Handlers

        private void DummyVoid()
        {
            // This is used for the Stock toolbar. Some delegates are not needed.
        }

        internal void Awake()
        {
            try
            {
                //if (HighLogic.LoadedScene != GameScenes.SPACECENTER && !HighLogic.LoadedSceneIsFlight)
                //    return;

                Settings.LoadSettings();

                // Handle toolbars
                CreateToolbar();

                settingsID = WindowHelper.NextWindowId("SkylabSettings");
                windowSkylabID = WindowHelper.NextWindowId("WinSkylab");
            }
            catch (Exception ex)
            {
                Log.Error($"Addon.Awake. Error: {ex}");
            }
        }

        internal void Start()
        {
            try
            {
                // Reset frame error latch if set
                //if (FrameErrTripped)
                FrameErrTripped = false;
                Log.Info("Addon.Start");
                // Instantiate event handlers
                GameEvents.onGameSceneSwitchRequested.Add(OnGameSceneSwitchRequested);

                // If we are not in flight, the rest does not get done!
                //if (!HighLogic.LoadedSceneIsFlight)
                //    return;

                GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
                GameEvents.onShowUI.Add(OnShowUi);
                GameEvents.onHideUI.Add(OnHideUi);
                GameEvents.onVesselCreate.Add(onVesselCreate);
                GameEvents.onVesselDestroy.Add(onVesselDestroy);
                InitExperiments();

                FindVesselsWithSkylab();

                StartCoroutine("SlowUpdate");
            }
            catch (Exception ex)
            {
                Log.Error($"Addon.Start. Error: {ex}");
            }
        }

        static public Dictionary<string, Modules.Experiment> experiments = new Dictionary<string, Modules.Experiment>();

        void InitExperiments()
        {
            Log.Info("Addon.InitExperiment, count: " + experiments.Count);
            if (experiments.Count == 0)
            {
                ConfigNode[] allExperiment = GameDatabase.Instance.GetConfigNodes("EXPERIMENT_DEFINITION");
                foreach (var n in allExperiment)
                {
                    if (n.HasNode("LTECH"))
                    {
                        string id = "";
                        uint biomeMask = 0, situationMask = 0;

                        n.TryGetValue("id", ref id);
                        n.TryGetValue("biomeMask", ref biomeMask);
                        n.TryGetValue("situationMask", ref situationMask);
                        Log.Info("EXPERIMENT_DEFINITION, id: " + id);

                        ConfigNode node = n.GetNode("LTECH");
                        var data = new LtScience.Modules.Experiment().Load(id, node);
                        data.biomeMask = biomeMask;
                        data.situationMask = situationMask;

                        experiments.Add(data.name, data);
                    }
                }
            }
        }
        void onVesselCreate(Vessel v)
        {
            FindVesselsWithSkylab();
        }
        void onVesselDestroy(Vessel v)
        {
            FindVesselsWithSkylab(v);
        }

        internal void OnDestroy()
        {
            try
            {
                if (Settings.loaded)
                    Settings.SaveSettings();

                GameEvents.onGameSceneSwitchRequested.Remove(OnGameSceneSwitchRequested);
                GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
                GameEvents.onVesselCreate.Remove(onVesselCreate);
                GameEvents.onVesselDestroy.Remove(onVesselDestroy);

                GameEvents.onHideUI.Remove(OnHideUi);
                GameEvents.onShowUI.Remove(OnShowUi);
                StopCoroutine("SlowUpdate");

                // Handle toolbars
                DestroyAppIcon();
            }
            catch (Exception ex)
            {
                Log.Error($"Addon.OnDestroy. Error: {ex}");
            }
        }

        internal const string MODID = "LTech";
        internal const string MODNAME = "L-Tech";

        private void CreateToolbar()
        {
            if (toolbarControl == null)
            {
                toolbarControl = this.gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(
                    OnToolbarButtonToggle, OnToolbarButtonToggle,
                    ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.FLIGHT,
                    MODID,
                    "L-TechButton",
                   _stockOn, _stockOff,
                   _blizzyOn, _blizzyOff,
                    MODNAME
                    );
            }
        }

        private void DestroyAppIcon()
        {
            if (toolbarControl != null)
            {
                toolbarControl.OnDestroy();
                Destroy(toolbarControl);
            }
        }

        internal void OnGUI()
        {
            if (ShowUi)
            {
                try
                {
                    if (!HighLogic.CurrentGame.Parameters.CustomParams<LTech_1>().useAltSkin)
                        GUI.skin = HighLogic.Skin;

                    Style.SetupGuiStyles();
                    Display();
                }
                catch (Exception ex)
                {
                    Log.Error($"Addon.OnGUI. Error: {ex}");
                }
            }
        }
        List<Vessel> vesselsWithSkylab = new List<Vessel>();
        void FindVesselsWithSkylab(Vessel vesselDestroyed = null)
        {
            Log.Info("Addon.FindVesselsWithSkylab");

            vesselsWithSkylab.Clear();

            for (int vesselIdx = 0; vesselIdx < FlightGlobals.Vessels.Count; vesselIdx++)
            {
                Vessel v = FlightGlobals.Vessels[vesselIdx];
                Log.Info("vesselIdx[" + vesselIdx + "]: " + v.vesselName);
                if (vesselDestroyed != null && v.persistentId == vesselDestroyed.persistentId)
                {
                    Log.Info("Ignoring destroyed vessel: " + vesselDestroyed.vesselName);
                    continue;
                }
                if (v.packed)
                {
                    for (int i2 = 0; i2 < v.protoVessel.protoPartSnapshots.Count; i2++)
                    {
                        ProtoPartSnapshot p = v.protoVessel.protoPartSnapshots[i2];
                        var pms = p.FindModule("SkylabExperiment");
                        if (pms != null)
                        {
                            vesselsWithSkylab.Add(v);
                            break;
                        }
                    }

                }
            }
        }
        IEnumerator SlowUpdate()
        {
            Log.Info("Addon.SlowUpdate started");
            while (true)
            {
                // Look at each vessel with one or more Skylab parts
                Log.Info("SlowUpdate, vesselsWithSkylab.Count(): " + vesselsWithSkylab.Count());

                foreach (var v in vesselsWithSkylab)
                {
                    if (v.packed) // Only packed vessels, which are on rails
                    {
                        // Look at all the parts in the vessel
                        foreach (ProtoPartSnapshot ppsSkylab in v.protoVessel.protoPartSnapshots)
                        {
                            // Find the part(s) which have the SkylabExperiment
                            ProtoPartModuleSnapshot pms = ppsSkylab.FindModule("SkylabExperiment");
                            if (pms != null)
                            {
                                // Check for active experiments
                                if (pms.moduleValues.HasNode(LtScience.Modules.ExpStatus.EXPERIMENT_STATUS))
                                {
                                    // Loop through all nodes which are active experiments
                                    var activeExpNodes = pms.moduleValues.GetNodes(LtScience.Modules.ExpStatus.EXPERIMENT_STATUS);
                                    for (var i = 0; i < activeExpNodes.Count(); i++)
                                    {
                                        ConfigNode dataNode = activeExpNodes[i];
                                        Modules.ExpStatus data = new LtScience.Modules.ExpStatus().Load(dataNode);
                                        if (data.active)
                                        {
                                            var activeExperiment = new Modules.ActiveExperiment(data.expId, v.mainBody.bodyName, ScienceUtil.GetExperimentSituation(v), data.biome);
                                            Log.Info("Addon.SlowUpdate, stored key: " + data.key + ", calculated key: " + activeExperiment.KeyUnpacked(data.expId));

                                            if (activeExperiment.KeyUnpacked(data.expId) == data.key)
                                            {
                                                double delta = Planetarium.GetUniversalTime() - data.lastTimeUpdated;
                                                Log.Info("Addon.SlowUpdate, Active experiment is in valid situations");

                                                double resourceRequest = delta / Planetarium.fetch.fixedDeltaTime;

                                                double amtNeeded = Math.Min(
                                                    experiments[activeExperiment.activeExpid].resourceUsageRate * resourceRequest,
                                                     experiments[activeExperiment.activeExpid].resourceAmtRequired - data.processedResource);


                                                double totalAmtNeeded = amtNeeded;

                                                // look at skylab part first, then rest of vessel

                                                foreach (var pprs in ppsSkylab.resources)
                                                {
                                                    if (pprs.resourceName == experiments[activeExperiment.activeExpid].neededResourceName)
                                                    {
                                                        if (pprs.amount > amtNeeded)
                                                        {
                                                            pprs.amount -= amtNeeded;
                                                            amtNeeded = 0;
                                                        }
                                                        else
                                                        {
                                                            amtNeeded -= pprs.amount;
                                                            pprs.amount = 0;
                                                        }
                                                        break;
                                                    }
                                                }

                                                // If there wasn't enough of the resource in the Skylab part, then look at the rest of the vessel
                                                if (amtNeeded > 0)
                                                {
                                                    foreach (ProtoPartSnapshot pps in v.protoVessel.protoPartSnapshots)
                                                    {
                                                        foreach (var pprs in pps.resources)
                                                        {
                                                            if (pprs.resourceName == experiments[activeExperiment.activeExpid].neededResourceName)
                                                            {
                                                                if (pprs.amount > amtNeeded)
                                                                {
                                                                    pprs.amount -= amtNeeded;
                                                                    amtNeeded = 0;
                                                                }
                                                                else
                                                                {
                                                                    amtNeeded -= pprs.amount;
                                                                    pprs.amount = 0;
                                                                }

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
                        }
                    }
                    // Being a nice citizen by yielding after each vessel is processed
                    yield return null;
                }
                yield return new WaitForSecondsRealtime(5.0f);
            }
        }

        // Save settings on scene changes
        private void OnGameSceneLoadRequested(GameScenes requestedScene)
        {
            Settings.SaveSettings();
        }

        private void OnGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> sceneData)
        {
            WindowSettings.showWindow = false;
            var windows = GetComponents<WindowSkylab>();
            foreach (var w in windows)
                Destroy(w);


            // Since the changes to Startup options, OnDestroy is not being called when a scene change occurs. Startup is being called when the proper scene is loaded.
            // Let's do some cleanup of the app icons here as well to be sure we only have the icons we want.
            DestroyAppIcon();
        }

        // Camera UI toggle handlers
        private void OnShowUi()
        {
            ShowUi = true;
        }

        private void OnHideUi()
        {
            ShowUi = false;
        }

        // Toolbar button click handler
        internal static void OnToolbarButtonToggle()
        {
            try
            {
                //if (HighLogic.LoadedScene != GameScenes.SPACECENTER && !HighLogic.LoadedSceneIsFlight)
                //    return;

                WindowSettings.showWindow = !WindowSettings.showWindow;
                toolbarControl.SetTexture(WindowSettings.showWindow ? _stockOn : _stockOff, 
                                          WindowSettings.showWindow ? _blizzyOn : _blizzyOff);
            }
            catch (Exception ex)
            {
                Log.Error($"Addon.OnToolbarButtonToggle. Error: {ex}");
            }
        }

        #endregion

        #region GUI Methods

        private void Display()
        {
            string step = string.Empty;
            try
            {
                step = "0 - Start";
                if ( /* (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight) && */
                    ShowUi && !Utils.IsPauseMenuOpen())
                {
                    step = "2 - Can show settings window - true";
                    if (WindowSettings.showWindow)
                    {
                        step = "3 - Show settings window";
                        WindowSettings.position = ClickThruBlocker.GUILayoutWindow(settingsID, WindowSettings.position, WindowSettings.Display, WindowSettings.title, GUILayout.MinHeight(20));
                    }
                    else
                    {
                        step = "3 - Hide settings window";
                        WindowSettings.showWindow = false;
                    }
                }
                else
                {
                    step = "2 - Can show settings window - false";
                }

#if false
                step = "1 - Show Interface";
                if (Utils.CanShowSkylab())
                {
                    step = "4 - Can show Skylab window - true";
                    if (WindowSkylab.showWindow)
                    {
                        step = "5 - Show Skylab window";
                        WindowSkylab.position = ClickThruBlocker.GUILayoutWindow(windowSkylabID, WindowSkylab.position, WindowSkylab.Display, WindowSkylab.title, GUILayout.MinHeight(20));
                    }
                    else
                    {
                        step = "5 - Hide Skylab window";
                        WindowSkylab.showWindow = false;
                    }
                }
                else
                {
                    step = "4 - Can show Skylab window - false";
                }
#endif
            }
            catch (Exception ex)
            {
                if (!FrameErrTripped)
                {
                    Log.Error($"Addon.Display at or near step: {step}. Error: {ex.Message}\r\n\r\n{ex.StackTrace}");
                    FrameErrTripped = true;
                }
            }
        }

        internal static void RepositionWindows()
        {
            RepositionWindow(ref WindowSettings.position);
            //RepositionWindow(ref WindowSkylab.position);
        }

        internal static void RepositionWindow(ref Rect position)
        {
            if (position.x < 0)
                position.x = 0;

            if (position.y < 0)
                position.y = 0;

            if (position.xMax > Screen.width)
                position.x = Screen.width - position.width;

            if (position.yMax > Screen.height)
                position.y = Screen.height - position.height;
        }

        #endregion

        #region Action Methods

        #endregion
    }
}
