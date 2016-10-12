/*
 * L-Tech Scientific Industries Continued
 * Copyright © 2015-2016, Arne Peirs (Olympic1)
 * Copyright © 2016, linuxgurugamer
 * 
 * Kerbal Space Program is Copyright © 2011-2016 Squad. See http://kerbalspaceprogram.com/.
 * This project is in no way associated with nor endorsed by Squad.
 * 
 * This file is part of Olympic1's L-Tech (Continued). Original author of L-Tech is 'ludsoe' on the KSP Forums.
 * This file was not part of the original L-Tech but was written by Arne Peirs.
 * Copyright © 2015-2016, Arne Peirs (Olympic1)
 * 
 * Continues to be licensed under the MIT License.
 * See <https://opensource.org/licenses/MIT> for full details.
 */

using LtScience.InternalObjects;
using LtScience.Modules;
using System;
using UnityEngine;

namespace LtScience.Windows
{
    public class WindowSkyLab : MonoBehaviour
    {
        private const string title = "SkyLab Experiments";
        internal static Rect position = new Rect(20, 60, 0, 0);
        internal static bool showWindow;

        public SkyLabConfig configcore;
        public SkyLabExperiment activeLab;

        public void DrawGui()
        {
            GUI.skin = HighLogic.Skin;

            string step = "";
            try
            {
                step = "0 - Start";
                if (Util.CanShowSkyLab())
                {
                    step = "2 - Can Show SkyLab - true";
                    if (showWindow)
                    {
                        if (ValidLab())
                        {
                            step = "3 - Show SkyLab";
                            position = GUILayout.Window(5234628, position, Display, title, LtAddon.WindowStyle, GUILayout.MinHeight(20));
                        }
                        else
                        {
                            step = "3 - Hide SkyLab";
                            showWindow = false;
                        }
                    }
                }
                else
                    step = "2 - Can Show SkyLab - false";
            }
            catch (Exception ex)
            {
                Util.LogMessage($"LTAddon.Display at or near step: {step}. Error: {ex.Message} \r\n\r\n{ex.StackTrace}", Util.LogType.Error);
            }
        }

        private bool ValidLab()
        {
            if (activeLab == null)
                return false;

            return true;
        }

        private void Display(int windowId)
        {
            try
            {
                Rect rect = new Rect(position.width - 29, 4, 25, 25);
                if (GUI.Button(rect, new GUIContent("X", "Close Window")))
                    showWindow = false;

                GUILayout.BeginVertical();
                GUI.enabled = true;

                GUILayout.Space(10);
                foreach (SkyLabExperimentData node in SkyLabConfig.Experiments)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Study " + node.Name, GUILayout.Height(30)))
                    {
                        activeLab.DoScienceThing(node);
                        showWindow = false;
                    }
                    GUILayout.Space(5);
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
                LtAddon.RepositionWindow(ref position);
            }
            catch (Exception ex)
            {
                Util.LogMessage($"WindowSkyLab.Display. Error: {ex.Message} \r\n\r\n{ex.StackTrace}", Util.LogType.Error);
            }
        }
    }
}
