/*
 * L-Tech Scientific Industries Continued
 * Copyright © 2015-2017, Arne Peirs (Olympic1)
 * Copyright © 2016-2017, linuxgurugamer
 * 
 * Kerbal Space Program is Copyright © 2011-2017 Squad. See http://kerbalspaceprogram.com/.
 * This project is in no way associated with nor endorsed by Squad.
 * 
 * This file is part of Olympic1's L-Tech (Continued). Original author of L-Tech is 'ludsoe' on the KSP Forums.
 * This file was not part of the original L-Tech but was written by Arne Peirs.
 * Copyright © 2015-2017, Arne Peirs (Olympic1)
 * 
 * Continues to be licensed under the MIT License.
 * See <https://opensource.org/licenses/MIT> for full details.
 */

using UnityEngine;

namespace LtScience.Windows
{
    internal static class LtStyle
    {
        internal static GUIStyle WindowStyle;
        internal static GUIStyle ButtonStyle;
        internal static GUIStyle ButtonToggledStyle;
        internal static GUIStyle ToggleStyleHeader;
        internal static GUIStyle LabelStyle;
        internal static GUIStyle LabelTabHeader;
        internal static GUIStyle LabelStyleHardRule;
        internal static GUIStyle ToolTipStyle;
        internal static GUIStyle ScrollStyle;

        internal static void SetupGuiStyles()
        {
            if (WindowStyle != null)
                return;

            SetStyles();
        }

        private static void SetStyles()
        {
            WindowStyle = new GUIStyle(GUI.skin.window);

            ButtonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = Color.white },
                hover = { textColor = Color.blue },
                fontSize = 12,
                padding =
                {
                    top = 0,
                    bottom = 0
                },
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip
            };

            ButtonToggledStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = Color.green },
                hover = { textColor = Color.blue },
                fontSize = 12,
                padding =
                {
                    top = 0,
                    bottom = 0
                },
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip
            };
            ButtonToggledStyle.normal.background = ButtonToggledStyle.onActive.background;

            ToggleStyleHeader = new GUIStyle(GUI.skin.toggle)
            {
                padding =
                {
                    top = 10,
                    bottom = 6
                },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerLeft,
                wordWrap = false,
                margin = new RectOffset(0, 0, 0, 0)
            };

            LabelStyle = new GUIStyle(GUI.skin.label);

            LabelTabHeader = new GUIStyle(GUI.skin.label)
            {
                padding =
                {
                    top = 10,
                    bottom = 6
                },
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                margin = new RectOffset(0, 0, 0, 0)
            };

            LabelStyleHardRule = new GUIStyle(GUI.skin.label)
            {
                padding =
                {
                    top = 0,
                    bottom = 6
                },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerLeft,
                wordWrap = false,
                margin = new RectOffset(0, 0, 0, 0)
            };

            ToolTipStyle = new GUIStyle(GUI.skin.textArea)
            {
                normal = { textColor = Color.green },
                hover = { textColor = Color.green },
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                border = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(5, 5, 5, 5)
            };
            ToolTipStyle.hover.background = ToolTipStyle.normal.background;

            ScrollStyle = new GUIStyle(GUI.skin.box);

            if (GUI.skin != null)
                GUI.skin = null;

            GUI.skin = HighLogic.Skin;
        }
    }
}
