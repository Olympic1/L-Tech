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

using UnityEngine;

namespace LtScience.Utilities
{
    internal static class Style
    {
        internal static GUIStyle WindowStyle;
        internal static GUIStyle LabelHeader;
        internal static GUIStyle LabelStyleBold;
        internal static GUIStyle LabelStyleHardRule;

        internal static void SetupGuiStyles()
        {
            if (WindowStyle != null)
                return;

            SetStyles();
        }

        private static void SetStyles()
        {
            WindowStyle = new GUIStyle(GUI.skin.window);

            LabelHeader = new GUIStyle(GUI.skin.label)
            {
                padding =
                {
                    top = 10,
                    bottom = 6
                },
                margin = new RectOffset(0, 0, 0, 0),
                fontStyle = FontStyle.Bold,
                wordWrap = false
            };

            LabelStyleBold = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };

            LabelStyleHardRule = new GUIStyle(GUI.skin.label)
            {
                padding =
                {
                    top = 0,
                    bottom = 6
                },
                margin = new RectOffset(0, 0, 0, 0),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerLeft,
                wordWrap = false
            };

            if (GUI.skin != null)
                GUI.skin = null;

            GUI.skin = HighLogic.Skin;
        }
    }
}
