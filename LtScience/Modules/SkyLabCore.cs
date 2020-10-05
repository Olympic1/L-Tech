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
using System.Collections;
using LtScience.Utilities;
using UnityEngine;


using static LtScience.Addon;

namespace LtScience.Modules
{
    public class SkylabCore : PartModule
    {
        #region Properties

        [KSPField]
        public int minimumCrew = 0;

        [KSPField]
        public float minCrewScienceExp = 0;

        [KSPField(isPersistant = false, guiActive = true, guiName = "#autoLOC_LTech_LabCore_001")]
        public string status = string.Empty;


        #endregion

        #region Methods

        private void UpdateUI()
        {
            if (minimumCrew > 0 && part.protoModuleCrew.Count < minimumCrew)
            {
                status = Localizer.Format("#autoLOC_LTech_LabCore_005", part.protoModuleCrew.Count, minimumCrew);
                return;
            }
            var f = GetCrewScientistTotals();
             if (f < minCrewScienceExp)
            {
                status = Localizer.Format("#autoLOC_LTech_LabCore_013", f, minCrewScienceExp);
                return;
            }
           if (Utils.CheckBoring(vessel))
            {
                status = Localizer.Format("#autoLOC_LTech_LabCore_006");
                return;
            }
            else
                DummyStatus();
        }

        protected virtual void DummyStatus()
        {
            status = Localizer.Format("#autoLOC_LTech_LabCore_012");
        }

        internal float GetCrewScientistTotals()
        {
            return GetCrewTraitTotals(KerbalRoster.scientistTrait);
        }
        private float GetCrewTraitTotals(string kerbalTrait)
        {
            float experienceTotals = 0;
            for (int i = 0; i < part.protoModuleCrew.Count; i++)
            {
                if (part.protoModuleCrew[i] != null)
                {
                    if (part.protoModuleCrew[i].trait == kerbalTrait)
                    {
                        // num++;
                        experienceTotals += part.protoModuleCrew[i].experienceTrait.CrewMemberExperienceLevel();
                    }
                }
            }
            //Log.Info("GetCrewTraitTotals, trait: " + kerbalTrait + ",  experience: " + experienceTotals);
            return experienceTotals;
        }

        #endregion



        #region Event Handlers

        public void Start()
        {
            if (HighLogic.LoadedSceneIsEditor)
                return;

            part.force_activate();
            StartCoroutine("SlowUpdate");
        }

        IEnumerator SlowUpdate()
        {
            while (true)
            {
                UpdateUI();
                yield return new WaitForSecondsRealtime(0.25f);
            }
        }



        public override string GetInfo()
        {
            return Localizer.Format("#autoLOC_LTech_LabCore_011", minimumCrew);
        }

        #endregion
    }
}
