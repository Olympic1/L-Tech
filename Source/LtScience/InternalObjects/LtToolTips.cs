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

using LtScience.Windows;
using UnityEngine;

namespace LtScience.InternalObjects
{
    internal static class LtToolTips
    {
        // Tooltip variables
        private static Rect _controlRect;
        private static Vector2 _toolTipPos;
        private static string _toolTip;
        private static Rect _position;
        private static float _xOffset;

        internal static void ShowToolTips()
        {
            if (!string.IsNullOrEmpty(_toolTip))
                ShowToolTip(_toolTipPos, _toolTip);

            // Obtain the new value from the last repaint
            _toolTip = GetCurrentToolTip();
        }

        private static void ShowToolTip(Vector2 toolTipPos, string toolTip)
        {
            if (LtSettings.showToolTips && (toolTip != null) && (toolTip.Trim().Length > 0))
            {
                Vector2 size = LtStyle.ToolTipStyle.CalcSize(new GUIContent(toolTip));
                _position = new Rect(toolTipPos.x, toolTipPos.y, size.x, size.y);
                RepositionToolTip();
                GUI.Window(0, _position, EmptyWindow, toolTip, LtStyle.ToolTipStyle);
                GUI.BringWindowToFront(0);
            }
        }

        internal static string SetActiveToolTip(Rect control, string toolTip, ref bool toolTipActive, float xOffset)
        {
            // Note: All values are screenpoint based (0, 0 in lower left). This removes confusion with the Gui point of elements (0, 0 in upper left).
            if (!toolTipActive && control.Contains(Event.current.mousePosition))
            {
                toolTipActive = true;

                // Note: At this time controlPosition is in Gui Point system and is local position. Convert to screenpoint.
                Rect newControl = new Rect
                {
                    position = GUIUtility.GUIToScreenPoint(control.position),
                    width = control.width,
                    height = control.height
                };

                // Event.current.mousePosition returns sceen mouseposition. Gui elements return a value in Gui position.
                // Add the height of parent Gui elements already drawn to y offset to get the correct screen position.
                if (control.Contains(Event.current.mousePosition))
                {
                    // Let's use the rectangle as a solid anchor and a stable tooltip, forgiving of mouse movement within bounding box.
                    _toolTipPos = new Vector2(newControl.xMax + xOffset, newControl.y - 10);

                    _controlRect = newControl;
                    _xOffset = xOffset;
                    _controlRect.x += xOffset;
                    _controlRect.y -= 10;
                }
                else
                    toolTip = "";
            }

            // We are in a loop so we don't need the return value from SetUpToolTip. We will assign it instead.
            if (!toolTipActive)
                toolTip = "";

            return toolTip;
        }

        private static string GetCurrentToolTip()
        {
            // Only one of these values can be active at a time (onMouseOver), so this will trap it.
            // (Brute force, but functional)
            string toolTip = "";
            if (!string.IsNullOrEmpty(WindowSettings.toolTip))
                toolTip = WindowSettings.toolTip;

            // Update stored tooltip. We do this here so change can be picked up after the current OnGUI.
            // Tooltip will not display if changes are made during the current OnGUI.
            // (Unity uses OnGUI async callbacks so we need to allow for the callback)
            return toolTip;
        }

        private static void RepositionToolTip()
        {
            if (_position.xMax > Screen.width)
                _position.x = _controlRect.x - _position.width - (_xOffset > 30 ? 30 : _xOffset);

            if (_position.yMax > Screen.height)
                _position.y = Screen.height - _position.height;
        }

        private static void EmptyWindow(int windowId)
        { }
    }
}
