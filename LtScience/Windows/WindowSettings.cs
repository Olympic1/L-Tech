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
using KSP.Localization;
using LtScience.Utilities;
using UnityEngine;

using static LtScience.Addon;

namespace LtScience.Windows
{
    internal static class WindowSettings
    {
        internal static string title = "L-Tech Settings";
        internal static Rect position = new Rect(20, 60, 0, 0);
        internal static bool showWindow;

        // GUI tooltip and label support
        private static string _label = string.Empty;
        private static string _tooltip = string.Empty;
        private static GUIContent _guiLabel;

        private const int height = 25;
        private const int width = 250;

        internal static void Display(int windowId)
        {
            try
            {
                title = Localizer.Format("#autoLOC_LTech_Settings_001");

                Rect rect = new Rect(position.width - 20, 4, 18, 18);
                _label = "x";
                _tooltip = Localizer.Format("#autoLOC_LTech_Settings_tt_001");
                _guiLabel = new GUIContent(_label, _tooltip);
                if (GUI.Button(rect, _guiLabel))
                {
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight)
                        Addon.OnToolbarButtonToggle();
                    else
                        showWindow = false;
                }

                GUILayout.BeginVertical();

                DisplaySettings();

                DisplayActions();

                GUILayout.EndVertical();

                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
                Addon.RepositionWindow(ref position);
            }
            catch (Exception ex)
            {
                if (!Addon.FrameErrTripped)
                {
                    Log.Error($"WindowSettings.Display. Error: {ex.Message}\r\n\r\n{ex.StackTrace}");
                    Addon.FrameErrTripped = true;
                }
            }
        }

        private static void DisplaySettings()
        {
            try
            {
                //_label = Localizer.Format("#autoLOC_LTech_Settings_002", Settings.curVersion);
                //GUILayout.Label(_label, Style.LabelHeader, GUILayout.Width(width));
                //GUILayout.Label("__________________________________", Style.LabelStyleHardRule, GUILayout.Width(width), GUILayout.Height(10));

                GUI.enabled = true;

                GUILayout.Space(10);
                GUILayout.Label(Localizer.Format("#autoLOC_LTech_Settings_004"), Style.LabelStyleBold);

                // Hide UI on screenshot
                _label = Localizer.Format("#autoLOC_LTech_Settings_005");
                _tooltip = Localizer.Format("#autoLOC_LTech_Settings_tt_005");
                _guiLabel = new GUIContent(_label, _tooltip);
                Settings.hideUiOnScreenshot = GUILayout.Toggle(Settings.hideUiOnScreenshot, _guiLabel, GUILayout.Width(width));

                GUILayout.Space(10);
                GUILayout.Label(Localizer.Format("#autoLOC_LTech_Settings_006"), Style.LabelStyleBold);

                // Camera resolution
                _label = Localizer.Format("#autoLOC_LTech_Settings_007", (100 * Settings.resolution).ToString("0"), (Settings.resolution * Screen.width).ToString("0"), (Settings.resolution * Screen.height).ToString("0"));
                _tooltip = Localizer.Format("#autoLOC_LTech_Settings_tt_007a");
                _guiLabel = new GUIContent(_label, _tooltip);
                GUILayout.Label(_guiLabel, GUILayout.Width(width));

                // Resolution slider control
                GUILayout.BeginHorizontal();
                _label = (100 * Settings.minResolution).ToString("0");
                _tooltip = Localizer.Format("#autoLOC_LTech_Settings_tt_007b");
                _guiLabel = new GUIContent(_label, _tooltip);
                GUILayout.Label(_guiLabel, GUILayout.Width(20), GUILayout.Height(20));
                Settings.resolution = GUILayout.HorizontalSlider(Settings.resolution, Settings.minResolution, Settings.maxResolution, GUILayout.Width(width - 45), GUILayout.Height(20));
                _label = (100 * Settings.maxResolution).ToString("0");
                _tooltip = Localizer.Format("#autoLOC_LTech_Settings_tt_007b");
                _guiLabel = new GUIContent(_label, _tooltip);
                GUILayout.Label(_guiLabel, GUILayout.Width(25), GUILayout.Height(20));
                GUILayout.EndHorizontal();

                // Camera shuttertime
                _label = Localizer.Format("#autoLOC_LTech_Settings_008", Settings.shuttertime.ToString("0.#"));
                _tooltip = Localizer.Format("#autoLOC_LTech_Settings_tt_008a");
                _guiLabel = new GUIContent(_label, _tooltip);
                GUILayout.Label(_guiLabel, GUILayout.Width(width));

                // Shuttertime slider control
                GUILayout.BeginHorizontal();
                _label = Settings.minShuttertime.ToString("0.#");
                _tooltip = Localizer.Format("#autoLOC_LTech_Settings_tt_008b");
                _guiLabel = new GUIContent(_label, _tooltip);
                GUILayout.Label(_guiLabel, GUILayout.Width(20), GUILayout.Height(20));
                Settings.shuttertime = GUILayout.HorizontalSlider(Settings.shuttertime, Settings.minShuttertime, Settings.maxShuttertime, GUILayout.Width(width - 45), GUILayout.Height(20));
                _label = Settings.maxShuttertime.ToString("0");
                _tooltip = Localizer.Format("#autoLOC_LTech_Settings_tt_008b");
                _guiLabel = new GUIContent(_label, _tooltip);
                GUILayout.Label(_guiLabel, GUILayout.Width(25), GUILayout.Height(20));
                GUILayout.EndHorizontal();

                // Use alternate skin
                _label = Localizer.Format("Use alternate skin");
                _tooltip = Localizer.Format("Toggles the use of an alternate skin, which takes up less space");
                _guiLabel = new GUIContent(_label, _tooltip);

            }
            catch (Exception ex)
            {
                if (!Addon.FrameErrTripped)
                {
                    Log.Error($"WindowSettings.DisplaySettings. Error: {ex.Message}\r\n\r\n{ex.StackTrace}");
                    Addon.FrameErrTripped = true;
                }
            }
        }

        private static void DisplayActions()
        {
            try
            {
                GUILayout.BeginHorizontal();

                // Save button
                _label = Localizer.Format("#autoLOC_LTech_Settings_009");
                _tooltip = Localizer.Format("#autoLOC_LTech_Settings_tt_009");
                _guiLabel = new GUIContent(_label, _tooltip);
                if (GUILayout.Button(_guiLabel, GUILayout.Height(height)))
                {
                    Settings.SaveSettings();

                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight)
                        Addon.OnToolbarButtonToggle();
                    else
                        showWindow = false;
                }

                // Cancel button
                _label = Localizer.Format("#autoLOC_LTech_Settings_010");
                _tooltip = Localizer.Format("#autoLOC_LTech_Settings_tt_010");
                _guiLabel = new GUIContent(_label, _tooltip);
                if (GUILayout.Button(_guiLabel, GUILayout.Height(height)))
                {
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight)
                        Addon.OnToolbarButtonToggle();
                    else
                        showWindow = false;
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                // Reset button
                _label = Localizer.Format("#autoLOC_LTech_Settings_011");
                _tooltip = Localizer.Format("#autoLOC_LTech_Settings_tt_011");
                _guiLabel = new GUIContent(_label, _tooltip);
                if (GUILayout.Button(_guiLabel, GUILayout.Height(height)))
                    Settings.ResetSettings();

                GUILayout.EndHorizontal();
            }
            catch (Exception ex)
            {
                if (!Addon.FrameErrTripped)
                {
                    Log.Error($"WindowSettings.DisplayActions. Error: {ex.Message}\r\n\r\n{ex.StackTrace}");
                    Addon.FrameErrTripped = true;
                }
            }
        }
    }
}
