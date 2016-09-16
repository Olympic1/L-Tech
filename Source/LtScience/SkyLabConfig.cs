using LtScience.InternalObjects;
using LtScience.Modules;
using LtScience.Windows;
using System;
using UnityEngine;

namespace LtScience
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SkyLabConfig : MonoBehaviour
    {
        private static ConfigNode settings;

        private static readonly string exPath = string.Format("{0}GameData/LTech/Plugins/PluginData", KSPUtil.ApplicationRootPath);
        private static readonly string exFile = string.Format("{0}/SkyLabExperiments.cfg", exPath);

        public WindowSkyLab LabGui;
        public static SkyLabExperimentData[] Experiments;

        private static void LoadExperiments()
        {
            // Load settings
            settings = ConfigNode.Load(exFile);
            Experiments = new SkyLabExperimentData[settings.CountNodes];
            int num = 0;

            foreach (ConfigNode node in settings.GetNodes("LT_EXPERIMENT"))
            {
                ScienceExperiment exp;

                // Some apparently not impossible errors can cause duplicate experiments to be added to the R&D science experiment dictionary
                try
                {
                    exp = ResearchAndDevelopment.GetExperiment(node.GetValue("ID"));
                }
                catch (Exception ex)
                {
                    Util.LogMessage("Found a duplicate science experiment definition. Error: " + ex, Util.LogType.Error);
                    continue;
                }

                if (exp != null)
                {
                    Util.LogMessage("Loading experiment...", Util.LogType.Info);

                    SkyLabExperimentData data = new SkyLabExperimentData();

                    data.Id = node.GetValue("ID");
                    data.Name = node.GetValue("Name");

                    Util.LogMessage(string.Format("- ID: {0} - Name: {1}", data.Id, data.Name), Util.LogType.Info);

                    data.ReqResource = node.GetValue("ReqResource");
                    data.ReqAmount = int.Parse(node.GetValue("ReqAmount"));
                    data.ReqInsight = int.Parse(node.GetValue("ReqInsight"));

                    data.ScienceValue = int.Parse(node.GetValue("Science"));
                    data.ScienceCap = int.Parse(node.GetValue("Cap"));
                    data.DataScale = int.Parse(node.GetValue("Scale"));

                    data.ReqAtmo = bool.Parse(node.GetValue("ReqAtmo"));

                    data.Landed = bool.Parse(node.GetValue("Landed"));
                    data.Splashed = bool.Parse(node.GetValue("Splashed"));
                    data.Flying = bool.Parse(node.GetValue("Flying"));
                    data.Space = bool.Parse(node.GetValue("Space"));

                    Experiments[num] = data;
                    num += 1;
                }
            }

            Util.LogMessage(string.Format("Successfully added {0} new experiments", num), Util.LogType.Info);
        }

        public void Awake()
        {
            LoadExperiments();

            LabGui = new WindowSkyLab();
            LabGui.configcore = this;
        }

        private void OnGUI()
        {
            LabGui.DrawGui();
        }

        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                foreach (Vessel vessel in FlightGlobals.Vessels)
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
        public string Id = "Error";
        public string Name = "Error";

        public string ReqResource = "";
        public int ReqAmount;
        public int ReqInsight;

        public int ScienceValue;
        public int ScienceCap;
        public int DataScale;

        public bool ReqAtmo;

        public bool Landed;
        public bool Splashed;
        public bool Flying;
        public bool Space;
    }
}
