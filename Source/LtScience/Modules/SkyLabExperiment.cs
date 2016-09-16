using LtScience.InternalObjects;
using LtScience.Windows;

namespace LtScience.Modules
{
    public class SkyLabExperiment : LtScienceBase
    {
        [KSPField]
        public string specialExperimentTitle;

        [KSPField(isPersistant = true)]
        public string lastExperiment = "";

        public SkyLabConfig config;
        public WindowSkyLab GUI;

        [KSPEvent(guiActive = true, guiName = "Open experiment GUI")]
        public void OpenGui()
        {
            GUI = config.LabGui;

            if (!WindowSkyLab.showWindow)
            {
                GUI.activeLab = this;
                WindowSkyLab.showWindow = true;
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (state == StartState.Editor)
                return;

            Events["DeployExperiment"].active = false;
            Events["DeployExperiment"].guiActive = false;
            Actions["DeployAction"].active = false;
            part.force_activate();
        }

        public void DoScienceThing(SkyLabExperimentData node)
        {
            string msg = "";

            if (CheckBoring(vessel, true))
                return;

            if (!CanRunExperiment(vessel, node, ref msg))
            {
                Util.DisplayScreenMsg("You can't run that experiment right now!\n" + msg);
                return;
            }

            if (LtsUseResources("Insight", node.ReqInsight))
            {
                if (LtsUseResources(node.ReqResource, node.ReqAmount))
                {
                    experimentID = node.Id;
                    experiment.id = node.Id;
                    experiment.experimentTitle = specialExperimentTitle.Replace("#Exp#", node.Name);
                    experiment.baseValue = node.ScienceValue;
                    experiment.scienceCap = node.ScienceCap;
                    experiment.dataScale = node.DataScale;
                    base.DeployExperiment();
                }
                else
                    Util.DisplayScreenMsg("Need " + node.ReqAmount + " " + node.ReqResource + "!");
            }
            else
                Util.DisplayScreenMsg("Need " + node.ReqInsight + " Insight!");
        }

        public new void DeployAction(KSPActionParam p)
        { }

        public new void DeployExperiment()
        { }

        public new void OnDestroy()
        {
            if (GUI != null && GUI.activeLab == this)
                WindowSkyLab.showWindow = false;
        }
    }
}
