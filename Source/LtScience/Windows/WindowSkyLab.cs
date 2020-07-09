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
using LtScience.Modules;
using LtScience.Utilities;
using UnityEngine;

namespace LtScience.Windows
{
    internal static class WindowSkylab
    {
        internal static string title = "L-Tech Skylab";
        internal static Rect position = new Rect(20, 60, 0, 0);
        internal static bool showWindow;

        // GUI tooltip and label support
        private static string _label = string.Empty;
        private static string _tooltip = string.Empty;
        private static GUIContent _guiLabel;

        private const int height = 25;

        private static SkylabExperiment _labExp;

        internal static void Display(int windowId)
        {
            try
            {
                title = Localizer.Format("#autoLOC_LTech_Skylab_001");

                Rect rect = new Rect(position.width - 20, 4, 16, 16);
                _label = "x";
                _tooltip = Localizer.Format("#autoLOC_LTech_Skylab_tt_001");
                _guiLabel = new GUIContent(_label, _tooltip);
                if (GUI.Button(rect, _guiLabel))
                    showWindow = false;

                GUILayout.BeginVertical();

                DisplayExperiments();

                DisplayActions();

                GUILayout.EndVertical();

                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
                Addon.RepositionWindow(ref position);
            }
            catch (Exception ex)
            {
                if (!Addon.FrameErrTripped)
                {
                    Utils.LogMessage($"WindowSkylab.Display. Error: {ex.Message}\r\n\r\n{ex.StackTrace}", Utils.LogType.Error);
                    Addon.FrameErrTripped = true;
                }
            }
        }

        private static void DisplayExperiments()
        {
            try
            {
                // Load experiments
                _labExp = new SkylabExperiment();

                // Model Rockets
                _label = Localizer.Format("#autoLOC_LTech_Skylab_002");
                _tooltip = Localizer.Format("#autoLOC_LTech_Skylab_tt_002");
                _guiLabel = new GUIContent(_label, _tooltip);
                if (GUILayout.Button(_guiLabel, GUILayout.Height(height)))
                    _labExp.DoScience("modelRockets", 20, "ModelRockets", 1);

                // Micro Gravity
                _label = Localizer.Format("#autoLOC_LTech_Skylab_003");
                _tooltip = Localizer.Format("#autoLOC_LTech_Skylab_tt_003");
                _guiLabel = new GUIContent(_label, _tooltip);
                if (GUILayout.Button(_guiLabel, GUILayout.Height(height)))
                    _labExp.DoScience("microGrav", 10, "ClipBoards", 1);

                // Kerbal Habitation
                _label = Localizer.Format("#autoLOC_LTech_Skylab_004");
                _tooltip = Localizer.Format("#autoLOC_LTech_Skylab_tt_004");
                _guiLabel = new GUIContent(_label, _tooltip);
                if (GUILayout.Button(_guiLabel, GUILayout.Height(height)))
                    _labExp.DoScience("habCheck", 100, "ClipBoards", 1);

                // Fire Combustion
                _label = Localizer.Format("#autoLOC_LTech_Skylab_005");
                _tooltip = Localizer.Format("#autoLOC_LTech_Skylab_tt_005");
                _guiLabel = new GUIContent(_label, _tooltip);
                if (GUILayout.Button(_guiLabel, GUILayout.Height(height)))
                    _labExp.DoScience("fireCheck", 50, "ClipBoards", 1);
            }
            catch (Exception ex)
            {
                Utils.LogMessage($"WindowSkylab.DisplayExperiments. Error: {ex.Message}\r\n\r\n{ex.StackTrace}", Utils.LogType.Error);
            }
        }

        private static void DisplayActions()
        {
            try
            {
                GUILayout.BeginHorizontal();

                // Close button
                _label = Localizer.Format("#autoLOC_LTech_Skylab_006");
                _tooltip = Localizer.Format("#autoLOC_LTech_Skylab_tt_006");
                _guiLabel = new GUIContent(_label, _tooltip);
                if (GUILayout.Button(_guiLabel, GUILayout.Height(height)))
                    showWindow = false;

                GUILayout.EndHorizontal();
            }
            catch (Exception ex)
            {
                if (!Addon.FrameErrTripped)
                {
                    Utils.LogMessage($"WindowSkylab.DisplayActions. Error: {ex.Message}\r\n\r\n{ex.StackTrace}", Utils.LogType.Error);
                    Addon.FrameErrTripped = true;
                }
            }
        }
    }
}
