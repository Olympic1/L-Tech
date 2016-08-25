using KSP.UI.Screens;
using UnityEngine;

namespace LTScience
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class LTechCCore : MonoBehaviour
    {
        protected Rect windowPos = new Rect(20, 20, 320, 220);
        protected int windowID = 5234629;
        private bool uiShown = false;

        private string icon = "LTech/Plugins/LT-toolbar";

        private static ApplicationLauncherButton appButton = null;

        public float camres = 1f;
        public float strtime = 0.2f;

        ConfigNode settings;
        private string cfgpath = "GameData/LTech/Config/Settings.cfg";

        public void LoadConfig()
        {
            Debug.Log("LTech: Loading config!");

            // Load settings
            settings = ConfigNode.Load(KSPUtil.ApplicationRootPath + cfgpath);
            foreach (ConfigNode node in settings.GetNodes("CameraSettings"))
            {
                camres = float.Parse(node.GetValue("cameraresolution"));
                strtime = float.Parse(node.GetValue("camerashuttertime"));
            }
        }

        public void SaveConfig()
        {
            settings.GetNode("CameraSettings").SetValue("cameraresolution", "" + camres);
            settings.GetNode("CameraSettings").SetValue("camerashuttertime", "" + strtime);

            settings.Save(KSPUtil.ApplicationRootPath + cfgpath);
        }

        public void ResetConfig()
        {
            camres = 1f;
            strtime = 0.2f;

            SaveConfig();
        }

        public void Awake()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);

            LoadConfig();
        }

        public void Start()
        {
            Debug.Log("LTech: Start");

            if (ApplicationLauncher.Ready)
                OnGUIAppLauncherReady();

            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                foreach (var vessel in FlightGlobals.Vessels)
                {
                    vessel.FindPartModulesImplementing<HullCamera>().ForEach(part =>
                    {
                        part.config = this;
                    });
                }
            }
        }

        public void OnGUI()
        {
            OnUIDraw();
        }

        public void OnUIDraw()
        {
            if (uiShown)
                windowPos = GUILayout.Window(windowID, windowPos, DrawWindow, "LTech Config", GUILayout.MinHeight(20), GUILayout.ExpandHeight(true));
        }

        public void DrawWindow(int WindowID)
        {
            GUILayout.BeginVertical();

            // Camera Resolution
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Label("Resolution (Measured in game window resolutions)");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            camres = GUILayout.HorizontalSlider(camres, 1f, 5f, GUILayout.Width(240));
            GUILayout.Label(string.Format("{0:0}", 100 * camres) + " %");
            GUILayout.EndHorizontal();

            // Camera ShutterTime
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Label("ShutterTime (Seconds the game holds in camera view.)");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            strtime = GUILayout.HorizontalSlider(strtime, 0.2f, 5f, GUILayout.Width(240));
            GUILayout.Label(string.Format("{0:0}", strtime) + " sec");
            GUILayout.EndHorizontal();

            // Divider
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Label("_____________________________________");
            GUILayout.EndHorizontal();

            // Reset button
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));

            if (GUILayout.Button("Reset to default"))
                ResetConfig();

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));

            // Apply button
            if (GUILayout.Button("Apply", GUILayout.Height(50)))
            {
                SaveConfig();
                uiShown = false;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        public void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);

            if (appButton != null)
                ApplicationLauncher.Instance.RemoveModApplication(appButton);
        }

        private void OnToolbarButtonToggle()
        {
            uiShown = !uiShown;
            appButton.SetTexture(GameDatabase.Instance.GetTexture(icon, false));
        }

        void OnGUIAppLauncherReady()
        {
            appButton = ApplicationLauncher.Instance.AddModApplication(
                OnToolbarButtonToggle,
                OnToolbarButtonToggle,
                DummyVoid,
                DummyVoid,
                DummyVoid,
                DummyVoid,
                ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.FLIGHT,
                GameDatabase.Instance.GetTexture(icon, false));
        }

        void OnGUIAppLauncherDestroyed()
        {
            if (appButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appButton);
                appButton = null;
            }
        }

        void onAppLaunchToggleOff()
        {
            appButton.SetTexture(GameDatabase.Instance.GetTexture(icon, false));
        }

        void DummyVoid() { }
    }
}
