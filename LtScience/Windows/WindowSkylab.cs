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
using System.Collections.Generic;
using System.Collections;
using System.Linq.Expressions;
using KSP.Localization;
using LtScience.Modules;
using LtScience.Utilities;
using UnityEngine;
using ClickThroughFix;
using SpaceTuxUtility;

using static LtScience.Addon;

namespace LtScience.Windows
{
    internal class WindowSkylab : MonoBehaviour
    {
#if false
        internal class WinPos
        {
            internal int id;
            internal Rect position;
        }
        internal static  List<WinPos>
#endif
        internal Rect position = new Rect(20, 60, 0, 0);

        // GUI tooltip and label support
        private string _label = string.Empty;
        private string _tooltip = string.Empty;
        private GUIContent _guiLabel;

        private const int height = 25;

        private SkylabExperiment _labExp;

        int winID;

        internal SkylabExperiment LabExp { get { return _labExp; } set { _labExp = value; } }

        bool visible = true;

        static public bool? exitWarpWhenDone;

        void Awake()
        {
            if (exitWarpWhenDone == null)
                exitWarpWhenDone = HighLogic.CurrentGame.Parameters.CustomParams<LTech_1>().exitWarpWhenDone;

        }
        void Start()
        {
            GameEvents.onHideUI.Add(OnHideUI);
            GameEvents.onShowUI.Add(OnShowUI);
            GameEvents.onGamePause.Add(OnHideUI);
            GameEvents.onGameUnpause.Add(OnShowUI);
            winID = WindowHelper.NextWindowId("SkylabExperiment");

            //if (exitWarpWhenDone == null)
            //    exitWarpWhenDone = HighLogic.CurrentGame.Parameters.CustomParams<LTech_1>().exitWarpWhenDone;
        }
        void OnDestroy()
        {
            GameEvents.onHideUI.Remove(OnHideUI);
            GameEvents.onShowUI.Remove(OnShowUI);
            GameEvents.onGamePause.Remove(OnHideUI);
            GameEvents.onGameUnpause.Remove(OnShowUI);
        }

        private void OnHideUI() { visible = false; }

        private void OnShowUI() { visible = true; }

        void OnGUI()
        {
            if (visible)
            {
                if (!HighLogic.CurrentGame.Parameters.CustomParams<LTech_1>().useAltSkin)
                    GUI.skin = HighLogic.Skin;
                position = ClickThruBlocker.GUILayoutWindow(winID, this.position, Display, Localizer.Format("#autoLOC_LTech_Skylab_Title"), GUILayout.MinHeight(20));
            }
        }
        internal void Display(int windowId)
        {
            try
            {
                Rect rect = new Rect(position.width - 20, 4, 16, 16);
                _label = "x";
                _tooltip = Localizer.Format("#autoLOC_LTech_Skylab_tt_001");
                _guiLabel = new GUIContent(_label, _tooltip);
                if (GUI.Button(rect, _guiLabel))
                {
                    _labExp.windowSkylab = null;
                    Destroy(this);
                }

                GUILayout.BeginVertical();

                StartExperiment();

                DisplayActions();

                GUILayout.EndVertical();

                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
                Addon.RepositionWindow(ref position);
            }
            catch (Exception ex)
            {
                if (!Addon.FrameErrTripped)
                {
                    Log.Error($"WindowSkylab.Display. Error: {ex.Message}\r\n\r\n{ex.StackTrace}");
                    Addon.FrameErrTripped = true;
                }
            }
        }

        bool clickToDel = false;
        double clicktoDelTime = 0;

        private void StartExperiment()
        {
            if (clickToDel && Planetarium.GetUniversalTime() - clicktoDelTime > 5)
                clickToDel = false;
            try
            {
                ExperimentSituations vesselSit = ScienceUtil.GetExperimentSituation(FlightGlobals.ActiveVessel);
                string result = "";
                switch (vesselSit)
                {
                    case ExperimentSituations.InSpaceHigh:
                        result = Localizer.Format("In space high over <<1>>", FlightGlobals.ActiveVessel.mainBody.displayName);
                        break;
                    case ExperimentSituations.InSpaceLow:
                        result = Localizer.Format("In space near <<1>>", FlightGlobals.ActiveVessel.mainBody.displayName);
                        break;
                }
                GUILayout.Label("Situation: " + result);
                exitWarpWhenDone = GUILayout.Toggle((bool)exitWarpWhenDone, "Stop Warp when completed");

                GUILayout.Space(height);
                foreach (Experiment e in Addon.experiments.Values)
                {
                    double percent = 0;
                    if (_labExp.activeExperiment != null)
                    {
                        if (_labExp.activeExperiment.Key != null &&
                            _labExp.expStatuses.ContainsKey(_labExp.activeExperiment.Key) &&
                            _labExp.expStatuses[_labExp.activeExperiment.Key].active &&
                            _labExp.expStatuses[_labExp.activeExperiment.Key].expId == e.name)
                        {

                            percent = 0.001 + _labExp.expStatuses[_labExp.activeExperiment.Key].processedResource / Addon.experiments[_labExp.activeExperiment.activeExpid].resourceAmtRequired * 100;
                            if (percent < 100f)
                            {
                                if (clickToDel)
                                    _guiLabel = new GUIContent("Click to Cancel: " + e.label + " - " + percent.ToString("F2") + " % completed", e.tooltip);
                                else
                                    _guiLabel = new GUIContent(e.label + " - " + percent.ToString("F2") + " % completed", e.tooltip);
                            }
                            else
                            {
                                _guiLabel = new GUIContent(e.label + " - Completed, Click to Finalize", e.tooltip);
                            }
                            GUI.enabled = true;
                        }
                        else
                        {
                            _guiLabel = new GUIContent(e.label, e.tooltip);
                            GUI.enabled = false;
                        }
                    }
                    else
                    {
                        ScienceExperiment experiment = ResearchAndDevelopment.GetExperiment(e.name);
                        string title = ScienceUtil.GenerateScienceSubjectTitle(experiment, vesselSit, FlightGlobals.ActiveVessel.mainBody);
                        _guiLabel = new GUIContent(e.label, e.tooltip);

                        //var key1 = e.name + "@" + FlightGlobals.ActiveVessel.mainBody.name + vesselSit.ToString();

                        //var m = ResearchAndDevelopment.GetResults(key1);
                        //if (m == null) m = "(null)";
                        //Log.Info("Experiment.Key: " + key1 + ",  result: " + m);

                        ScienceSubject subject = ResearchAndDevelopment.GetExperimentSubject(experiment, vesselSit,
                             FlightGlobals.ActiveVessel.mainBody, null, null);
#if false

                                    float maxScience, availScience;
                        Debug.Log("WindowSkylab, subject: " + subject.id);
                        Debug.Log("key1: " + key1);
                        Debug.Log("WindowSkylab, checkExperiment: " + 
                            checkExperiment(e.name, experiment.id, vesselSit, FlightGlobals.ActiveVessel.mainBody, "", out maxScience, out availScience));
                        Debug.Log("maxScience: " + maxScience + ", availScience: " + availScience);
#endif

                    }
                    if (GUILayout.Button(_guiLabel, GUILayout.Height(height)))
                    {
                        if (percent == 0)
                            _labExp.DoScience(e.name);
                        else
                        {
                            if (percent < 100)
                            {
                                if (clickToDel)
                                {
                                    _labExp.expStatuses.Remove(_labExp.activeExperiment.Key);
                                    _labExp.activeExperiment = null;
                                    clickToDel = false;
                                }
                                else
                                {
                                    clickToDel = true;
                                    clicktoDelTime = Planetarium.GetUniversalTime();
                                }
                            }
                            else
                                _labExp.FinalizeExperiment();
                        }
                    }
                    GUI.enabled = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"WindowSkylab.DisplayExperiments. Error: {ex.Message}\r\n\r\n{ex.StackTrace}");
            }
        }

#region ExperimentTracker
        public bool checkExperiment(string expName, string experimentID, ExperimentSituations expSituation, CelestialBody lastBody, string curBiome,
            out float maxScience, out float availScience)
        {
            ScienceExperiment sciexp = ResearchAndDevelopment.GetExperiment(experimentID);
            ScienceSubject sub = ResearchAndDevelopment.GetExperimentSubject(sciexp, expSituation, lastBody, curBiome, curBiome);
            float dmscival = ResearchAndDevelopment.GetScienceValue(sciexp.dataScale * sciexp.baseValue, sub);

            maxScience = sciexp.dataScale * sciexp.baseValue;
            availScience =  dmscival;


            //var CompletedScience = ScienceSubject.science * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
            var TotalScience = sciexp.scienceCap * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
            maxScience = TotalScience;

            return dmscival > 1f;

        }

#endregion


        private void DisplayActions()
        {
            try
            {
                GUILayout.BeginHorizontal();

                // Close button
                _label = Localizer.Format("#autoLOC_LTech_Skylab_006");
                _tooltip = Localizer.Format("#autoLOC_LTech_Skylab_tt_006");
                _guiLabel = new GUIContent(_label, _tooltip);
                if (GUILayout.Button(_guiLabel, GUILayout.Height(height)))
                {
                    _labExp.ToggleGUI();
                    clickToDel = false;
                    Destroy(this);
                }
                GUILayout.EndHorizontal();
            }
            catch (Exception ex)
            {
                if (!Addon.FrameErrTripped)
                {
                    Log.Error($"WindowSkylab.DisplayActions. Error: {ex.Message}\r\n\r\n{ex.StackTrace}");
                    Addon.FrameErrTripped = true;
                }
            }
        }
    }
}
