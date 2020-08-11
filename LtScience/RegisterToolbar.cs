
using UnityEngine;
using ToolbarControl_NS;
using KSP_Log;

using static LtScience.Addon;

namespace LtScience
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        protected void Start()
        {
            ToolbarControl.RegisterMod(Addon.MODID, Addon.MODNAME);
        }
    }

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class InitLog : MonoBehaviour
    {
        protected void Awake()
        {
            Addon.Log = new KSP_Log.Log("L-Tech"
#if DEBUG
                , KSP_Log.Log.LEVEL.INFO
#endif
                );
        }
    }

}
