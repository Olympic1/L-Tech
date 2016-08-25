public class SkyLabCore : LTechScienceBase
{
    [KSPField(isPersistant = false)]
    public float rate = 5;

    [KSPField(isPersistant = false)]
    public int minimumCrew = 0;

    [KSPField(isPersistant = true)]
    public bool doResearch = false;

    [KSPField(isPersistant = true)]
    public float last_active = 0;

    [KSPField(isPersistant = false, guiActive = true, guiName = "Run Lab")]
    public string status = "";

    private bool IsActive()
    {
        return doResearch && part.protoModuleCrew.Count >= minimumCrew && !CheckBoring(vessel, false);
    }

    private float last_production = 0;
    private int crew_count = 0;

    private void UpdateStatus()
    {
        if (IsActive())
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
                status = "Insufficient crew (add " + (minimumCrew - part.protoModuleCrew.Count) + " more crew)";
            else if (CheckBoring(vessel, false))
                status = "Location too boring";
            else
                status = "ERROR!";
        }
    }

    public void GenerateInsight(float Amount)
    {
        // TODO: Disable when a life support mod is installed

        float resource = part.RequestResource("Snacks", Amount);
        last_production = part.RequestResource("Insight", -resource);

        if (last_production == 0)
            part.RequestResource("Snacks", -resource);
    }

    public override void OnStart(StartState state)
    {
        base.OnStart(state);

        if (state == StartState.Editor)
            return;

        part.force_activate();

        if (last_active > 0 && IsActive())
        {
            double time_diff = Planetarium.GetUniversalTime() - last_active;
            float resourcerate = ((rate * (float)time_diff) / 3600) * (crew_count);
            GenerateInsight(resourcerate);
        }

        Events["StopResearch"].active = doResearch;
        Events["StartResearch"].active = !doResearch;
        UpdateStatus();
    }

    [KSPEvent(guiActive = true, guiName = "Start research", active = true)]
    public void StartResearch()
    {
        if (part.protoModuleCrew.Count < minimumCrew)
        {
            ScreenMessages.PostScreenMessage("Not enough crew in this module.", 6, ScreenMessageStyle.UPPER_CENTER);
            return;
        }

        doResearch = true;
        crew_count = (part.protoModuleCrew.Count + 1);
        Events["StopResearch"].active = doResearch;
        Events["StartResearch"].active = !doResearch;
        UpdateStatus();
    }

    [KSPEvent(guiActive = true, guiName = "Stop research", active = true)]
    public void StopResearch()
    {
        doResearch = false;
        Events["StopResearch"].active = doResearch;
        Events["StartResearch"].active = !doResearch;
        UpdateStatus();
    }

    [KSPAction("Activate Lab")]
    public void StartResearchingAction(KSPActionParam param)
    {
        StartResearch();
    }

    [KSPAction("Deactivate Lab")]
    public void StopGeneratingAction(KSPActionParam param)
    {
        StopResearch();
    }

    [KSPAction("Toggle Lab")]
    public void ToggleResearchAction(KSPActionParam param)
    {
        if (doResearch)
            StopResearch();
        else
            StartResearch();
    }

    public override void OnFixedUpdate()
    {
        if (IsActive())
        {
            float resourcerate = ((rate * TimeWarp.fixedDeltaTime) / 3600) * (crew_count);
            GenerateInsight(resourcerate);
            last_active = (float)Planetarium.GetUniversalTime();
        }
    }

    public override void OnUpdate()
    {
        UpdateStatus();
    }

    public override string GetInfo()
    {
        return "Researchers required: " + minimumCrew + "\n" + "Insight per hour: " + rate + " * Crew";
    }
}
