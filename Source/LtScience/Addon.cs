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
using KSP.UI.Screens;
using LtScience.Utilities;
using LtScience.Windows;
using UnityEngine;

namespace LtScience
{
    [KSPAddon(KSPAddon.Startup.FlightAndKSC, false)]
    internal class Addon : MonoBehaviour
    {
        #region Properties

        // Toolbar integration
        private static IButton _blizzyButton;
        private static ApplicationLauncherButton _stockButton;

        // Toolbar icons
        private const string _blizzyOff = "LTech/Plugins/LT_blizzy_off";
        private const string _blizzyOn = "LTech/Plugins/LT_blizzy_on";
        private const string _stockOff = "LTech/Plugins/LT_stock_off";
        private const string _stockOn = "LTech/Plugins/LT_stock_on";

        // Repeating error latch
        internal static bool FrameErrTripped;

        // Camera UI toggle
        internal static bool ShowUi = true;

        // Makes instance available via reflection
        public static Addon Instance;

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
                if (HighLogic.LoadedScene != GameScenes.SPACECENTER && !HighLogic.LoadedSceneIsFlight)
                    return;

                Settings.LoadSettings();

                // Handle toolbars
                CreateAppIcon();
            }
            catch (Exception ex)
            {
                Utils.LogMessage($"Addon.Awake. Error: {ex}", Utils.LogType.Error);
            }
        }

        internal void Start()
        {
            try
            {
                // Reset frame error latch if set
                if (FrameErrTripped)
                    FrameErrTripped = false;

                // Instantiate event handlers
                GameEvents.onGameSceneSwitchRequested.Add(OnGameSceneSwitchRequested);

                // If we are not in flight, the rest does not get done!
                if (!HighLogic.LoadedSceneIsFlight)
                    return;

                GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
                GameEvents.onShowUI.Add(OnShowUi);
                GameEvents.onHideUI.Add(OnHideUi);
            }
            catch (Exception ex)
            {
                Utils.LogMessage($"Addon.Start. Error: {ex}", Utils.LogType.Error);
            }
        }

        internal void OnDestroy()
        {
            try
            {
                if (Settings.loaded)
                    Settings.SaveSettings();

                GameEvents.onGameSceneSwitchRequested.Remove(OnGameSceneSwitchRequested);
                GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
                GameEvents.onHideUI.Remove(OnHideUi);
                GameEvents.onShowUI.Remove(OnShowUi);

                // Handle toolbars
                DestroyAppIcon();
            }
            catch (Exception ex)
            {
                Utils.LogMessage($"Addon.OnDestroy. Error: {ex}", Utils.LogType.Error);
            }
        }

        private void CreateAppIcon()
        {
            if (Settings.enableBlizzyToolbar)
            {
                // Let's try to use Blizzy's toolbar
                if (ActivateBlizzyToolbar())
                    return;

                // We failed to activate the toolbar, so revert to stock
                GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGuiAppLauncherDestroyed);
            }
            else
            {
                // Use stock toolbar
                GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGuiAppLauncherDestroyed);
            }
        }

        private void DestroyAppIcon()
        {
            if (_blizzyButton == null)
            {
                if (_stockButton != null)
                {
                    ApplicationLauncher.Instance.RemoveModApplication(_stockButton);
                    _stockButton = null;
                }

                if (_stockButton == null)
                    // Remove the stock toolbar button
                    GameEvents.onGUIApplicationLauncherReady.Remove(OnGuiAppLauncherReady);
            }
            else
            {
                _blizzyButton?.Destroy();
            }
        }

        internal void OnGUI()
        {
            try
            {
                GUI.skin = HighLogic.Skin;

                Style.SetupGuiStyles();
                Display();
            }
            catch (Exception ex)
            {
                Utils.LogMessage($"Addon.OnGUI. Error: {ex}", Utils.LogType.Error);
            }
        }

        internal void Update()
        {
            try
            {
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight)
                    CheckForToolbarTypeToggle();
            }
            catch (Exception ex)
            {
                if (!FrameErrTripped)
                {
                    Utils.LogMessage($"Addon.Update (repeating error). Error: {ex.Message}\r\n\r\n{ex.StackTrace}", Utils.LogType.Error);
                    FrameErrTripped = true;
                }
            }
        }

        // Save settings on scene changes
        private void OnGameSceneLoadRequested(GameScenes requestedScene)
        {
            Settings.SaveSettings();
        }

        private void OnGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> sceneData)
        {
            WindowSettings.showWindow = WindowSkylab.showWindow = false;

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

        // Stock vs Blizzy toolbar switch handler
        private void CheckForToolbarTypeToggle()
        {
            if (Settings.enableBlizzyToolbar && !Settings.prevEnableBlizzyToolbar)
            {
                // Let's try to use Blizzy's toolbar
                if (!ActivateBlizzyToolbar())
                {
                    // We failed to activate the toolbar, so revert to stock
                    GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
                    GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGuiAppLauncherDestroyed);
                    Settings.enableBlizzyToolbar = Settings.prevEnableBlizzyToolbar;
                }
                else
                {
                    // Use Blizzy toolbar
                    OnGuiAppLauncherDestroyed();
                    GameEvents.onGUIApplicationLauncherReady.Remove(OnGuiAppLauncherReady);
                    GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGuiAppLauncherDestroyed);
                    Settings.prevEnableBlizzyToolbar = Settings.enableBlizzyToolbar;

                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight)
                        _blizzyButton.Visible = true;
                }
            }
            else if (!Settings.enableBlizzyToolbar && Settings.prevEnableBlizzyToolbar)
            {
                // Use stock toolbar
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight)
                    _blizzyButton.Visible = false;

                GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGuiAppLauncherDestroyed);
                OnGuiAppLauncherReady();
                Settings.prevEnableBlizzyToolbar = Settings.enableBlizzyToolbar;
            }
        }

        // Stock toolbar startup and cleanup
        private void OnGuiAppLauncherReady()
        {
            try
            {
                // Setup settings button
                if ((HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight) && _stockButton == null && !Settings.enableBlizzyToolbar)
                {
                    _stockButton = ApplicationLauncher.Instance.AddModApplication(
                        OnToolbarButtonToggle,
                        OnToolbarButtonToggle,
                        DummyVoid, DummyVoid,
                        DummyVoid, DummyVoid,
                        ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.FLIGHT,
                        GameDatabase.Instance.GetTexture(_stockOff, false));

                    if (WindowSettings.showWindow)
                        _stockButton.SetTexture(GameDatabase.Instance.GetTexture(WindowSettings.showWindow ? _stockOn : _stockOff, false));
                }
            }
            catch (Exception ex)
            {
                Utils.LogMessage($"Addon.OnGuiAppLauncherReady. Error: {ex}", Utils.LogType.Error);
            }
        }

        private void OnGuiAppLauncherDestroyed()
        {
            try
            {
                if (_stockButton == null)
                    return;

                ApplicationLauncher.Instance.RemoveModApplication(_stockButton);
                _stockButton = null;
            }
            catch (Exception ex)
            {
                Utils.LogMessage($"Addon.OnGuiAppLauncherDestroyed. Error: {ex}", Utils.LogType.Error);
            }
        }

        // Toolbar button click handler
        internal static void OnToolbarButtonToggle()
        {
            try
            {
                if (HighLogic.LoadedScene != GameScenes.SPACECENTER && !HighLogic.LoadedSceneIsFlight)
                    return;

                WindowSettings.showWindow = !WindowSettings.showWindow;

                if (Settings.enableBlizzyToolbar)
                    _blizzyButton.TexturePath = WindowSettings.showWindow ? _blizzyOn : _blizzyOff;
                else
                    _stockButton.SetTexture(GameDatabase.Instance.GetTexture(WindowSettings.showWindow ? _stockOn : _stockOff, false));
            }
            catch (Exception ex)
            {
                Utils.LogMessage($"Addon.OnToolbarButtonToggle. Error: {ex}", Utils.LogType.Error);
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
                if ((HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight) && ShowUi && !Utils.IsPauseMenuOpen())
                {
                    step = "2 - Can show settings window - true";
                    if (WindowSettings.showWindow)
                    {
                        step = "3 - Show settings window";
                        WindowSettings.position = GUILayout.Window(5234629, WindowSettings.position, WindowSettings.Display, WindowSettings.title, GUILayout.MinHeight(20));
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

                step = "1 - Show Interface";
                if (Utils.CanShowSkylab())
                {
                    step = "4 - Can show Skylab window - true";
                    if (WindowSkylab.showWindow)
                    {
                        step = "5 - Show Skylab window";
                        WindowSkylab.position = GUILayout.Window(5234628, WindowSkylab.position, WindowSkylab.Display, WindowSkylab.title, GUILayout.MinHeight(20));
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
            }
            catch (Exception ex)
            {
                if (!FrameErrTripped)
                {
                    Utils.LogMessage($"Addon.Display at or near step: {step}. Error: {ex.Message}\r\n\r\n{ex.StackTrace}", Utils.LogType.Error);
                    FrameErrTripped = true;
                }
            }
        }

        internal static void RepositionWindows()
        {
            RepositionWindow(ref WindowSettings.position);
            RepositionWindow(ref WindowSkylab.position);
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

        private bool ActivateBlizzyToolbar()
        {
            if (!Settings.enableBlizzyToolbar)
                return false;

            if (!ToolbarManager.ToolbarAvailable)
                return false;

            try
            {
                if (HighLogic.LoadedScene != GameScenes.SPACECENTER && !HighLogic.LoadedSceneIsFlight)
                    return true;

                _blizzyButton = ToolbarManager.Instance.add("L-Tech", "Settings");
                _blizzyButton.TexturePath = WindowSettings.showWindow ? _blizzyOn : _blizzyOff;
                _blizzyButton.ToolTip = "L-Tech Settings Window";
                _blizzyButton.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER, GameScenes.FLIGHT);
                _blizzyButton.Visible = true;
                _blizzyButton.OnClick += e => { OnToolbarButtonToggle(); };

                return true;
            }
            catch (Exception)
            {
                // Blizzy toolbar instantiation error - Ignore
                return false;
            }
        }

        #endregion
    }
}
