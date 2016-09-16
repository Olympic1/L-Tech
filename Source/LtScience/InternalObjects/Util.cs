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

                values += "CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.IVA = " + (CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.IVA);

                LogMessage(string.Format("Util.CanShowSkyLab (repeating error). Error: {0} \r\n\r\n{1}\r\n\r\nValues: {2}", ex.Message, ex.StackTrace, values), LogType.Error);
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
