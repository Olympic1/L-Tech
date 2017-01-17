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

using KSP.UI.Dialogs;
using LtScience.Windows;
using System;
using System.IO;
using UnityEngine;

namespace LtScience.InternalObjects
{
    internal static class Util
    {
        #region Messages

        internal static void DisplayScreenMsg(string msg)
        {
            ScreenMessage sMsg = new ScreenMessage(msg, 6f, ScreenMessageStyle.UPPER_CENTER);
            ScreenMessages.PostScreenMessage(sMsg);
        }

        internal static void LogMessage(string msg, LogType type)
        {
            try
            {
                if (type == LogType.Info)
                    Debug.LogFormat("[LTech] - {0}: {1}", type, msg);

                if (type == LogType.Warning)
                    Debug.LogWarningFormat("[LTech] - {0}: {1}", type, msg);

                if (type == LogType.Error)
                    Debug.LogErrorFormat("[LTech] - {0}: {1}", type, msg);
            }
            catch (Exception ex)
            {
                Debug.LogError("[LTech] - Error: Could not log a message. Error: " + ex);
            }
        }

        internal enum LogType
        {
            Info,
            Warning,
            Error
        }

        #endregion

        #region Conversions

        internal static void ConvertToJpg(string origFile, string newFile, int quality)
        {
            Texture2D png = new Texture2D(1, 1);

            byte[] pngData = File.ReadAllBytes(origFile);
            png.LoadImage(pngData);

            byte[] jpgData = png.EncodeToJPG(quality);
            FileStream file = File.Open(newFile, FileMode.Create);
            BinaryWriter binary = new BinaryWriter(file);

            binary.Write(jpgData);
            file.Close();
            UnityEngine.Object.Destroy(png);
        }

        #endregion

        #region Condition Methods

        internal static bool CanShowSkyLab()
        {
            try
            {
                bool canShow = false;
                if (LtAddon.ShowUi
                    && HighLogic.LoadedSceneIsFlight
                    && !IsPauseMenuOpen()
                    && !IsFlightDialogDisplaying()
                    && FlightGlobals.fetch != null
                    && FlightGlobals.ActiveVessel != null
                    && !FlightGlobals.ActiveVessel.isEVA
                    && FlightGlobals.ActiveVessel.vesselType != VesselType.Flag
                    )
                    canShow = WindowSkyLab.showWindow;

                return canShow;
            }
            catch (Exception ex)
            {
                string values = "LTAddon.ShowUi = " + LtAddon.ShowUi + "\r\n";
                values += "HighLogic.LoadedScene = " + HighLogic.LoadedScene + "\r\n";
                values += "PauseMenu.isOpen = " + IsPauseMenuOpen() + "\r\n";
                values += "FlightResultsDialog.isDisplaying = " + IsFlightDialogDisplaying() + "\r\n";
                values += "FlightGlobals.fetch != null = " + (FlightGlobals.fetch != null) + "\r\n";
                values += "FlightGlobals.ActiveVessel != null = " + (FlightGlobals.ActiveVessel != null) + "\r\n";
                values += "!FlightGlobals.ActiveVessel.isEVA = " + (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.isEVA) + "\r\n";
                if (FlightGlobals.ActiveVessel != null)
                    values += "FlightGlobals.ActiveVessel.vesselType = " + FlightGlobals.ActiveVessel.vesselType + "\r\n";

                LogMessage($"Util.CanShowSkyLab (repeating error). Error: {ex.Message} \r\n\r\n{ex.StackTrace}\r\n\r\nValues: {values}", LogType.Error);
                return false;
            }
        }

        private static bool IsPauseMenuOpen()
        {
            try
            {
                return PauseMenu.isOpen;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsFlightDialogDisplaying()
        {
            try
            {
                return FlightResultsDialog.isDisplaying;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
