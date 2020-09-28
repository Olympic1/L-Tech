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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KSP.Localization;
using KSP.UI.Screens.Flight.Dialogs;
using LtScience.Utilities;
using LtScience.Windows;
using UnityEngine;

using static LtScience.Addon;

namespace LtScience.Modules
{
    // science is done at the resourceUsageRate per (???minute/hour/day????)
    // When the usedResource >= reqAmount,the experiment is complete

    // speed & efficiency of use based on kerbal's time in space
    // Need to come up with decent formula
    //static double NthRoot(double A, int N)
    //{
    //    return Math.Pow(A, 1.0 / N);
    //}

    public class Experiment
    {
        internal string name;
        internal string label;
        internal string tooltip;
        internal string neededResourceName;
        internal double resourceAmtRequired;
        internal double resourceUsageRate;
        internal uint biomeMask;
        internal uint situationMask;

        public Experiment(string name, string label, string tooltip, string neededResourceName, float resourceAmtRequired, float resourceUsageRate)
        {
            Init();
            this.name = name;
            this.label = label;
            this.tooltip = tooltip;
            this.neededResourceName = neededResourceName;
            this.resourceAmtRequired = resourceAmtRequired;
            this.resourceUsageRate = resourceUsageRate;
            this.biomeMask = 0;
            this.situationMask = 0;
        }

        protected internal Experiment()
        {
            Init();
        }
        void Init()
        {
            name = "";
            label = "";
            tooltip = "";
            neededResourceName = "";
        }
        internal Experiment(string id)
        {
            Init();
            name = id;
        }
        public Experiment Load(string id, ConfigNode node)
        {
            var experiment = new Experiment(id);

            node.TryGetValue("label", ref experiment.label);
            node.TryGetValue("tooltip", ref experiment.tooltip);
            node.TryGetValue("resourceUsed", ref experiment.neededResourceName);
            node.TryGetValue("resourceAmtRequired", ref experiment.resourceAmtRequired);
            node.TryGetValue("resourceUsageRate", ref experiment.resourceUsageRate);

            Log.Info("Experiment.Load, id: " + experiment.name +
                ", label: " + experiment.label +
                ", tooltip: " + experiment.tooltip +
                ", neededResourceName: " + experiment.neededResourceName +
                ", resourceAmtRequired: " + experiment.resourceAmtRequired +
                ", resourceUsageRate: " + resourceUsageRate);
            return experiment;
        }
    }

    internal class ExpStatus
    {
        internal const string EXPERIMENT_STATUS = "ExpStatus";

        internal string expId;
        internal string key;
        internal string bodyName;
        internal ExperimentSituations vesselSit;
        internal string biome;
        internal double processedResource;
        internal string reqResource;
        internal double reqAmount;
        internal bool active;
        internal double lastTimeUpdated;


        public ExpStatus(string expId, string key, string bodyName, ExperimentSituations vesselSit, string biome, string reqResource, double reqAmount)
        {
            this.expId = expId;
            this.key = key;
            this.bodyName = bodyName;
            this.vesselSit = vesselSit;
            this.processedResource = 0;
            this.biome = biome;
            this.reqResource = reqResource;
            this.reqAmount = reqAmount;
            active = false;
            lastTimeUpdated = 0;
        }

        protected internal ExpStatus() // internal use only
        {
            expId = "";
            key = "";
            bodyName = "";
            reqResource = "";
            biome = "";
            this.processedResource = 0;
            active = false;
        }
        public string Key
        {
            get
            {
                return key;
            }
        }
        public ExpStatus Load(ConfigNode node, SkylabExperiment instance = null)
        {
            var expStatus = new ExpStatus();

            node.TryGetValue("expId", ref expStatus.expId);
            node.TryGetValue("key", ref expStatus.key);
            node.TryGetValue("bodyName", ref expStatus.bodyName);
            node.TryGetEnum<ExperimentSituations>("vesselSit", ref expStatus.vesselSit, 0);
            node.TryGetValue("biome", ref expStatus.biome);
            node.TryGetValue("processedResource", ref expStatus.processedResource);
            node.TryGetValue("reqResource", ref expStatus.reqResource);
            node.TryGetValue("reqAmount", ref expStatus.reqAmount);
            node.TryGetValue("active", ref expStatus.active);
            node.TryGetValue("lastTimeUpdated", ref expStatus.lastTimeUpdated);
            if (expStatus.active)
            {
                ModuleScienceExperiment exp = new ModuleScienceExperiment();
                exp.experimentID = expStatus.expId;
                if (instance != null)
                    instance.SetUpActiveExperiment(expStatus.expId, expStatus.biome, exp, expStatus.reqResource);
            }
            Log.Info("ExpStatus.Load, expId: " + expStatus.expId + ", key: " + expStatus.key + ", bodyName: " + expStatus.bodyName +
                ", vesselSit: " + expStatus.vesselSit + ", biome: " + expStatus.biome + ", processedResource: " + expStatus.processedResource +
                ", reqAmount: " + expStatus.reqAmount + ", active: " + expStatus.active);
            return expStatus;
        }

        public void Save(ConfigNode node)
        {
            node.AddValue("expId", expId);
            node.AddValue("key", key);
            node.AddValue("bodyName", bodyName);
            node.AddValue("vesselSit", vesselSit);
            node.AddValue("biome", biome);
            node.AddValue("processedResource", processedResource);
            node.AddValue("reqResource", reqResource);
            node.AddValue("reqAmount", reqAmount);
            node.AddValue("active", active);
            node.AddValue("lastTimeUpdated", lastTimeUpdated);
        }
    }


    internal class ActiveExperiment
    {
        internal string activeExpid;
        internal string bodyName;
        internal ExperimentSituations expSit;
        internal string biomeSit;
        internal ModuleScienceExperiment mse;
        internal bool completed;
        internal ActiveExperiment(string expId, string bodyName, ExperimentSituations sit, string biomeSit, ModuleScienceExperiment mse)
        {
            Init(expId, bodyName, sit, biomeSit, mse);
        }
        internal ActiveExperiment(string expId, string bodyName, ExperimentSituations sit, string biomeSit)
        {
            Init(expId, bodyName, sit, biomeSit, null);
        }
        void Init(string expId, string bodyName, ExperimentSituations sit, string biomeSit, ModuleScienceExperiment mse)
        {
            this.activeExpid = expId;
            this.bodyName = bodyName;
            this.expSit = sit;
            this.biomeSit = biomeSit;
            this.mse = mse;
            completed = false;
        }

        public string KeyUnpacked(string expid)
        {
            string expSit = this.expSit.ToString();
            string biomeSit = this.biomeSit;

            uint e_sit = Addon.experiments[expid].situationMask;
            uint e_biome = Addon.experiments[expid].biomeMask;


            if (e_sit > 0)
            {
                if ((e_sit & (uint)this.expSit) == 0)
                {
                    return null;
                }
            }
            else
                expSit = "";

            if (e_biome > 0)
            {
                if ((e_biome & (uint)this.expSit) == 0)
                {
                    return null;
                }
            }
            else
                biomeSit = "";


            return expid + ":" + bodyName + ":" + expSit.ToString() + ":" + biomeSit;

        }
        public string Key
        {
            get
            {
                return KeyUnpacked(activeExpid);
            }
        }

#if false
            public static bool operator ==(ActiveExperiment lhs, ActiveExperiment rhs)
            {
                if ((object)lhs == null || (object)rhs == null)
                    return false;
                return (lhs.activeExpid == rhs.activeExpid &&
                    lhs.bodyName == rhs.bodyName &&
                    lhs.expSit == rhs.expSit &&
                    lhs.biomeSit == rhs.biomeSit);
            }
            public static bool operator !=(ActiveExperiment lhs, ActiveExperiment rhs)
            {
                if ((object)lhs == null || (object)rhs == null)
                    return true;
                return (lhs.activeExpid != rhs.activeExpid ||
                    lhs.bodyName != rhs.bodyName ||
                    lhs.expSit != rhs.expSit ||
                    lhs.biomeSit != rhs.biomeSit);
            }
#endif
    }

    public class SkylabExperiment : PartModule, IScienceDataContainer
    {

        internal class Body
        {
            internal CelestialBody _celestialBody;
            public Body(CelestialBody Body)
            {
                _celestialBody = Body;
            }
        }

        internal Dictionary<CelestialBody, Body> _bodyList = new Dictionary<CelestialBody, Body>();



        const string SCIENCE_DATA = "ScienceData";
        const string EXPERIMENT = "EXPERIMENT";

        internal WindowSkylab windowSkylab = null;
        internal ActiveExperiment activeExperiment = null;
        internal static bool experimentStarted = false;

        SkylabCore skylabcoreModule = null;

        #region Properties

        [KSPField]
        public float labBoostScalar = 1f;

        //[KSPField]
        //public float resourceUsageRate = 1f;

        private readonly List<ScienceData> _storedData = new List<ScienceData>();
        internal Dictionary<string, ExpStatus> expStatuses = new Dictionary<string, ExpStatus>();
        private ExperimentsResultDialog _expDialog;

        #endregion

        #region Event Handlers

        public new void Awake()
        {
            base.Awake();
            GameEvents.onGamePause.Add(OnPause);
            GameEvents.onGameUnpause.Add(OnUnpause);
        }

        public override void OnSave(ConfigNode node)
        {
            foreach (ScienceData data in _storedData)
            {
                data.Save(node.AddNode(SCIENCE_DATA));
            }
            foreach (var data in expStatuses.Values)
            {
                data.Save(node.AddNode(ExpStatus.EXPERIMENT_STATUS));
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            _storedData.Clear();
            expStatuses.Clear();

            if (node.HasNode(SCIENCE_DATA))
            {
                foreach (ConfigNode dataNode in node.GetNodes(SCIENCE_DATA))
                {
                    _storedData.Add(new ScienceData(dataNode));
                }
            }


            if (node.HasNode(ExpStatus.EXPERIMENT_STATUS))
            {
                foreach (ConfigNode dataNode in node.GetNodes(ExpStatus.EXPERIMENT_STATUS))
                {
                    var data = new ExpStatus().Load(dataNode, this);
                    expStatuses.Add(data.Key, data);
                }
            }


            Log.Info("SkylabExperiments, found: " + experiments.Count);
        }

        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                Log.Info("SkylabExperiment.OnStart");
                skylabcoreModule = part.FindModuleImplementing<SkylabCore>();
                StartCoroutine("SlowUpdate");
            }
        }
        IEnumerator SlowUpdate()
        {
            while (true)
            {
                UpdateUI();
                yield return new WaitForSecondsRealtime(0.25f);
            }
        }

        public void OnDestroy()
        {
            Log.Info("SkylabExperiment.OnDestroy");
            StopCoroutine("SlowUpdate");
            GameEvents.onGamePause.Remove(OnPause);
            GameEvents.onGameUnpause.Remove(OnUnpause);
        }

        private void UpdateUI()
        {
            //Events["OpenGui"].active = true;
            Events["EvaCollect"].active = _storedData.Count > 0;
            Events["ReviewDataEvent"].active = _storedData.Count > 0;

            Events["OpenGui"].guiActive =  
                (skylabcoreModule != null && 
                 part.protoModuleCrew.Count >= skylabcoreModule.minimumCrew &&
                 !Utils.CheckBoring(vessel) &&
                 skylabcoreModule.GetCrewScientistTotals() >= skylabcoreModule.minCrewScienceExp) ;
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


        [KSPEvent(active = true, guiActive = true, guiName = "#autoLOC_LTech_Experiment_001")] // Open experiment GUI
        public void OpenGui()
        {
            ToggleGUI();
        }

        internal void ToggleGUI()
        {
            if (windowSkylab == null)
            {
                windowSkylab = gameObject.AddComponent<WindowSkylab>();
                windowSkylab.LabExp = this;
                Events["OpenGui"].guiName = "#autoLOC_LTech_Experiment_008";
            }
            else
            {
                Events["OpenGui"].guiName = "#autoLOC_LTech_Experiment_001";
                Destroy(windowSkylab);
                windowSkylab = null;

            }
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
            //UpdateUI();
        }

        #endregion
        internal static int GetResourceID(string resourceName)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(resourceName);
            return resource.id;
        }

        #region UnityStuff
        Double lastUpdateTime = 0;
        IEnumerator FixedUpdate2()
        {
            while (activeExperiment != null)
            {
                yield return new WaitForSecondsRealtime(1);
                Do_SlowUpdate();
            }
        }
        public void Do_SlowUpdate()
        {
            if ((object)activeExperiment != null)
            {
                var curTime = Planetarium.GetUniversalTime();
                var delta = curTime - lastUpdateTime;

                // Tasks
                // 1. Make sure experiment situation hasn't changed, if it has, then return
                // 2. calcualte resource usage
                // 3. Check to see if exeriment is completed, if so, set a flag

                string biome, displayBiome;
                if (vessel.landedAt != string.Empty)
                {
                    biome = Vessel.GetLandedAtString(vessel.landedAt);
                    displayBiome = Localizer.Format(vessel.displaylandedAt);
                }
                else
                {
                    biome = ScienceUtil.GetExperimentBiome(vessel.mainBody, vessel.latitude, vessel.longitude);
                    displayBiome = ScienceUtil.GetBiomedisplayName(vessel.mainBody, biome);
                }
                var curExp = new ActiveExperiment(activeExperiment.activeExpid, vessel.mainBody.bodyName, ScienceUtil.GetExperimentSituation(vessel), biome);

                if ((object)curExp != null && curExp.Key == activeExperiment.Key)
                {
                    if (!expStatuses.ContainsKey(activeExperiment.Key))
                    {
                        Log.Info("Key missing from expStatuses, key: " + activeExperiment.Key);
                        foreach (var e in expStatuses.Keys)
                            Log.Info("key: " + e);
                    }
                    double resourceRequest = delta / Planetarium.fetch.fixedDeltaTime;

                    double amtNeeded = Math.Min(
                        experiments[activeExperiment.activeExpid].resourceUsageRate * resourceRequest,
                         experiments[activeExperiment.activeExpid].resourceAmtRequired - expStatuses[activeExperiment.Key].processedResource);
                    amtNeeded = amtNeeded * KCT_Interface.ResearchTimeAdjustment();

                    double resource = part.RequestResource(experiments[activeExperiment.activeExpid].neededResourceName, amtNeeded);
                    expStatuses[activeExperiment.Key].processedResource += resource;


                    int resourceID = GetResourceID(expStatuses[activeExperiment.Key].reqResource);
                    part.GetConnectedResourceTotals(resourceID, out double amount, out double maxAmount);

                    expStatuses[activeExperiment.Key].lastTimeUpdated = Planetarium.GetUniversalTime();
                    // var experiment = experiments[activeExperiment.activeExpid];
                }
                else
                {
                    Log.Info("Situation changed");
                    Utils.DisplayScreenMsg("Vessel Situation Changed, Experiment Paused");
                } // need to decide what to do if something changed
                lastUpdateTime = curTime;

            }
            else
                if (experimentStarted)
                Log.Info("FixedUpdate, activeExperiment is null");
        }
        #endregion
        #region Science

        internal void DoScience(string expId)
        {
            string step = "Start";

            string reqResource = experiments[expId].neededResourceName;
            float reqAmount = 1;

            try
            {
                string msg = string.Empty;
                //Vessel ves = FlightGlobals.ActiveVessel;
                Part prt = FlightGlobals.ActiveVessel.rootPart;
                ModuleScienceExperiment exp = new ModuleScienceExperiment();

                // Checks
                step = "Check Boring";
                if (Utils.CheckBoring(vessel, true))
                    return;

                step = "Check CanRun";
                if (!Utils.CanRunExperiment(vessel, expId, ref msg))
                {
                    Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Experiment_002", msg));
                    return;
                }

#if false
                step = "Check Insight";
                if (Utils.ResourceAvailable(prt, "Insight") < reqInsight)
                {
                    double current = Utils.ResourceAvailable(prt, "Insight");
                    double needed = reqInsight - current;

                    Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Experiment_003", (int)needed));
                    return;
                }
#endif

                step = "Check Resource";
                if (Utils.ResourceAvailable(prt, reqResource) < reqAmount)
                {
                    double current = Utils.ResourceAvailable(prt, reqResource);
                    double needed = reqAmount - current;

                    Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Experiment_004", (int)needed, reqResource));
                    return;
                }

#if false
                step = "Take Resources";
                Utils.RequestResource(prt, "Insight", reqInsight);
                Utils.RequestResource(prt, reqResource, reqAmount);
#endif
                // Experiment
                step = "Get Experiment";
                exp.experimentID = expId;
                ScienceExperiment labExp = ResearchAndDevelopment.GetExperiment(exp.experimentID);
                if (labExp == null)
                {
                    Log.Warning(Localizer.Format("#autoLOC_LTech_Experiment_005", exp.experimentID));
                    Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Experiment_006"));
                    return;
                }

                step = "Get Situation";
                ExperimentSituations vesselSit = ScienceUtil.GetExperimentSituation(vessel);
                if (labExp.IsAvailableWhile(vesselSit, vessel.mainBody))
                {
                    step = "Get Biome";
                    string biome, displayBiome;
                    if (vessel.landedAt != string.Empty)
                    {
                        biome = Vessel.GetLandedAtString(vessel.landedAt);
                        displayBiome = Localizer.Format(vessel.displaylandedAt);
                    }
                    else
                    {
                        biome = ScienceUtil.GetExperimentBiome(vessel.mainBody, vessel.latitude, vessel.longitude);
                        displayBiome = ScienceUtil.GetBiomedisplayName(vessel.mainBody, biome);
                    }

                    Log.Info("DoScience, expId: " + expId +
                        "body: " + vessel.mainBody.bodyName +
                        ", ScienceUtil.GetExperimentSituation: " + ScienceUtil.GetExperimentSituation(vessel) +
                        ", biome: " + biome);

                    SetUpActiveExperiment(expId, biome, exp, reqResource);
#if false
                    activeExperiment = new ActiveExperiment(expId, vessel.mainBody.bodyName, ScienceUtil.GetExperimentSituation(vessel), biome, exp);

                    // need to add to 
                    //                   internal ExperimentSituations vesselSit;
                    //                    internal string biome;
                    ExpStatus es = new ExpStatus(expId, activeExperiment.Key, vessel.mainBody.bodyName, ScienceUtil.GetExperimentSituation(vessel), biome,
                        reqResource, experiments[expId].resourceAmtRequired);
                    es.active = true;
                    expStatuses.Add(es.Key, es);
                    experimentStarted = true;
                    StartCoroutine(FixedUpdate2());
#endif
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
                Log.Error($"SkylabExperiment.DoScience at step \"{step}\";. Error: {ex}");
            }
        }


        internal void SetUpActiveExperiment(string expId, string biome, ModuleScienceExperiment exp, string reqResource)
        {

            activeExperiment = new ActiveExperiment(expId, vessel.mainBody.bodyName, ScienceUtil.GetExperimentSituation(vessel), biome, exp);

            ExpStatus es = new ExpStatus(expId, activeExperiment.Key, vessel.mainBody.bodyName, ScienceUtil.GetExperimentSituation(vessel), biome,
                reqResource, experiments[expId].resourceAmtRequired);
            es.active = true;
            expStatuses.Add(es.Key, es);
            experimentStarted = true;

            lastUpdateTime = Planetarium.GetUniversalTime();

            StartCoroutine(FixedUpdate2());

        }

        internal void FinalizeExperiment()
        {
            Log.Info("FinalizeExperiment");

            ScienceExperiment labExp = ResearchAndDevelopment.GetExperiment(activeExperiment.activeExpid);


            string displayBiome = "";
            if (vessel.landedAt != string.Empty)
            {
                activeExperiment.biomeSit = Vessel.GetLandedAtString(vessel.landedAt);
                displayBiome = Localizer.Format(vessel.displaylandedAt);
            }
            else
            {
                activeExperiment.biomeSit = ScienceUtil.GetExperimentBiome(vessel.mainBody, vessel.latitude, vessel.longitude);
                displayBiome = ScienceUtil.GetBiomedisplayName(vessel.mainBody, activeExperiment.biomeSit);
            }

            ModuleScienceExperiment exp = activeExperiment.mse;

#if DEBUG
            var step = "Get Subject";
#endif
            ScienceSubject labSub = ResearchAndDevelopment.GetExperimentSubject(labExp, activeExperiment.expSit, vessel.mainBody, activeExperiment.biomeSit, displayBiome);
            //labSub.title = $"{labExp.experimentTitle}";

            labSub.title = ScienceUtil.GenerateScienceSubjectTitle(labExp, activeExperiment.expSit, vessel.mainBody, activeExperiment.biomeSit, displayBiome);

            labSub.subjectValue *= labBoostScalar;
            labSub.scienceCap = labExp.scienceCap * labSub.subjectValue;

#if DEBUG
            step = "Calculate Points";
#endif
            float sciencePoints = labExp.baseValue * labExp.dataScale;

            ScienceData labData = new ScienceData(sciencePoints, exp.xmitDataScalar, 0, labSub.id, labSub.title, false, vessel.rootPart.flightID);

#if DEBUG
            step = "Add Experiment";
#endif
            _storedData.Add(labData);

#if DEBUG
            step = "Show Dialog";
#endif
            Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_238419", vessel.rootPart.partInfo.title, labData.dataAmount, labSub.title));
            ReviewDataItem(labData);

            expStatuses.Remove(activeExperiment.Key);
            activeExperiment = null;

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
            //UpdateUI();
        }

        private void OnKeepData(ScienceData data)
        {
            _expDialog = null;
            //UpdateUI();
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

            //UpdateUI();
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
                //UpdateUI();
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
            //UpdateUI();
        }

        public void DumpData(ScienceData data)
        {
            _storedData.Remove(data);
            //UpdateUI();
        }

        #endregion
    }
}
