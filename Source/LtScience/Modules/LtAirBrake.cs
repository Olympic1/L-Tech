/*
 * L-Tech Scientific Industries Continued
 * Copyright © 2015-2016, Arne Peirs (Olympic1)
 * Copyright © 2016, linuxgurugamer
 * 
 * Kerbal Space Program is Copyright © 2011-2016 Squad. See http://kerbalspaceprogram.com/.
 * This project is in no way associated with nor endorsed by Squad.
 * 
 * This file is part of Olympic1's L-Tech (Continued). Original author of L-Tech is 'ludsoe' on the KSP Forums.
 * This file was part of the original L-Tech and was written by ludsoe.
 * Copyright © 2015, ludsoe
 * 
 * Continues to be licensed under the MIT License.
 * See <https://opensource.org/licenses/MIT> for full details.
 */

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
