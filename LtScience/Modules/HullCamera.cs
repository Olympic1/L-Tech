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
using System.IO;
using KSP.Localization;
using LtScience.Utilities;
using UnityEngine;

using static LtScience.Addon;

namespace LtScience.Modules
{
    internal class HullCamera : ModuleScienceExperiment
    {
        #region Properties

        [KSPField]
        public Vector3 cameraPosition = Vector3.zero;

        [KSPField]
        public Vector3 cameraForward = Vector3.forward;

        [KSPField]
        public Vector3 cameraUp = Vector3.up;

        [KSPField]
        public string cameraTransformName = string.Empty;

        [KSPField(isPersistant = false)]
        public bool usesFilm = false;

        [KSPField]
        public float picScienceVal = 1f;

        [KSPField]
        public float picDataVal = 1f;

        [KSPField]
        public string specialExperimentName = "photo-";

        [KSPField]
        public string specialExperimentTitle = "#Anon# Picture";

        // Camera properties
        private static readonly List<HullCamera> _cameras = new List<HullCamera>();
        private static HullCamera _currentCamera;
        private static HullCamera _currentHandler;
        private double _cameraDistance = double.NaN;
        private readonly float cameraClip = 0.01f;
        private readonly float cameraFoV = 60f;
        private bool ltCamActive;

        // FlightCamera properties
        private static FlightCamera _cam;
        private static Transform _origParent;
        private static Quaternion _origRotation = Quaternion.identity;
        private static Vector3 _origPosition = Vector3.zero;
        private static float _origFoV;
        private static float _origClip;

        // Screenshot properties
        private static readonly string folder = $"{KSPUtil.ApplicationRootPath}Screenshots/LTech/";
        private int _cnt;
        private string _screenshotFile = string.Empty;
        private bool _takingPic;
        private bool _tookPic;
        private float _timer;

        #endregion

        #region Methods

        private static void SaveMainCamera()
        {
            _origParent = _cam.transform.parent;
            _origClip = Camera.main.nearClipPlane;
            _origFoV = Camera.main.fieldOfView;
            _origPosition = _cam.transform.localPosition;
            _origRotation = _cam.transform.localRotation;
        }

        private static void ToMainCamera()
        {
            if (_cam != null)
            {
                _cam.transform.parent = _origParent;
                _cam.transform.localPosition = _origPosition;
                _cam.transform.localRotation = _origRotation;
                _cam.SetFoV(_origFoV);
                _cam.ActivateUpdate();

                if (FlightGlobals.ActiveVessel != null && HighLogic.LoadedSceneIsFlight)
                    _cam.SetTarget(FlightGlobals.ActiveVessel.transform, FlightCamera.TargetMode.Vessel);

                _origParent = null;
            }

            if (_currentCamera != null)
                _currentCamera.ltCamActive = false;

            _currentCamera = null;
            Camera.main.nearClipPlane = _origClip;
        }

        private static void LeaveCamera()
        {
            if (_currentCamera == null)
                return;

            ToMainCamera();
        }

        private void ActivateCamera()
        {
            if (part.State == PartStates.DEAD)
                return;

            if (ltCamActive)
            {
                ToMainCamera();
                return;
            }

            _currentCamera = this;
            ltCamActive = true;
        }

        private void DetectAnomalies()
        {
            Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Hullcam_001"));
            PQSSurfaceObject[] anomalies = vessel.mainBody.pqsSurfaceObjects;

            if (anomalies != null)
            {
                GetClosestAnomaly(anomalies, out double nearest, out PQSSurfaceObject anomaly);

                if (anomaly != null)
                    Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Hullcam_002", anomaly.SurfaceObjectName, nearest));
            }
            else
            {
                Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Hullcam_003"));
            }
        }

        private void GetClosestAnomaly(PQSSurfaceObject[] anomalies, out double nearest, out PQSSurfaceObject anomaly)
        {
            nearest = double.PositiveInfinity;
            anomaly = null;

            foreach (PQSSurfaceObject anom in anomalies)
            {
                double dist = Vector3d.Distance(anom.transform.position, FlightGlobals.ship_position);

                if (dist < nearest)
                {
                    nearest = dist;
                    anomaly = anom;
                }
            }
        }

        private void DoExperiment()
        {
            string expId = "Space";
            string expName = "Picture";
            float expValue = picScienceVal;
            float expData = picDataVal;

            PQSSurfaceObject[] anomalies = vessel.mainBody.pqsSurfaceObjects;
            GetClosestAnomaly(anomalies, out double nearest, out PQSSurfaceObject anomaly);

            if (anomaly != null && nearest < 2500)
            {
                double targetAngleTo = Vector3d.Dot(part.transform.up + cameraForward, (anomaly.transform.position - FlightGlobals.ship_position).normalized);

                if (targetAngleTo > 1)
                {
                    expId = $"{vessel.mainBody.name}-{anomaly.SurfaceObjectName}";
                    expValue = (float)(Math.Abs(2500.0 - nearest) * (targetAngleTo - 1.0) / 100.0) * picScienceVal;
                    expName = anomaly.SurfaceObjectName;
                    expData = 20f * picDataVal;
                }
            }

            Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Hullcam_004", expName, expValue));

            experiment.id = specialExperimentName + expId;
            experiment.experimentTitle = specialExperimentTitle.Replace("#Anon#", expName);
            experiment.baseValue = expValue;
            experiment.dataScale = expData;
            base.DeployExperiment();
        }

        private void BeginPic()
        {
            if (usesFilm && part.RequestResource("CameraFilm", 1f, ResourceFlowMode.NO_FLOW) < 1)
            {
                Utils.DisplayScreenMsg(Localizer.Format("#autoLOC_LTech_Hullcam_005"));
                return;
            }

            _takingPic = true;
            _tookPic = false;
            _timer = 0;
        }

        private void TakeScreenshot()
        {
            int resolution = (int)Settings.resolution;
            resolution = (int)Math.Floor((decimal)resolution);

            // Prevent smaller then 1 resolutions to be taken
            if (resolution < 1)
                resolution = 1;

            if (_tookPic && Settings.hideUiOnScreenshot && File.Exists(_screenshotFile))
                GameEvents.onShowUI.Fire();

            // Check if the folder exists
            if (!Directory.Exists(folder))
            {
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception ex)
                {
                    Log.Error($"HullCamera.TakeScreenshot. Error: {ex}");
                    return;
                }
            }

            string pngName;

            // Check if the file exists
            do
            {
                _cnt++;
                string sName = $"Screenshot_{_cnt}";
                pngName = $"{Path.GetFullPath(folder)}{sName}.png";
            } while (File.Exists(pngName));

            if (Settings.hideUiOnScreenshot)
                GameEvents.onHideUI.Fire();

            _tookPic = true;
            _screenshotFile = pngName;
            //Application.CaptureScreenshot(pngName, resolution);
            ScreenCapture.CaptureScreenshot(pngName);
        }

        #endregion

        #region KSP Events

        [KSPEvent(guiActive = true, guiName = "#autoLOC_LTech_Hullcam_006")]
        public void ActivateCameraEvent()
        {
            ActivateCamera();
        }

        [KSPEvent(guiActive = false, guiName = "#autoLOC_LTech_Hullcam_007")]
        public new void DeployExperiment()
        {
            BeginPic();
        }

        [KSPEvent(guiActive = true, guiName = "#autoLOC_LTech_Hullcam_008")]
        public void DetectAnomaliesEvent()
        {
            DetectAnomalies();
        }

        #endregion

        #region KSP Actions

        [KSPAction("#autoLOC_LTech_Hullcam_006")]
        public void ActivateCameraAction(KSPActionParam param)
        {
            ActivateCamera();
        }

        [KSPAction("#autoLOC_LTech_Hullcam_007")]
        public new void DeployAction(KSPActionParam param)
        {
            BeginPic();
        }

        [KSPAction("#autoLOC_LTech_Hullcam_008")]
        public void DetectAnomaliesAction(KSPActionParam param)
        {
            DetectAnomalies();
        }

        #endregion

        #region Event Handlers

        public void LateUpdate()
        {
            if (vessel == null)
                return;

            if (_currentHandler == null)
                _currentHandler = this;

            if (_currentCamera != null)
            {
                if (_currentCamera.vessel != FlightGlobals.ActiveVessel)
                {
                    Vector3d vesselPos = FlightGlobals.ActiveVessel.orbit.getRelativePositionAtUT(Planetarium.GetUniversalTime()) + FlightGlobals.ActiveVessel.orbit.referenceBody.position;
                    Vector3d targetPos = _currentCamera.vessel.orbit.getRelativePositionAtUT(Planetarium.GetUniversalTime()) + _currentCamera.vessel.orbit.referenceBody.position;

                    _cameraDistance = Vector3d.Distance(vesselPos, targetPos);

                    if (_cameraDistance >= 2480.0)
                        LeaveCamera();
                }
            }

            if (!_takingPic && _tookPic && Settings.hideUiOnScreenshot)
                GameEvents.onShowUI.Fire();

            if (_takingPic)
            {
                _timer += Time.deltaTime;

                if (_timer > 0.1f && !_tookPic)
                    TakeScreenshot();

                if (_timer > Settings.shuttertime)
                {
                    ToMainCamera();
                    DoExperiment();
                    _takingPic = false;
                }
            }
        }

        public void FixedUpdate()
        {
            if (vessel == null || MapView.MapIsEnabled)
                return;

            if (part.State == PartStates.DEAD)
            {
                if (ltCamActive)
                    LeaveCamera();

                Events["ActivateCameraEvent"].guiActive = false;
                Events["DeployExperiment"].guiActive = false;
                Events["DetectAnomaliesEvent"].guiActive = false;
                ltCamActive = false;
                CleanUp();
                return;
            }

            Events["ActivateCameraEvent"].guiName = ltCamActive ? Localizer.Format("#autoLOC_LTech_Hullcam_009") : Localizer.Format("#autoLOC_LTech_Hullcam_006");
            Events["DeployExperiment"].guiActive = ltCamActive;

            if (!ltCamActive)
                return;

            if (_cam == null)
            {
                _cam = FlightCamera.fetch;

                // Just a safety check
                if (_cam == null)
                    return;
            }

            if (_origParent == null)
                SaveMainCamera();

            _cam.SetTargetNone();
            _cam.transform.parent = cameraTransformName.Length > 0 ? part.FindModelTransform(cameraTransformName) : part.transform;
            _cam.DeactivateUpdate();
            _cam.transform.localPosition = cameraPosition;
            _cam.transform.localRotation = Quaternion.LookRotation(cameraForward, cameraUp);
            _cam.SetFoV(cameraFoV);
            Camera.main.nearClipPlane = cameraClip;

            OnFixedUpdate();
        }

        public override void OnStart(StartState state)
        {
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);

            if (state != StartState.None && state != StartState.Editor)
            {
                if (!_cameras.Contains(this))
                    _cameras.Add(this);

                vessel.OnJustAboutToBeDestroyed += CleanUp;
            }

            part.OnJustAboutToBeDestroyed += CleanUp;
            part.OnEditorDestroy += CleanUp;

            if (part.State == PartStates.DEAD)
            {
                Events["ActivateCameraEvent"].guiActive = false;
                Events["DeployExperiment"].guiActive = false;
                Events["DetectAnomaliesEvent"].guiActive = false;
            }

            base.OnStart(state);
        }

        private void OnGameSceneLoadRequested(GameScenes gameScene)
        {
            if (_currentCamera != null)
            {
                _cameras.Clear();
                _currentCamera = null;
            }
        }

        private void CleanUp()
        {
            if (_currentHandler == this)
                _currentHandler = null;

            if (_currentCamera == this)
                LeaveCamera();

            if (_cameras.Contains(this))
            {
                _cameras.Remove(this);

                if (_cameras.Count < 1 && _origParent != null && !ltCamActive)
                {
                    _currentCamera = null;
                    ToMainCamera();
                }
            }
        }

        public void OnVesselDestroy()
        {
            if (_cameras.Contains(this))
            {
                _cameras.Remove(this);
                LeaveCamera();
            }
        }

        public new void OnDestroy()
        {
            CleanUp();
        }

        public override string GetInfo()
        {
            return usesFilm ? Localizer.Format("#autoLOC_LTech_Hullcam_010") : Localizer.Format("#autoLOC_LTech_Hullcam_011");
        }

        #endregion
    }
}
