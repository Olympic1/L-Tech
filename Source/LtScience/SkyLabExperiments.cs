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
                Xper.DisplayName = node.GetValue("DisplayName");

                Xper.ReqResource = node.GetValue("ReqResource");
                Xper.ReqAmount = int.Parse(node.GetValue("ReqAmount"));
                Xper.ReqInsight = int.Parse(node.GetValue("ReqInsight"));

                Xper.ScienceValue = int.Parse(node.GetValue("Science"));
                Xper.ScienceCap = int.Parse(node.GetValue("Cap"));
                Xper.DataScale = int.Parse(node.GetValue("Scale"));

                Xper.ReqAtmo = bool.Parse(node.GetValue("ReqAtmo"));

                Xper.landed = bool.Parse(node.GetValue("Landed"));
                Xper.splashed = bool.Parse(node.GetValue("Splashed"));
                Xper.flying = bool.Parse(node.GetValue("Flying"));
                Xper.space = bool.Parse(node.GetValue("Space"));

                experiments[Num] = Xper;
                Num += 1;
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
        public int ReqAmount = 0;
        public int ReqInsight = 0;

        public int ScienceValue = 0;
        public int ScienceCap = 0;
        public int DataScale = 0;

        public bool ReqAtmo = false;

        public bool landed = false;
        public bool splashed = false;
        public bool flying = false;
        public bool space = false;
    }
}
