/*
 * L-Tech Scientific Industries Continued
 * Copyright © 2015-2016, Arne Peirs (Olympic1)
 * Copyright © 2016, linuxgurugamer
 * 
 * Kerbal Space Program is Copyright © 2011-2016 Squad. See http://kerbalspaceprogram.com/.
 * This project is in no way associated with nor endorsed by Squad.
 * 
 * This file is part of Olympic1's L-Tech (Continued). Original author of L-Tech is 'ludsoe' on the KSP Forums.
 * This file was part of the original L-Tech and was written by ludsoe.
 * Copyright © 2015, ludsoe
 * 
 * Continues to be licensed under the MIT License.
 * See <https://opensource.org/licenses/MIT> for full details.
 */

using LtScience.InternalObjects;
using System;
using System.IO;
using UnityEngine;

namespace LtScience.Modules
{
    public class HullCamera : LtScienceBase
    {
        #region Properties

        [KSPField]
        public Vector3 cameraPosition = Vector3.zero;

        [KSPField]
        public Vector3 cameraForward = Vector3.forward;

        [KSPField]
        public Vector3 cameraUp = Vector3.up;

        [KSPField]
        public string cameraTransformName = "";

        [KSPField]
        public float picScienceVal = 1f;

        [KSPField]
        public float picDataVal = 1f;

        [KSPField]
        public float cameraFoV = 60;

        [KSPField(isPersistant = false)]
        public float cameraClip = 0.01f;

        [KSPField]
        public bool ltCamActive;

        [KSPField(isPersistant = false)]
        public bool usesFilm = false;

        [KSPField]
        public string specialExperimentName;

        [KSPField]
        public string specialExperimentTitle;

        public LtSettings config;
        private static HullCamera _currentCamera;

        private static FlightCamera _cam;
        private static Transform _origParent;
        private static Quaternion _origRotation = Quaternion.identity;
        private static Vector3 _origPosition = Vector3.zero;
        private static float _origFoV;
        private static float _origClip;

        private static readonly string folder = $"{KSPUtil.ApplicationRootPath}Screenshots/LTech/";

        private int _cnt;
        private string _screenshotFile = "";

        private string _jpgName = "";
        private string _pngToConvert = "";

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
            if ((_cam != null) && (_cam.transform != null))
            {
                _cam.transform.parent = _origParent;
                _cam.transform.localPosition = _origPosition;
                _cam.transform.localRotation = _origRotation;
                Camera.main.nearClipPlane = _origClip;
                _cam.SetFoV(_origFoV);
                _cam.ActivateUpdate();

                if (FlightGlobals.ActiveVessel != null && HighLogic.LoadedSceneIsFlight)
                    _cam.SetTargetTransform(FlightGlobals.ActiveVessel.transform);

                _origParent = null;

                if (_currentCamera != null)
                    _currentCamera.ltCamActive = false;

                _currentCamera = null;
            }
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

            ltCamActive = !ltCamActive;

            if (!ltCamActive && (_cam != null))
                ToMainCamera();
            else
            {
                if ((_currentCamera != null) && (_currentCamera != this))
                    _currentCamera.ltCamActive = false;

                _currentCamera = this;
                BeginPic();
            }
        }

        private void DetectAnomaly()
        {
            if (usesFilm && !LtsUseResources("CameraFilm", 1))
            {
                Util.DisplayScreenMsg("Need more Camera Film!");
                return;
            }

            print("Detecting anomalies...");
            PQSCity[] planetAnomalies = vessel.mainBody.GetComponentsInChildren<PQSCity>(true);

            double nearest = double.PositiveInfinity;
            PQSCity anomaly = null;

            // Find the closest anomaly
            foreach (PQSCity anom in planetAnomalies)
            {
                print("Anomaly: " + anom.name);
                double dist = (anom.transform.position - part.transform.position).magnitude;

                if (dist < nearest)
                {
                    nearest = dist;
                    anomaly = anom;
                }
            }

            string expId = "Space";
            string expName = "";
            float expValue = picScienceVal;
            float expData = picDataVal;

            if (anomaly != null)
            {
                print("Closest anomaly is: " + anomaly.name + " and is " + nearest + " m away!");

                if (nearest < 2500)
                {
                    double targetAngleTo = Vector3d.Dot(part.transform.up + cameraForward, (anomaly.transform.position - part.transform.position).normalized);
                    print("Angle: " + targetAngleTo + " degrees");

                    if (targetAngleTo > 1)
                    {
                        expId = vessel.mainBody.name + "-" + anomaly.name;
                        expValue = (float)(Math.Abs(2500 - nearest) * (targetAngleTo - 1) / 100) * picScienceVal;
                        expName = anomaly.name;
                        expData = 20f * picDataVal;
                    }
                }
            }

            print(expName + " is worth: " + expValue);

            experiment.id = specialExperimentName + expId;
            experiment.experimentTitle = specialExperimentTitle.Replace("#Anon#", expName);
            experiment.baseValue = expValue;
            experiment.dataScale = expData;
            DeployExperiment();
        }

        private void BeginPic()
        {
            _takingPic = true;
            _tookPic = false;
            _timer = 0;
        }

        private void TakeScreenshot()
        {
            string pngName;

            int resolution = (int)LtSettings.resolution;
            resolution = (int)Math.Floor((decimal)resolution);

            // Prevent smaller then 1 resolutions to be taken
            if (resolution < 1)
                resolution = 1;

            if (_tookPic && LtSettings.hideUiOnScreenshot && File.Exists(_screenshotFile))
                GameEvents.onShowUI.Fire();

            // If there is a png file waiting to be converted, then don't do another screenshot
            if (_pngToConvert != "" && LtSettings.convertToJpg)
            {
                if (File.Exists(_pngToConvert))
                {
                    Util.ConvertToJpg(_pngToConvert, _jpgName, LtSettings.jpgQuality);
                    FileInfo file = new FileInfo(_pngToConvert);
                    if (!LtSettings.keepOriginalPng)
                        file.Delete();

                    _pngToConvert = "";
                    _takingPic = false;
                }
            }
            else
            {
                _takingPic = true;

                // Check if the folder exists
                if (!Directory.Exists(folder))
                {
                    try
                    {
                        Directory.CreateDirectory(folder);
                    }
                    catch (Exception ex)
                    {
                        Util.LogMessage("HullCamera.TakeScreenshot. Error: " + ex, Util.LogType.Error);
                        return;
                    }
                }

                // Check if the file exists
                do
                {
                    _cnt++;
                    string sName = "Screenshot_" + _cnt;
                    pngName = Path.GetFullPath(folder) + sName + ".png";
                    _jpgName = Path.GetFullPath(folder) + sName + ".jpg";

                } while (File.Exists(pngName) || File.Exists(_jpgName));

                if (LtSettings.hideUiOnScreenshot)
                    GameEvents.onHideUI.Fire();

                _tookPic = true;
                _screenshotFile = pngName;
                Application.CaptureScreenshot(pngName, resolution);

                if (LtSettings.convertToJpg)
                    _pngToConvert = pngName;
            }
        }

        #endregion

        #region KSP Events

        [KSPEvent(guiActive = true, guiName = "Take RL picture")]
        public void ActivateCameraEvent()
        {
            ActivateCamera();
        }

        [KSPEvent(guiActive = true, guiName = "Take photo")]
        public void DetectAnomalyEvent()
        {
            DetectAnomaly();
        }

        #endregion

        #region KSP Actions

        [KSPAction("Take RL picture")]
        public void ActivateCameraAction(KSPActionParam ap)
        {
            ActivateCamera();
        }

        [KSPAction("Take photo")]
        public void DetectAnomalyAction(KSPActionParam ap)
        {
            DetectAnomaly();
        }

        #endregion

        #region Event Handlers

        public new void Update()
        {
            if (vessel == null)
                return;

            if (!_takingPic && _tookPic && LtSettings.hideUiOnScreenshot)
                GameEvents.onShowUI.Fire();

            if (_takingPic)
            {
                _timer += Time.deltaTime;

                if (_timer > 0.1f && !_tookPic)
                    TakeScreenshot();

                if (_timer > LtSettings.shuttertime)
                {
                    ToMainCamera();
                    DetectAnomaly();
                    _takingPic = false;
                }
            }
        }

        public void FixedUpdate()
        {
            if (vessel == null)
                return;

            if (MapView.MapIsEnabled)
                return;

            if (_cam == null)
            {
                _cam = FlightCamera.fetch;

                // Just a safety check
                if (_cam == null)
                    return;
            }

            if ((_cam != null) && (_origParent == null))
            {
                SaveMainCamera();
            }

            if (ltCamActive && (part.State == PartStates.DEAD))
            {
                LeaveCamera();
                CleanUp();
            }

            if ((_origParent != null) && (_cam != null) && ltCamActive)
            {
                _cam.SetTargetNone();
                _cam.transform.parent = cameraTransformName.Length > 0 ? part.FindModelTransform(cameraTransformName) : part.transform;
                _cam.DeactivateUpdate();
                _cam.transform.localPosition = cameraPosition;
                _cam.transform.localRotation = Quaternion.LookRotation(cameraForward, cameraUp);
                _cam.SetFoV(cameraFoV);
                Camera.main.nearClipPlane = cameraClip;
            }

            OnFixedUpdate();
        }

        public override void OnStart(StartState state)
        {
            part.OnJustAboutToBeDestroyed += CleanUp;
            part.OnEditorDestroy += CleanUp;

            Events["DeployExperiment"].active = false;
            Events["DeployExperiment"].guiActive = false;
            Actions["DeployAction"].active = false;

            base.OnStart(state);
        }

        private void CleanUp()
        {
            if (_currentCamera == this)
                LeaveCamera();

            if (ltCamActive)
            {
                _currentCamera = null;
                ToMainCamera();
            }
        }

        public new void OnDestroy()
        {
            CleanUp();
        }

        public override string GetInfo()
        {
            return usesFilm ? "Mode: FilmBased" : "Mode: Digital";
        }

        #endregion
    }
}
