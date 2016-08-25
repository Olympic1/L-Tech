using System;
using LTScience;

public class LTechScienceBase : ModuleScienceExperiment
{
    private int resource = 0;

    public static bool checkBoring(Vessel vessel, bool msg = false)
    {
        if ((vessel.orbit.referenceBody.name == "Kerbin") && (vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.PRELAUNCH || vessel.situation == Vessel.Situations.SPLASHED || vessel.altitude <= vessel.orbit.referenceBody.atmosphereDepth))
        {
            if (msg)
                ScreenMessages.PostScreenMessage("Too boring here. Go to space!", 6, ScreenMessageStyle.UPPER_CENTER);

            return true;
        }

        return false;
    }

    // Simple system to prevent some experiments from being run in places they can't be ran.
    public bool canrunexperiment(Vessel vessel, SkyLabExperimentData node, ref string msg)
    {
        msg = "Vessel must be (one of): ";

        if (node.landed)
        {
            if (vessel.situation == Vessel.Situations.LANDED)
                return true;
            msg += "landed, ";
        }

        if (node.splashed)
        {
            if (vessel.situation == Vessel.Situations.SPLASHED)
                return true;
            msg += "splashed down, ";
        }

        if (node.flying)
        {
            if (vessel.altitude <= vessel.orbit.referenceBody.atmosphereDepth)
                return true;
            msg += "flying, ";
        }

        if (node.space)
        {
            if (vessel.altitude > vessel.orbit.referenceBody.atmosphereDepth)
                return true;
            msg += "in space, ";
        }

        msg = msg.Remove(msg.Length - 2);
        return false;
    }

    public PartResource getResource(string name)
    {
        PartResourceList resourceList = this.part.Resources;
        return resourceList.list.Find(delegate (PartResource cur)
        {
            return (cur.resourceName == name);
        });
    }

    public int getResourceAmount(string name)
    {
        PartResource res = getResource(name);

        if (res == null)
            return 0;

        return (int)Math.Floor(res.amount);
    }

    // Checks if a craft has an amount of resources without actually taking them.
    public bool LTSHasResources(string name, int amount)
    {
        resource = (int)part.RequestResource(name, amount);
        part.RequestResource(name, -(resource));
        return (resource == amount);
    }

    // Some ease function I guess... whatever
    public bool LTSUseResources(string name, int amount)
    {
        resource = (int)part.RequestResource(name, amount);

        if (resource == amount)
            return true;

        part.RequestResource(name, -(resource));
        return false;
    }
}
