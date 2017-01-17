/*
 * L-Tech Scientific Industries Continued
 * Copyright © 2015-2017, Arne Peirs (Olympic1)
 * Copyright © 2016-2017, linuxgurugamer
 * 
 * Kerbal Space Program is Copyright © 2011-2017 Squad. See http://kerbalspaceprogram.com/.
 * This project is in no way associated with nor endorsed by Squad.
 * 
 * This file is part of Olympic1's L-Tech (Continued). Original author of L-Tech is 'ludsoe' on the KSP Forums.
 * This file was part of the original L-Tech and was written by ludsoe.
 * Copyright © 2015, ludsoe
 * 
 * Continues to be licensed under the MIT License.
 * See <https://opensource.org/licenses/MIT> for full details.
 */

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

        private static readonly string exPath = $"{KSPUtil.ApplicationRootPath}GameData/LTech/Plugins/PluginData";
        private static readonly string exFile = $"{exPath}/SkyLabExperiments.cfg";

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

                    Util.LogMessage($"- ID: {data.Id} - Name: {data.Name}", Util.LogType.Info);

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

            Util.LogMessage($"Successfully added {num} new experiments", Util.LogType.Info);
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
