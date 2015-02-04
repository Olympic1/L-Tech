using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Toolbar;
using LTScience;

namespace LTScience
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class ConfigWindow : MonoBehaviour
    {
        protected Rect windowPos = new Rect(20, 20, 320, 220);
        protected int windowID = -5234628;

        private static bool activated = false;
        LTechCCore configcore;
        
        void Start()
        {
    	    // Lets find our config manager!
            configcore = FindObjectOfType<LTechCCore>();

            toolbarButton();

            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));
        }

        private void drawGUI()
        {
            if (activated)
            {
                windowPos = GUILayout.Window(windowID, windowPos, Window, "LTech Config");
            }
        }

        private void Window(int windowID)
	    {
            GUILayout.BeginVertical();

            // Camera Resolution
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Label("Resolution (Measured in game window resolutions)");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            configcore.camres = GUILayout.HorizontalSlider(configcore.camres, 1f, 5f, GUILayout.Width(240));
            GUILayout.Label(string.Format("{0:0}", 100 * configcore.camres) + " %");
            GUILayout.EndHorizontal();

            // Camera ShutterTime
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Label("ShutterTime (Seconds the game holds in camera view.)");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            configcore.strtime = GUILayout.HorizontalSlider(configcore.strtime, 0.2f, 5f, GUILayout.Width(240));
            GUILayout.Label(string.Format("{0:0}", configcore.strtime) + " sec");
            GUILayout.EndHorizontal();	
            
            // Divider
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Label("_____________________________________");
            GUILayout.EndHorizontal();
            
		    // Reset button
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));

	        if (GUILayout.Button("Reset to default"))
	        {
                configcore.ResetConfig();
	        }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));

	        if (GUILayout.Button("Apply", GUILayout.Height(50)))
	        {
                configcore.SaveConfig();
                activated = false;
	        }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
	    }
        
	    // ToolBar button part
        private IButton button1;
	    private void toolbarButton()
        {
            Debug.Log("Ltech Adding Button!");

		    // Button that toggles its icon when clicked
            button1 = ToolbarManager.Instance.add("LTech", "LtechConButt");
            button1.TexturePath = "LTech/Icons/toolbarIcon";
            button1.ToolTip = "LTech Config Menu";
            button1.OnClick += (e) =>
            {
                activated = !activated;
            };
	    }
        
	    internal void OnDestroy()
        {
            activated = false;
            button1.Destroy();
	    }
    }
}