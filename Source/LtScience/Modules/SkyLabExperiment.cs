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
using System.Linq;
using KSP.Localization;
using KSP.UI.Screens.Flight.Dialogs;
using LtScience.Utilities;
using LtScience.Windows;

namespace LtScience.Modules
{
    internal class SkylabExperiment : PartModule, IScienceDataContainer
    {
        #region Properties

        [KSPField]
        public float labBoostScalar = 1f;

        private readonly List<ScienceData> _storedData = new List<ScienceData>();
        private ExperimentsResultDialog _expDialog;

        #endregion

        #region Event Handlers

        public override void OnAwake()
        {
            GameEvents.onGamePause.Add(OnPause);
            GameEvents.onGameUnpause.Add(OnUnpause);
        }

        public override void OnSave(ConfigNode node)
        {
            foreach (ScienceData data in _storedData)
            {
                data.Save(node.AddNode("ScienceData"));
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            _storedData.Clear();
            if (node.HasNode("ScienceData"))
            {
                foreach (ConfigNode dataNode in node.GetNodes("ScienceData"))
                {
                    _storedData.Add(new ScienceData(dataNode));
                }
            }
        }

        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
                UpdateUI();
        }

        public void OnDestroy()
        {
            GameEvents.onGamePause.Remove(OnPause);
            GameEvents.onGameUnpause.Remove(OnUnpause);
        }

        private void UpdateUI()
        {
            Utils.LogMessage("UpdateUI called", Utils.LogType.Info);
            Events["OpenGui"].active = !WindowSkylab.showWindow;
            Events["EvaCollect"].active = _storedData.Count > 0;
            Events["ReviewDataEvent"].active = _storedData.Count > 0;
        }

        private void OnPause()
        {
            if (_expDialog != null)
                _expDialog.gameObject.SetActive(false);
        }

        private void OnUnpause()
        {
            if (_expDialog != null)
                _expDialog.gameObject.SetActive(true);
        }

        #endregion

        #region KSP Events

        [KSPEvent(active = true, guiActive = true, guiName = "#autoLOC_LTech_Experiment_001")]
        public void OpenGui()
        {
            WindowSkylab.showWindow = true;
        }

        [KSPEvent(active = true, externalToEVAOnly = true, guiActive = false, guiActiveUnfocused = true, guiName = "#autoLOC_6004057", unfocusedRange = 2f)]
        public void EvaCollect()
        {
            List<ModuleScienceContainer> evaCont = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceContainer>();

            if (_storedData.Count > 0)
            {
                if (evaCont.First().StoreData(new List<IScienceDataContainer> { this }, false))
                {
                    foreach (ScienceData data in _storedData)
                        DumpData(data);
                }
            }
        }

        [KSPEvent(active = true, guiActive = true, guiName = "#autoLOC_502204")]
        public void ReviewDataEvent()
        {
            ReviewData();
            UpdateUI();
        }

        #endregion

        #region Science

        internal void DoScience(string expId, double reqInsight, string reqResource, double reqAmount)
        {
            string step = "Start";
            try
            {
                string msg = string.Empty;
                Vessel ves = FlightGlobals.ActiveVessel;
                Part prt = FlightGlobals.ActiveVessel.rootPart;
                ModuleScienceExperiment exp = new ModuleScienceExperiment();

                // Checks
                step = "Check Boring";
                if (Utils.CheckBoring(ves, true))
                    return;

                step = "Check CanRun";
                if (!Utils.CanRunExperiment(ves, ref msg))
                {
                    Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Experiment_002", msg));
                    return;
                }

                step = "Check Insight";
                if (Utils.ResourceAvailable(prt, "Insight") < reqInsight)
                {
                    double current = Utils.ResourceAvailable(prt, "Insight");
                    double needed = reqInsight - current;

                    Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Experiment_003", (int)needed));
                    return;
                }

                step = "Check Resource";
                if (Utils.ResourceAvailable(prt, reqResource) < reqAmount)
                {
                    double current = Utils.ResourceAvailable(prt, reqResource);
                    double needed = reqAmount - current;

                    Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Experiment_004", (int)needed, reqResource));
                    return;
                }

                step = "Take Resources";
                Utils.RequestResource(prt, "Insight", reqInsight);
                Utils.RequestResource(prt, reqResource, reqAmount);

                // Experiment
                step = "Get Experiment";
                exp.experimentID = expId;
                ScienceExperiment labExp = ResearchAndDevelopment.GetExperiment(exp.experimentID);
                if (labExp == null)
                {
                    Utils.LogMessage(Localizer.Format("#autoLOC_LTech_Experiment_005", exp.experimentID), Utils.LogType.Warning);
                    Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Experiment_006"));
                    return;
                }

                step = "Get Situation";
                ExperimentSituations vesselSit = ScienceUtil.GetExperimentSituation(ves);
                if (labExp.IsAvailableWhile(vesselSit, ves.mainBody))
                {
                    step = "Get Biome";
                    string biome, displayBiome;
                    if (ves.landedAt != string.Empty)
                    {
                        biome = Vessel.GetLandedAtString(ves.landedAt);
                        displayBiome = Localizer.Format(ves.displaylandedAt);
                    }
                    else
                    {
                        biome = ScienceUtil.GetExperimentBiome(ves.mainBody, ves.latitude, ves.longitude);
                        displayBiome = ScienceUtil.GetBiomedisplayName(ves.mainBody, biome);
                    }

                    step = "Get Subject";
                    ScienceSubject labSub = ResearchAndDevelopment.GetExperimentSubject(labExp, vesselSit, ves.mainBody, biome, displayBiome);
                    labSub.title = $"{labExp.experimentTitle}";
                    labSub.subjectValue *= labBoostScalar;
                    labSub.scienceCap = labExp.scienceCap * labSub.subjectValue;

                    step = "Calculate Points";
                    float sciencePoints = labExp.baseValue * labExp.dataScale;

                    ScienceData labData = new ScienceData(sciencePoints, exp.xmitDataScalar, 0, labSub.id, labSub.title, false, prt.flightID);

                    step = "Add Experiment";
                    _storedData.Add(labData);

                    step = "Show Dialog";
                    Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_238419", prt.partInfo.title, labData.dataAmount, labSub.title));
                    ReviewDataItem(labData);
                }
                else
                {
                    Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_238424", labExp.experimentTitle));
                    Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Experiment_007", vesselSit.displayDescription()));
                }

                step = "End";
            }
            catch (Exception ex)
            {
                Utils.LogMessage($"SkylabExperiment.DoScience at step \"{step}\";. Error: {ex}", Utils.LogType.Error);
            }
        }

        #endregion

        #region Result Dialog

        private void ShowResultDialog(ScienceData data)
        {
            ScienceLabSearch labSearch = new ScienceLabSearch(FlightGlobals.ActiveVessel, data);

            _expDialog = ExperimentsResultDialog.DisplayResult(new ExperimentResultDialogPage(
                FlightGlobals.ActiveVessel.rootPart,
                data,
                data.baseTransmitValue,
                data.transmitBonus,
                false,
                string.Empty,
                false,
                labSearch,
                OnDiscardData,
                OnKeepData,
                OnTransmitData,
                OnSendToLab));
        }

        public void ReviewData()
        {
            if (_storedData.Count <= 0)
                return;

            foreach (ScienceData data in _storedData)
                ReviewDataItem(data);
        }

        public void ReviewDataItem(ScienceData data)
        {
            ShowResultDialog(data);
        }

        private void OnDiscardData(ScienceData data)
        {
            _expDialog = null;
            DumpData(data);
            UpdateUI();
        }

        private void OnKeepData(ScienceData data)
        {
            _expDialog = null;
            UpdateUI();
        }

        private void OnTransmitData(ScienceData data)
        {
            _expDialog = null;
            IScienceDataTransmitter transmitter = ScienceUtil.GetBestTransmitter(FlightGlobals.ActiveVessel);

            if (transmitter != null)
            {
                transmitter.TransmitData(new List<ScienceData> { data });
                DumpData(data);
            }
            else if (CommNet.CommNetScenario.CommNetEnabled)
            {
                Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_237738"));
            }
            else
            {
                Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_237740"));
            }

            UpdateUI();
        }

        private void OnSendToLab(ScienceData data)
        {
            _expDialog = null;
            ScienceLabSearch labSearch = new ScienceLabSearch(FlightGlobals.ActiveVessel, data);

            if (labSearch.NextLabForDataFound)
            {
                StartCoroutine(labSearch.NextLabForData.ProcessData(data));
                DumpData(data);
            }
            else
            {
                labSearch.PostErrorToScreen();
                UpdateUI();
            }
        }

        #endregion

        #region IScienceDataContainer

        public ScienceData[] GetData()
        {
            return _storedData.ToArray();
        }

        public int GetScienceCount()
        {
            return _storedData.Count;
        }

        public bool IsRerunnable()
        {
            return true;
        }

        public void ReturnData(ScienceData data)
        {
            if (data == null)
                return;

            _storedData.Add(data);
            UpdateUI();
        }

        public void DumpData(ScienceData data)
        {
            _storedData.Remove(data);
            UpdateUI();
        }

        #endregion
    }
}
