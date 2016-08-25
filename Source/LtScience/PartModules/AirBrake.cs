using UnityEngine;

public class LTechAirBrake : ModuleAnimateGeneric
{
    [KSPField(isPersistant = false)]
    public float deployeddrag = 300f;

    [KSPField(isPersistant = false)]
    public float stoweddrag = 0.05f;

    [KSPField(isPersistant = false)]
    public float dragrate = 0.1f;

    public float targetDrag;

    private float parachuteDrag;
    private bool staged = false;

    public override void OnStart(StartState state)
    {
        part.stagingIcon = "PARACHUTES";
        base.OnStart(state);
    }

    public override void OnUpdate()
    {
        if (animSwitch)
            targetDrag = stoweddrag;
        else
            targetDrag = deployeddrag;

        parachuteDrag = Mathf.Lerp(parachuteDrag, targetDrag, dragrate * Time.deltaTime);
        part.maximum_drag = parachuteDrag;
    }

    public override void OnActive()
    {
        if (!staged && animSwitch)
        {
            staged = true;
            Toggle();
        }
    }

    public override string GetInfo()
    {
        return "Deployed drag: " + deployeddrag + " \n" + "Stowed drag: " + stoweddrag;
    }
}
