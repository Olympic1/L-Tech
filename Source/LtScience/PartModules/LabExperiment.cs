using LTScience;

public class SkyLabExperiment : LTechScienceBase
{
    [KSPField]
    public string specialExperimentName;

    [KSPField]
    public string specialExperimentTitle;

    [KSPField(isPersistant = true)]
    public string lastexperiment = "";

    public SkyLabConfig config;
    private SkyLabExperimentGUI GUI;

    [KSPEvent(guiActive = true, guiName = "Open experiment GUI")]
    public void OpenGUI()
    {
        GUI = config.labGUI;

        if (!GUI.uiShown)
        {
            GUI.activeLab = this;
            GUI.uiShown = true;
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
            ScreenMessages.PostScreenMessage("You can't run that experiment right now!\n" + msg, 6, ScreenMessageStyle.UPPER_CENTER);
            return;
        }

        if (LTSUseResources("Insight", node.ReqInsight))
        {
            if (LTSUseResources(node.ReqResource, node.ReqAmount))
            {
                experimentID = specialExperimentName + node.Name;
                experiment.id = specialExperimentName + node.Name;
                experiment.experimentTitle = specialExperimentTitle.Replace("#Exp#", node.DisplayName);
                experiment.baseValue = node.ScienceValue;
                experiment.scienceCap = node.ScienceCap;
                experiment.dataScale = node.DataScale;
                base.DeployExperiment();
            }
            else
                ScreenMessages.PostScreenMessage("Need: " + node.ReqAmount + " of " + node.ReqResource + "!", 6, ScreenMessageStyle.UPPER_CENTER);
        }
        else
            ScreenMessages.PostScreenMessage("Need: " + node.ReqInsight + " of Insight!", 6, ScreenMessageStyle.UPPER_CENTER);
    }

    new public void DeployAction(KSPActionParam p)
    {
        return;
    }

    new public void DeployExperiment()
    {
        return;
    }

    new public void OnDestroy()
    {
        if (GUI != null && GUI.activeLab == this)
            GUI.uiShown = false;
    }
}
