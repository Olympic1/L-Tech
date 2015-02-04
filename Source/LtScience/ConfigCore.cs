using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LTScience
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class LTechCCore : MonoBehaviour
	{
        public float camres = 1f;
        public float strtime = 0.2f;

        ConfigNode settings;
        private string cfgpath = "GameData/LTech/Config/Settings.cfg";
        
		public void LoadConfig()
		{
            Debug.Log("LTech: loading config!");

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
            LoadConfig();
		}
        
        public void Start()
        {
            Debug.Log("LTech start called");

            /*
            if (HighLogic.LoadedSceneIsEditor || this.vessel == null)
            {
                return;
            }

            foreach (var vessel in FlightGlobals.Vessels)
            {
                vessel.FindPartModulesImplementing<HullCamera>().ForEach(part =>
                {
                    part.config = this;
                });
            }
            */

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
	}
}