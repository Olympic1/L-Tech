using UnityEngine;

namespace LtScience.Modules
{
    public class LtAirBrake : ModuleAnimateGeneric
    {
        [KSPField(isPersistant = false)]
        public float deployedDrag = 300f;

        [KSPField(isPersistant = false)]
        public float stowedDrag = 0.05f;

        [KSPField(isPersistant = false)]
        public float dragRate = 0.1f;

        private float _targetDrag;
        private float _parachuteDrag;
        private bool _staged;

        public override void OnStart(StartState state)
        {
            part.stagingIcon = "PARACHUTES";
            base.OnStart(state);
        }

        public override void OnUpdate()
        {
            if (animSwitch)
                _targetDrag = stowedDrag;
            else
                _targetDrag = deployedDrag;

            _parachuteDrag = Mathf.Lerp(_parachuteDrag, _targetDrag, dragRate * Time.deltaTime);
            part.maximum_drag = _parachuteDrag;
        }

        public override void OnActive()
        {
            if (!_staged && animSwitch)
            {
                _staged = true;
                Toggle();
            }
        }

        public override string GetInfo()
        {
            return "Deployed drag: " + deployedDrag + " \n" + "Stowed drag: " + stowedDrag;
        }
    }
}
