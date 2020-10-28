using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LtScience.Utilities;

using KerbalConstructionTime;
using UnityEngine;
using KSP_Log;

namespace LtScience
{
    public class KCT_Interface
    {
        static public bool HasKCT {  get { return SpaceTuxUtility.HasMod.hasMod("KerbalConstructionTime"); } }

        static double KCTAdjustment()
        {
            if (!KCT_GameStates.Enabled)
                return 1f;

            double kctTotals = 0;
            if (HighLogic.CurrentGame.Parameters.CustomParams<LTech_2>().useKCTDevelopment)
                kctTotals = KCT_GameStates.SciencePointsAllocated;
            if (HighLogic.CurrentGame.Parameters.CustomParams<LTech_2>().useKCTResearch)
                kctTotals += KCT_GameStates.TechUpgradePointsAllocated;

            return HighLogic.CurrentGame.Parameters.CustomParams<LTech_2>().efficiencyMultiplierAdjustment * (kctTotals + 1) * Math.Log(1 + 1 / (0.1 * (kctTotals + 1)));
        }
        public static double ResearchTimeAdjustment()
        {
            try
            {
                if (HasKCT && HighLogic.CurrentGame.Parameters.CustomParams<LTech_1>().useEfficiencyMultiplier)
                    return KCTAdjustment();
            } catch (Exception ex)
             {
                Debug.Log("L-Tech: KCT Error, msg: " + ex.Message);
                return 1f; 
            }
            return 1f;
        }
    }
}
