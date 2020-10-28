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

using System;
using KSP.Localization;
using KSP.UI.Dialogs;
using LtScience.Windows;
using UnityEngine;

using static LtScience.Addon;

namespace LtScience.Utilities
{
    internal static class Utils
    {
        #region Messages

        internal static void DisplayScreenMsg(string msg)
        {
            ScreenMessage sMsg = new ScreenMessage(msg, 6f, ScreenMessageStyle.UPPER_CENTER);
            ScreenMessages.PostScreenMessage(sMsg);
        }

#if false
        internal static void LogMessage(string msg, LogType type)
        {
            try
            {
                string sMsg = $"[LTech] - {type}: {msg}";

                if (type == LogType.Info)
                    Debug.Log(sMsg);

                if (type == LogType.Warning)
                    Debug.LogWarning(sMsg);

                if (type == LogType.Error)
                    Debug.LogError(sMsg);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LTech] - Error: Could not log a message. Error: {ex}");
            }
        }

        internal enum LogType
        {
            Info,
            Warning,
            Error
        }
#endif
#endregion

#region Condition methods

#if false
        internal static bool CanShowSkylab()
        {
            try
            {
                bool canShow = false;
                if (Addon.ShowUi &&
                    HighLogic.LoadedSceneIsFlight &&
                    !IsPauseMenuOpen() &&
                    !IsFlightDialogDisplaying() &&
                    FlightGlobals.fetch != null &&
                    FlightGlobals.ActiveVessel != null &&
                    !FlightGlobals.ActiveVessel.isEVA &&
                    FlightGlobals.ActiveVessel.vesselType != VesselType.Flag
                )
                    canShow = true; // WindowSkylab.showWindow;

                return canShow;
            }
            catch (Exception ex)
            {
                if (!Addon.FrameErrTripped)
                {
                    string values = $"Addon.ShowUi = {Addon.ShowUi}\r\n";
                    values += $"HighLogic.LoadedScene = {HighLogic.LoadedScene}\r\n";
                    values += $"PauseMenu.isOpen = {IsPauseMenuOpen()}\r\n";
                    values += $"FlightResultsDialog.isDisplaying = {IsFlightDialogDisplaying()}\r\n";
                    values += $"FlightGlobals.fetch != null = {FlightGlobals.fetch != null}\r\n";
                    values += $"FlightGlobals.ActiveVessel != null = {FlightGlobals.ActiveVessel != null}\r\n";
                    values += $"!FlightGlobals.ActiveVessel.isEVA = {FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.isEVA}\r\n";
                    if (FlightGlobals.ActiveVessel != null)
                        values += $"FlightGlobals.ActiveVessel.vesselType = {FlightGlobals.ActiveVessel.vesselType}\r\n";

                    Log.Error($"Utils.CanShowSkylab (repeating error). Error: {ex.Message}\r\n\r\n{ex.StackTrace}\r\n\r\nValues: {values}");
                    Addon.FrameErrTripped = true;
                }

                return false;
            }
        }
#endif

        internal static bool IsPauseMenuOpen()
        {
            try
            {
                return PauseMenu.isOpen;
            }
            catch
            {
                try
                {
                    return KSCPauseMenu.Instance.isActiveAndEnabled;
                }
                catch
                {
                    return false;
                }
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

        internal static bool CheckBoring(Vessel vessel, bool msg = false)
        {
            if (vessel.orbit.referenceBody == Planetarium.fetch.Home &&
                (vessel.situation == Vessel.Situations.PRELAUNCH ||
                 vessel.situation == Vessel.Situations.LANDED ||
                 vessel.situation == Vessel.Situations.SPLASHED ||
                 vessel.altitude <= vessel.orbit.referenceBody.atmosphereDepth)
            )
            {
                if (msg)
                    DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Util_001"));

                return true;
            }

            return false;
        }

        internal static bool CanRunExperiment(Vessel vessel, string expId, ref string msg)
        {
            msg = Localizer.Format("#autoLOC_LTech_Util_002");

            ScienceExperiment labExp = ResearchAndDevelopment.GetExperiment(expId);
            if (labExp.IsAvailableWhile((ExperimentSituations)vessel.situation, vessel.mainBody))
            {
                if (labExp.BiomeIsRelevantWhile((ExperimentSituations)vessel.situation))
                    return true;
            }

            if (vessel.situation == Vessel.Situations.LANDED)
            {
                if (vessel.situation == Vessel.Situations.LANDED)
                    return true;

                msg += Localizer.Format("#autoLOC_LTech_Util_003");
            }

            if (vessel.situation == Vessel.Situations.SPLASHED)
            {
                if (vessel.situation == Vessel.Situations.SPLASHED)
                    return true;

                msg += Localizer.Format("#autoLOC_LTech_Util_004");
            }

            if (vessel.situation == Vessel.Situations.FLYING)
            {
                if (vessel.altitude <= vessel.orbit.referenceBody.atmosphereDepth)
                    return true;

                msg += Localizer.Format("#autoLOC_LTech_Util_005");
            }

            if (vessel.situation == Vessel.Situations.ORBITING)
            {
                if (vessel.altitude > vessel.orbit.referenceBody.atmosphereDepth)
                    return true;

                msg += Localizer.Format("#autoLOC_LTech_Util_006");
            }

            msg = msg.Remove(msg.Length - 2);
            return false;
        }

#endregion

#region Resources

        internal static PartResourceDefinition GetDefinition(string resourceName)
        {
            return PartResourceLibrary.Instance.GetDefinition(resourceName);
        }

        internal static double ResourceAvailable(this Part part, string resourceName)
        {
            PartResourceDefinition resource = GetDefinition(resourceName);
            part.GetConnectedResourceTotals(resource.id, out double amount, out double maxAmount);
            return amount;
        }

#if true
        internal static float RequestResource(this Part part, string resourceName, double demand)
        {
            PartResourceDefinition resource = GetDefinition(resourceName);
            return (float)part.RequestResource(resource.id, demand);
        }
#endif
#endregion
    }
}
