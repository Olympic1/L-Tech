using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SkyLabCore : LTechScienceBase
{
    [KSPField(isPersistant = false)]
    //public float rate = 1;
    public float rate = 5;
    
    [KSPField(isPersistant = false)]
    public int minimumCrew = 0;
    
    [KSPField(isPersistant = true)]
    public bool doResearch = false;
    
    [KSPField(isPersistant = true)]
    public float last_active = 0;
    
    [KSPField(isPersistant = false, guiActive = true, guiName = "Run Lab")]
    public string status = "";
    
    private bool isActive()
    {
        return doResearch && part.protoModuleCrew.Count >= minimumCrew && !SkyLabExperiment.checkBoring(vessel, false);
    }
    
    private float last_production = 0;
    private int crew_count = 0;
    
    private void updateStatus()
    {
        if (isActive())
        {
            if (last_production != 0)
                status = "Generating Insight.";
            else
                status = "Unable to generate Insight.";
        }
        else
        {
            if (!doResearch)
                status = "Paused";
            else if (part.protoModuleCrew.Count < minimumCrew)
                status = "Insufficient crew (" + part.protoModuleCrew.Count + " / " + minimumCrew + ")";
            else if (SkyLabExperiment.checkBoring(vessel, false))
                status = "Location too boring";
            else
                status = "ERROR!";
        }
    }
    
    public void GenerateInsight(float Amount)
    {
        float resource = part.RequestResource("Snacks", Amount);
        last_production = part.RequestResource("Insight", -resource);
        
    	if (last_production == 0)
        {
            part.RequestResource("Snacks", -resource);
    	}
    }
    
    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        
        if (state == StartState.Editor)
        {
            return;
        }
        
        this.part.force_activate();
        
        if (last_active > 0 && isActive())
        {
            double time_diff = Planetarium.GetUniversalTime() - last_active;
            float resourcerate = ((rate * (float)time_diff) / 3600) * (crew_count);
            GenerateInsight(resourcerate);
        }
        
        Events["stopResearch"].active = doResearch;
        Events["startResearch"].active = !doResearch;
        updateStatus();
    }
    
    [KSPEvent(guiActive = true, guiName = "Activate Lab", active = true)]
    public void startResearch()
    {
        if (part.protoModuleCrew.Count < minimumCrew)
        {
            ScreenMessages.PostScreenMessage("Not enough crew in this module.", 6, ScreenMessageStyle.UPPER_CENTER);
            return;
        }
        
        doResearch = true;
        crew_count = (part.protoModuleCrew.Count + 1);
        Events["stopResearch"].active = doResearch;
        Events["startResearch"].active = !doResearch;
        updateStatus();
    }
    
    [KSPEvent(guiActive = true, guiName = "Stop researching", active = true)]
    public void stopResearch()
    {
        doResearch = false;
        Events["stopResearch"].active = doResearch;
        Events["startResearch"].active = !doResearch;
        updateStatus();
    }
    
    [KSPAction("Activate Lab")]
    public void startResearchingAction(KSPActionParam param)
    {
        startResearch();
    }
    
    [KSPAction("Deactivate Lab")]
    public void stopGeneratingAction(KSPActionParam param)
    {
        stopResearch();
    }
    
    [KSPAction("Toggle Lab")]
    public void toggleResearchAction(KSPActionParam param)
    {
        if (doResearch)
            stopResearch();
        else
            startResearch();
    }
    
    public override void OnFixedUpdate()
    {
        if (isActive())
        {
            float resourcerate = ((rate * TimeWarp.fixedDeltaTime) / 3600) * (crew_count);
            GenerateInsight(resourcerate);
            last_active = (float)Planetarium.GetUniversalTime();
        }
    }
    
    public override void OnUpdate()
    {
        updateStatus();
    }
    
    public override string GetInfo()
    {
        return "Researchers required: " + minimumCrew + "\n" + "Insight per hour: " + rate + " * Crew";
    }
}