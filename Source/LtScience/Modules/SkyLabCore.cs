using LtScience.APIClients;
using LtScience.InternalObjects;

namespace LtScience.Modules
{
    public class SkyLabCore : LtScienceBase
    {
        [KSPField(isPersistant = false)]
        public float rate = 5;

        [KSPField(isPersistant = false)]
        public int minimumCrew = 0;

        [KSPField(isPersistant = true)]
        public bool doResearch;

        [KSPField(isPersistant = true)]
        public float lastActive;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Run Lab")]
        public string status = "";

        private bool IsActive()
        {
            return doResearch && part.protoModuleCrew.Count >= minimumCrew && !CheckBoring(vessel);
        }

        private float _lastProduction;
        private int _crewCount;

        private void UpdateStatus()
        {
            if (IsActive())
            {
                if (_lastProduction != 0)
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
                else if (CheckBoring(vessel))
                    status = "Location too boring";
                else
                    status = "ERROR!";
            }
        }

        private void GenerateInsight(float amount)
        {
            if (InstalledMods.IsTacInstalled)
            {
                //Util.LogMessage("TAC Life Support is installed. Use Food as resource.", Util.LogType.Info);
                // Generate Insight using Food
            }
            else if (InstalledMods.IsUsiInstalled)
            {
                //Util.LogMessage("USI Life Support is installed. Use Food or Supplies as resource.", Util.LogType.Info);
                // Generate Insight using Food or Supplies
            }
            else if (InstalledMods.IsSnacksInstalled)
            {
                //Util.LogMessage("Snacks is installed. Use Snacks as resource.", Util.LogType.Info);
                // Generate Insight using Snacks
            }
            else
            {
                //Util.LogMessage("No Life Support is installed. Use magical unicorns as resource.", Util.LogType.Info);
                // Just generate Insight
            }

            float resource = part.RequestResource("Snacks", amount);
            _lastProduction = part.RequestResource("Insight", -resource);

            if (_lastProduction == 0)
                part.RequestResource("Snacks", -resource);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (state == StartState.Editor)
                return;

            part.force_activate();

            if (lastActive > 0 && IsActive())
            {
                double timeDiff = Planetarium.GetUniversalTime() - lastActive;
                float resourcerate = ((rate * (float)timeDiff) / 3600) * (_crewCount);
                GenerateInsight(resourcerate);
            }

            Events["StopResearch"].active = doResearch;
            Events["StartResearch"].active = !doResearch;
            UpdateStatus();
        }

        [KSPEvent(guiActive = true, guiName = "Start research (LT)", active = true)]
        private void StartResearch()
        {
            if (part.protoModuleCrew.Count < minimumCrew)
            {
                Util.DisplayScreenMsg("Not enough crew in this module.");
                return;
            }

            doResearch = true;
            _crewCount = (part.protoModuleCrew.Count + 1);
            Events["StopResearch"].active = doResearch;
            Events["StartResearch"].active = !doResearch;
            UpdateStatus();
        }

        [KSPEvent(guiActive = true, guiName = "Stop research (LT)", active = true)]
        private void StopResearch()
        {
            doResearch = false;
            Events["StopResearch"].active = doResearch;
            Events["StartResearch"].active = !doResearch;
            UpdateStatus();
        }

        [KSPAction("Activate Lab (LT)")]
        public void StartResearchingAction(KSPActionParam param)
        {
            StartResearch();
        }

        [KSPAction("Deactivate Lab (LT)")]
        public void StopGeneratingAction(KSPActionParam param)
        {
            StopResearch();
        }

        [KSPAction("Toggle Lab (LT)")]
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
                float resourcerate = ((rate * TimeWarp.fixedDeltaTime) / 3600) * (_crewCount);
                GenerateInsight(resourcerate);
                lastActive = (float)Planetarium.GetUniversalTime();
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
}
