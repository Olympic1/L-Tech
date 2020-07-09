/*
 * L-Tech Scientific Industries Continued
 * Copyright © 2015-2018, Arne Peirs (Olympic1)
 * Copyright © 2016-2018, Jonathan Bayer (linuxgurugamer)
 * 
 * Kerbal Space Program is Copyright © 2011-2018 Squad. See https://kerbalspaceprogram.com/.
 * This project is in no way associated with nor endorsed by Squad.
 * 
 * This file is part of Olympic1's L-Tech (Continued). Original author of L-Tech is 'ludsoe' on the KSP Forums.
 * This file was not part of the original L-Tech but was written by Arne Peirs.
 * Copyright © 2015-2018, Arne Peirs (Olympic1)
 * 
 * Continues to be licensed under the MIT License.
 * See <https://opensource.org/licenses/MIT> for full details.
 */

using KSP.Localization;
using LtScience.Utilities;

namespace LtScience.Modules
{
    internal class SkylabCore : PartModule
    {
        #region Properties

        [KSPField]
        public float rate = 5f;

        [KSPField]
        public int minimumCrew = 0;

        [KSPField(isPersistant = true)]
        public bool doResearch;

        [KSPField(isPersistant = true)]
        public double lastActive;

        [KSPField(isPersistant = false, guiActive = true, guiName = "#autoLOC_LTech_LabCore_001")]
        public string status = string.Empty;

        protected virtual bool IsActive()
        {
            return doResearch && part.protoModuleCrew.Count >= minimumCrew && !Utils.CheckBoring(vessel);
        }

        private double _lastProduction;
        private int _crewCount;

        #endregion

        #region Methods

        private void UpdateUI()
        {
            Events["StartResearch"].active = !doResearch;
            Events["StopResearch"].active = doResearch;

            if (IsActive())
            {
                status = _lastProduction != 0.0 ? Localizer.Format("#autoLOC_LTech_LabCore_002") : Localizer.Format("#autoLOC_LTech_LabCore_003");
            }
            else
            {
                if (!doResearch)
                    status = Localizer.Format("#autoLOC_LTech_LabCore_004");
                else if (minimumCrew > 0 && part.protoModuleCrew.Count < minimumCrew)
                    status = Localizer.Format("#autoLOC_LTech_LabCore_005", part.protoModuleCrew.Count, minimumCrew);
                else if (Utils.CheckBoring(vessel))
                    status = Localizer.Format("#autoLOC_LTech_LabCore_006");
                else
                    DummyStatus();
            }
        }

        protected virtual void DummyStatus()
        {
        }

        private void GenerateInsight(double amount)
        {
            double resource = part.RequestResource("Snacks", amount);
            _lastProduction = part.RequestResource("Insight", -resource);

            if (_lastProduction == 0.0)
                part.RequestResource("Snacks", -resource);
        }

        #endregion

        #region KSP Events

        [KSPEvent(guiActive = true, guiName = "#autoLOC_LTech_LabCore_007", active = true)]
        public void StartResearch()
        {
            if (part.protoModuleCrew.Count < minimumCrew)
            {
                Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_LabCore_008"));
                return;
            }

            doResearch = true;
            _crewCount = part.protoModuleCrew.Count + 1;
            UpdateUI();
        }

        [KSPEvent(guiActive = true, guiName = "#autoLOC_LTech_LabCore_009", active = true)]
        public void StopResearch()
        {
            doResearch = false;
            UpdateUI();
        }

        #endregion

        #region KSP Actions

        [KSPAction("#autoLOC_LTech_LabCore_007")]
        public void StartResearchAction(KSPActionParam param)
        {
            StartResearch();
        }

        [KSPAction("#autoLOC_LTech_LabCore_009")]
        public void StopResearchAction(KSPActionParam param)
        {
            StopResearch();
        }

        [KSPAction("#autoLOC_LTech_LabCore_010")]
        public void ToggleResearchAction(KSPActionParam param)
        {
            if (doResearch)
                StopResearch();
            else
                StartResearch();
        }

        #endregion

        #region Event Handlers

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
                return;

            part.force_activate();
            if (lastActive > 0 && IsActive())
            {
                double timeDiff = Planetarium.GetUniversalTime() - lastActive;
                double resourceRate = rate * timeDiff / 500 * _crewCount;
                GenerateInsight(resourceRate);
            }

            UpdateUI();
        }

        public override void OnFixedUpdate()
        {
            if (IsActive())
            {
                double resourceRate = rate * TimeWarp.fixedDeltaTime / 500 * _crewCount;
                GenerateInsight(resourceRate);
                lastActive = Planetarium.GetUniversalTime();
            }
            else
            {
                lastActive = 0.0;
            }
        }

        public override void OnUpdate()
        {
            UpdateUI();
        }

        public override string GetInfo()
        {
            return Localizer.Format("#autoLOC_LTech_LabCore_011", minimumCrew);
        }

        #endregion
    }
}
