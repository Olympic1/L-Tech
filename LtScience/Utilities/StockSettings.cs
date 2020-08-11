using System.Collections;
using System.Reflection;

namespace LtScience.Utilities
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    // HighLogic.CurrentGame.Parameters.CustomParams<LTech>().useAltSkin
    public class LTech : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "L-Tech"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "L-Tech"; } }
        public override string DisplaySection { get { return "L-Tech"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }


        [GameParameters.CustomParameterUI("Use alternate skin",
                  toolTip = "Use a more minimiliast skin")]
        public bool useAltSkin = true;


        public override void SetDifficultyPreset(GameParameters.Preset preset) { }
        public override bool Enabled(MemberInfo member, GameParameters parameters) { return true; }
        public override bool Interactible(MemberInfo member, GameParameters parameters) { return true; }
        public override IList ValidValues(MemberInfo member) { return null; }
    }
}