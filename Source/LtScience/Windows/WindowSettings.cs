using LtScience.APIClients;
using LtScience.InternalObjects;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LtScience.Windows
{
    internal static class WindowSettings
    {
        internal const string title = "L-Tech Settings";
        internal static Rect position = new Rect(20, 60, 0, 0);
        internal static bool showWindow;

        // Tooltips
        internal static string toolTip = "";
        private static bool toolTipActive;
        private static bool showToolTips = true;
        private static bool _canShowToolTips = true;

        // GUI tooltip and label support
        private static string _label = "";
        private static string _toolTip = "";
        private static GUIContent _guiLabel;
        private static Rect _rect;

        internal static void Display(int windowId)
        {
            try
            {
                // Reset Tooltip active flag
                toolTipActive = false;

                Rect rect = new Rect(position.width - 29, 4, 25, 25);
                if (GUI.Button(rect, new GUIContent("X", "Close Window.\r\nSettings will not be saved.")))
                {
                    toolTip = "";
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                        LtAddon.OnToolbarButtonToggle();
                    else
                        showWindow = false;
                }
                if (Event.current.type == EventType.Repaint && showToolTips)
                    toolTip = LtToolTips.SetActiveToolTip(rect, GUI.tooltip, ref toolTipActive, 10);

                GUILayout.BeginVertical();

                DisplaySettings();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save", GUILayout.Height(30)))
                {
                    LtSettings.SaveSettings();
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight)
                        LtAddon.OnToolbarButtonToggle();
                    else
                        showWindow = false;
                }

                if (GUILayout.Button("Cancel", GUILayout.Height(30)))
                {
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight)
                        LtAddon.OnToolbarButtonToggle();
                    else
                        showWindow = false;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset to default", GUILayout.Height(30)))
                {
                    LtSettings.ResetSettings();
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight)
                        LtAddon.OnToolbarButtonToggle();
                    else
                        showWindow = false;
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
                LtAddon.RepositionWindow(ref position);
            }
            catch (Exception ex)
            {
                Util.LogMessage(string.Format("WindowSettings.Display. Error: {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Util.LogType.Error);
            }
        }

        private static void DisplaySettings()
        {
            try
            {
                // Reset Tooltip active flag
                toolTipActive = false;
                _canShowToolTips = showToolTips;

                const int scrollX = 20;

                GUILayout.Label("L-Tech v" + LtSettings.curVersion, LtAddon.LabelTabHeader);
                GUILayout.Label("\r\n___________________________________", LtAddon.LabelStyleHardRule, GUILayout.Width(280), GUILayout.Height(10));

                // Blizzy Toolbar
                if (!ToolbarManager.ToolbarAvailable)
                {
                    if (LtSettings.enableBlizzyToolbar)
                        LtSettings.enableBlizzyToolbar = false;
                    GUI.enabled = false;
                }
                else
                    GUI.enabled = true;

                _label = "Enable Blizzy Toolbar";
                _toolTip = "Switches the toolbar Icons over to Blizzy's toolbar, if installed.";
                _toolTip += "\r\nIf Blizzy's toolbar is not installed, option is not selectable.";
                _guiLabel = new GUIContent(_label, _toolTip);
                LtSettings.enableBlizzyToolbar = GUILayout.Toggle(LtSettings.enableBlizzyToolbar, _guiLabel, GUILayout.Width(280));
                _rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && _canShowToolTips)
                    toolTip = LtToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTipActive, scrollX);

                GUI.enabled = true;
                // Tooltips
                _label = "Enable Tool Tips";
                _toolTip = "Turns tooltips On or Off.";
                _toolTip += "\r\nThis is a global setting for all windows.";
                _guiLabel = new GUIContent(_label, _toolTip);
                LtSettings.showToolTips = GUILayout.Toggle(LtSettings.showToolTips, _guiLabel, GUILayout.Width(280));
                _rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && _canShowToolTips)
                    toolTip = LtToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTipActive, scrollX);

                GUI.enabled = true;
                GUILayout.Label("", GUILayout.Height(10));
                GUILayout.Label("Screenshot Settings:", GUILayout.Height(20));

                // Hide UI On Screenshot
                _label = "Hide UI On Screenshot";
                _toolTip = "Hides or shows the UI when taking a screenshot.";
                _toolTip += "\r\nThis setting only affects the UI when taking a RL picture.";
                _guiLabel = new GUIContent(_label, _toolTip);
                LtSettings.hideUiOnScreenshot = GUILayout.Toggle(LtSettings.hideUiOnScreenshot, _guiLabel, GUILayout.Width(280));
                _rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && _canShowToolTips)
                    toolTip = LtToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTipActive, scrollX);

                // Convert to JPG
                _label = "Convert To JPG";
                _toolTip = "Converts the screenshot to a jpg.";
                _toolTip += "\r\nWhen disabled, the screenshot will be saved as a png.";
                _guiLabel = new GUIContent(_label, _toolTip);
                LtSettings.convertToJpg = GUILayout.Toggle(LtSettings.convertToJpg, _guiLabel, GUILayout.Width(280));
                _rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && _canShowToolTips)
                    toolTip = LtToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTipActive, scrollX);

                if (!LtSettings.convertToJpg)
                    GUI.enabled = false;

                // Keep Original PNG
                _label = "Keep Original PNG";
                _toolTip = "Keeps the original png along with the converted jpg.";
                _toolTip += "\r\nWhen disabled, the png will be deleted after the conversion.";
                _toolTip += "\r\nIf Convert To JPG is disabled, this option is not selectable.";
                _guiLabel = new GUIContent(_label, _toolTip);
                LtSettings.keepOriginalPng = GUILayout.Toggle(LtSettings.keepOriginalPng, _guiLabel, GUILayout.Width(280));
                _rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && _canShowToolTips)
                    toolTip = LtToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTipActive, scrollX);

                // JPG Quality
                float newQuality;
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                _label = "JPG Quality:";
                _toolTip = "Sets the quality of the jpg when converting the screenshot.";
                _toolTip += "\r\nRange is from 1 - 100. Default is 75.";
                _toolTip += "\r\nIf Convert To JPG is disabled, this option is not selectable.";
                _guiLabel = new GUIContent(_label, _toolTip);
                GUILayout.Label(_guiLabel, GUILayout.Width(140), GUILayout.Height(20));
                _rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && _canShowToolTips)
                    toolTip = LtToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTipActive, scrollX);

                // Lets parse the string
                string strQuality = LtSettings.jpgQuality.ToString();

                strQuality = GUILayout.TextField(strQuality, 3, GUILayout.Width(80), GUILayout.Height(20));

                // Make sure we can only type numbers
                strQuality = Regex.Replace(strQuality, @"[^0-9]", "");
                GUILayout.EndHorizontal();

                // Keep the quality between 1 and 100
                if (float.TryParse(strQuality, out newQuality))
                {
                    if (newQuality < 1)
                        newQuality = 1;

                    if (newQuality > 100)
                        newQuality = 100;

                    LtSettings.jpgQuality = (int)newQuality;
                }

                GUI.enabled = true;
                GUILayout.Label("", GUILayout.Height(10));
                GUILayout.Label("Camera Settings:", GUILayout.Height(20));

                // Camera Resolution
                _label = "Camera Resolution:  ";
                _label += string.Format("{0:0}", 100 * LtSettings.resolution) + " %";
                _toolTip = "Sets the resolution of the screenshot. Default is current resolution";
                _toolTip += "\r\nThe resolution is measured in game window resolutions.";
                _guiLabel = new GUIContent(_label, _toolTip);
                GUILayout.Label(_guiLabel, GUILayout.Width(280), GUILayout.Height(20));
                _rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && _canShowToolTips)
                    toolTip = LtToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTipActive, scrollX);

                // Resolution Slider Control
                GUILayout.BeginHorizontal();
                GUILayout.Label(LtSettings.minResolution.ToString(CultureInfo.InvariantCulture), GUILayout.Width(20), GUILayout.Height(15));
                LtSettings.resolution = GUILayout.HorizontalSlider(LtSettings.resolution, LtSettings.minResolution, LtSettings.maxResolution, GUILayout.Width(230), GUILayout.Height(20));
                _label = LtSettings.maxResolution.ToString(CultureInfo.InvariantCulture);
                _toolTip = "Slide control to change the resolution shown above.";
                _guiLabel = new GUIContent(_label, _toolTip);
                GUILayout.Label(_guiLabel, GUILayout.Width(20), GUILayout.Height(15));
                _rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && _canShowToolTips)
                    toolTip = LtToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTipActive, scrollX);
                GUILayout.EndHorizontal();

                // Camera Shuttertime
                _label = "Camera Shuttertime:  ";
                _label += string.Format("{0:0.#}", LtSettings.shuttertime) + " sec";
                _toolTip = "Sets the amount the game 'freezes' in camera view when taking a screenshot.";
                _toolTip += "\r\nThe shuttertime is timed in real time seconds.";
                _guiLabel = new GUIContent(_label, _toolTip);
                GUILayout.Label(_guiLabel, GUILayout.Width(280), GUILayout.Height(20));
                _rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && _canShowToolTips)
                    toolTip = LtToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTipActive, scrollX);

                // Shuttertime Slider Control
                GUILayout.BeginHorizontal();
                GUILayout.Label(LtSettings.minShuttertime.ToString(CultureInfo.InvariantCulture), GUILayout.Width(20), GUILayout.Height(15));
                LtSettings.shuttertime = GUILayout.HorizontalSlider(LtSettings.shuttertime, LtSettings.minShuttertime, LtSettings.maxShuttertime, GUILayout.Width(230), GUILayout.Height(20));
                _label = LtSettings.maxShuttertime.ToString(CultureInfo.InvariantCulture);
                _toolTip = "Slide control to change the shuttertime shown above.";
                _guiLabel = new GUIContent(_label, _toolTip);
                GUILayout.Label(_guiLabel, GUILayout.Width(20), GUILayout.Height(15));
                _rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && _canShowToolTips)
                    toolTip = LtToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTipActive, scrollX);
                GUILayout.EndHorizontal();
            }
            catch (Exception ex)
            {
                Util.LogMessage(string.Format("WindowSettings.DisplaySettings. Error: {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Util.LogType.Error);
            }
        }
    }
}
