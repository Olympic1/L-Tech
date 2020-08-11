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

using static LtScience.Addon;

namespace LtScience.Modules
{
    public class SkylabCore : PartModule
    {
        #region Properties

        [KSPField]
        public float rate = 5f;

        [KSPField]
        public int minimumCrew = 0;

        [KSPField(isPersistant = false, guiActive = true, guiName = "#autoLOC_LTech_LabCore_001")]
        public string status = string.Empty;


        #endregion

        #region Methods

        private void UpdateUI()
        {
            if (minimumCrew > 0 && part.protoModuleCrew.Count < minimumCrew)
                status = Localizer.Format("#autoLOC_LTech_LabCore_005", part.protoModuleCrew.Count, minimumCrew);
            else if (Utils.CheckBoring(vessel))
                status = Localizer.Format("#autoLOC_LTech_LabCore_006");
            else
                DummyStatus();
        }

        protected virtual void DummyStatus()
        {
        }

        #endregion



        #region Event Handlers

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
                return;

            part.force_activate();
            UpdateUI();
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
