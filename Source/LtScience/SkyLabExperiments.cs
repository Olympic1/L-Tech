using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LTScience
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SkyLabConfig : MonoBehaviour
    {
        ConfigNode settings;
        public SkyLabExperimentGUI labGUI;

        private string cfgpath = "GameData/LTech/Config/SkyLabExperiments.cfg";

        public SkyLabExperimentData[] experiments;

        public void LoadExperiments()
        {
            // Load settings
            settings = ConfigNode.Load(KSPUtil.ApplicationRootPath + cfgpath);
            experiments = new SkyLabExperimentData[settings.CountNodes];
            int Num = 0;

            foreach (ConfigNode node in settings.GetNodes("Experiment"))
            {
                SkyLabExperimentData Xper = new SkyLabExperimentData();

                Xper.Name = node.GetValue("Name");
                print("Loading Experiment " + Xper.Name);

                Xper.DisplayName = node.GetValue("DisplayName");
                print("Display Name: " + Xper.DisplayName);

                Xper.ReqResource = node.GetValue("ReqResource");
                Xper.ReqResAmount = int.Parse(node.GetValue("ReqResAmount"));
                print("Resource: " + Xper.ReqResAmount + " of " + Xper.ReqResource);

                Xper.ReqInsight = int.Parse(node.GetValue("ReqInsight"));
                print("Insight: " + Xper.ReqInsight);

                Xper.ScienceVal = int.Parse(node.GetValue("Science"));
                Xper.DataScale = int.Parse(node.GetValue("Data"));
                print("Science: " + Xper.ScienceVal + " Data: " + Xper.DataScale);

                Xper.landed = bool.Parse(node.GetValue("Landed"));
                Xper.splashed = bool.Parse(node.GetValue("Splashed"));
                Xper.space = bool.Parse(node.GetValue("Space"));
                Xper.flying = bool.Parse(node.GetValue("Flying"));

                experiments[Num] = Xper;
                Num = Num + 1;
            }
        }

        public void Awake()
        {
            LoadExperiments();

            labGUI = new SkyLabExperimentGUI();
            labGUI.configcore = this;
        }

        private void OnGUI()
        {
            labGUI.DrawUI();
        }

        public void Start()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                foreach (var vessel in FlightGlobals.Vessels)
                {
                    vessel.FindPartModulesImplementing<SkyLabExperiment>().ForEach(part =>
                    {
                        part.config = this;
                    });
                }
            }
        }
    }

    public class SkyLabExperimentData : MonoBehaviour
    {
        public string Name = "Error";
        public string DisplayName = "Error";

        public string ReqResource = "";
        public int ReqResAmount = 0;
        public int ReqInsight = 0;

        public int ScienceVal = 0;
        public int DataScale = 0;
        public int sciencecap = 0;

        public bool flying = false;
        public bool space = false;
        public bool splashed = false;
        public bool landed = false;
    }
}
