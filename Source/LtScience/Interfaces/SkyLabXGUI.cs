using UnityEngine;

namespace LTScience
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SkyLabExperimentGUI : MonoBehaviour
    {
        protected Rect windowPos = new Rect(20, 20, 100, 150);
        protected int windowID = 5234628;

        public SkyLabConfig configcore;
        public SkyLabExperiment activeLab;

        public bool uiShown = false;

        public void DrawUI()
        {
            if (uiShown)
            {
                if (ValidLab())
                    windowPos = GUILayout.Window(windowID, windowPos, Window, "SkyLab Experiments");
                else
                    uiShown = false;
            }
        }

        private bool ValidLab()
        {
            if (activeLab == null)
                return false;

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
                    activeLab.DoScienceThing(node);
                    uiShown = false;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));

            if (GUILayout.Button("Close", GUILayout.Height(50)))
                uiShown = false;

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
