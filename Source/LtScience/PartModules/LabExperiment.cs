using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        print("Open GUI clicked!");
        GUI = config.LabGUI;

		if (!GUI.Active)
        {
            print("GUI is not open, opening!");
            GUI.ActiveLab = this;
            GUI.Active = true;
		}
    }
    
    public override void OnStart(StartState state)
    {
        base.OnStart(state);

        if (state == StartState.Editor)
        {
            return;
        }

        Events["DeployExperiment"].active = false;
        Events["DeployExperiment"].guiActive = false;
        Actions["DeployAction"].active = false;
        this.part.force_activate();
    }
    
    public void DoScienceThing(SkyLabExperimentData node)
    {
        if (checkBoring(vessel, true))
            return;

        if (!canrunexperiment(vessel, node))
        {
            ScreenMessages.PostScreenMessage("You can't run that experiment right now!", 6, ScreenMessageStyle.UPPER_CENTER);
            return;
        }

        if (LTSUseResources("Insight", node.ReqInsight))
        {
        	if (LTSUseResources(node.ReqResource, node.ReqResAmount))
        	{
                base.experimentID = specialExperimentName + node.Name;
                base.experiment.id = specialExperimentName + node.Name;
                base.experiment.experimentTitle = specialExperimentTitle.Replace("#Exp#", node.DisplayName);
                base.experiment.baseValue = node.ScienceVal;
                base.experiment.dataScale = node.DataScale;
                base.DeployExperiment();
        	}
        	else
        	{
                ScreenMessages.PostScreenMessage("Need: " + node.ReqResAmount + " of " + node.ReqResource + "!", 6, ScreenMessageStyle.UPPER_CENTER);
        	}
        }
        else
        {
            ScreenMessages.PostScreenMessage("Need: " + node.ReqInsight + " of Insight!", 6, ScreenMessageStyle.UPPER_CENTER);
        }
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
    	if (GUI.ActiveLab == this)
        {
            GUI.Active = false;
    	}
    }
}