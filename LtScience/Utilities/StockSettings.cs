using System.Collections;
using System.Reflection;

namespace LtScience.Utilities
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    // HighLogic.CurrentGame.Parameters.CustomParams<LTech>().useAltSkin
    public class LTech_1 : GameParameters.CustomParameterNode
    {
        internal static LTech_1 fetch = null;
        public override string Title { get { return ""; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "L-Tech"; } }
        public override string DisplaySection { get { return "L-Tech"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }


        [GameParameters.CustomParameterUI("Use Efficiency multiplier calcuated from KCT",
                  toolTip = "If both Research & Development are false, this will be set to false")]
        public bool useEfficiencyMultiplier = true;

        [GameParameters.CustomParameterUI("Use alternate skin",
          toolTip = "Use a more minimiliast skin")]
        public bool useAltSkin = true;



        public override void SetDifficultyPreset(GameParameters.Preset preset) { }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            return true;
        }
        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            fetch = this;
            return true;
        }
        public override IList ValidValues(MemberInfo member) { return null; }
    }

    public class LTech_2 : GameParameters.CustomParameterNode
    {
        public override string Title { get { return ""; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "L-Tech"; } }
        public override string DisplaySection { get { return "L-Tech"; } }
        public override int SectionOrder { get { return 2; } }
        public override bool HasPresets { get { return true; } }

        [GameParameters.CustomParameterUI("Use KCT Research values",
          toolTip = "")]
        public bool useKCTResearch = true;

        [GameParameters.CustomParameterUI("Use KCT Development values",
                  toolTip = "")]
        public bool useKCTDevelopment = true;



        public float efficiencyMultiplierAdjustment = 1f;
        [GameParameters.CustomFloatParameterUI("Efficiency Multiplier Adjustment (%)", minValue = 10f, maxValue = 100.0f,
                 toolTip = "Only used with KCT, adjusts the effiency multipler which is based on the research and/or Development.  The lower it is, the longer experiments will take to be completed")]
        public float CostAdj
        {
            get { return efficiencyMultiplierAdjustment * 100; }
            set { efficiencyMultiplierAdjustment = value / 100.0f; }
        }


        public override void SetDifficultyPreset(GameParameters.Preset preset) { }

        public override bool Enabled(MemberInfo member, GameParameters parameters) { return true; }
        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "useKCTResearch" || member.Name == "useKCTDevelopment" || member.Name == "CostAdj")
            {
                return LTech_1.fetch.useEfficiencyMultiplier;
            }

            return true;
        }
        public override IList ValidValues(MemberInfo member) { return null; }
    }

}