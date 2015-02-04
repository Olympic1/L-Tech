using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LTScience
{
	//[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class SkyLabExperimentGUI : MonoBehaviour
	{
        protected Rect windowPos = new Rect(20, 20, 100, 150);
        protected int windowID = -5234628;

        public SkyLabConfig configcore;
        public SkyLabExperiment ActiveLab;

        public bool Active = false;

        public void drawGUI()
        {
            if (Active)
            {
                if (ValidLab())
                {
                    windowPos = GUILayout.Window(windowID, windowPos, Window, "SkyLab Experiments");
                }
                else
                {
                    Active = false;
                }
            }
        }

        private bool ValidLab()
        {
            if (ActiveLab == null)
            {
                return false;
            }

            return true;
        }

        private void Window(int windowID)
        {
            GUILayout.BeginVertical();

            foreach (SkyLabExperimentData node in configcore.experiments)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));

                if (GUILayout.Button("Study " + node.DisplayName, GUILayout.Height(50)))
                {
                    ActiveLab.DoScienceThing(node);
                    Active = false;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));

            if (GUILayout.Button("Close", GUILayout.Height(50)))
            {
                Active = false;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}