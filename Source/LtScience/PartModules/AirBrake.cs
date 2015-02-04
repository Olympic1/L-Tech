using A;
using System;
using System.Collections;
using System.Collections.Generic;
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
        base.part.stagingIcon = "PARACHUTES";
        base.OnStart(state);
    }
    
	public override void OnUpdate()
    {
		if (this.animSwitch)
		{
            this.targetDrag = stoweddrag;
		}
        else
        {
            this.targetDrag = deployeddrag;
		}

        this.parachuteDrag = Mathf.Lerp(this.parachuteDrag, this.targetDrag, dragrate * Time.deltaTime);
        base.part.maximum_drag = this.parachuteDrag;
    }
	
	public override void OnActive()
	{
		if (!this.staged && this.animSwitch)
		{
            this.staged = true;
            this.Toggle();
		}
	}
    
	public override string GetInfo()
    {
        return "Deployed drag: " + this.deployeddrag + " \n" + "Stowed drag: " + this.stoweddrag;
    }
}