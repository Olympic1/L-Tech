using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using UnityEngine;
using LTScience;

public class HullCamera : LTechScienceBase
{
    [KSPField]
    public Vector3 cameraPosition = Vector3.zero;
    
    [KSPField]
    public Vector3 cameraForward = Vector3.forward;
    
    [KSPField]
    public Vector3 cameraUp = Vector3.up;
    
    [KSPField]
    public string cameraTransformName = "";
    
    [KSPField]
    public float cameraFoV = 60;
    
    [KSPField]
    public float picscival = 1f;
    
    [KSPField]
    public float picdatval = 1f;
    
    [KSPField]
    public float shuttertime = 0.2f;
    
    [KSPField(isPersistant = false)]
    public float cameraClip = 0.01f;
    
    [KSPField]
    public bool ltcamActive = false;
    
    [KSPField(isPersistant = false)]
    public bool usesfilm = false;
    
    [KSPField]
    public string specialExperimentName;
    
    [KSPField]
    public string specialExperimentTitle;
    
    [KSPField(isPersistant = false)]
    public string cameraName = "Hull";
    
    public LTechCCore config;
    public static HullCamera currentCamera = null;
    
    protected static FlightCamera cam = null;
    protected static Transform origParent = null;
    protected static float origFoV;
    protected static float origClip;
    protected static Texture2D overlayTex = null;
    
    protected static int globalInput = 0;
    
    private string kspDir = KSPUtil.ApplicationRootPath;
    private string kspDir2 = KSPUtil.ApplicationRootPath + @"GameData/LTech/Plugins/PluginData/LtScience/";
    //public int screenshotCount, superSampleValueInt = 3;
    public bool takingpic = false;
    public bool tookpic = false;
    public float timer = 0;
    
    public void toMainCamera()
    {
        if ((cam != null) && (cam.transform != null))
        {
            cam.transform.parent = origParent;
            Camera.main.nearClipPlane = origClip;
            cam.setTarget(FlightGlobals.ActiveVessel.transform);
            
            if (currentCamera != null)
            {
                currentCamera.ltcamActive = false;
            }
            
            currentCamera = null;
            MapView.EnterMapView();
            MapView.ExitMapView();
        }
    }
    
    [KSPEvent(guiActive = true, guiName = "Take RL picture")]
    public void ActivateCamera()
    {
        if (part.State == PartStates.DEAD)
        {
            return;
        }
        
        print("Activating camera.");
        
        ltcamActive = !ltcamActive;
        
        if (!ltcamActive && (cam != null))
        {
            toMainCamera();
        }
        else
        {
            if ((currentCamera != null) && (currentCamera != this))
            {
                currentCamera.ltcamActive = false;
            }
            
            currentCamera = this;
            BeginPic();
        }
    }
    
    public void BeginPic()
    {
        takingpic = true;
        tookpic = false;
        timer = 0;
    }
    
    [KSPAction("Take RL picture")]
    public void ActivateCameraAction(KSPActionParam ap)
    {
        ActivateCamera();
    }
    
    [KSPEvent(guiActive = true, guiName = "Take photo")]
    public void DetectAnomaly()
    {
    	if (usesfilm && !LTSUseResources("CameraFilm", 1))
    	{
            ScreenMessages.PostScreenMessage("Need more Camera Film!", 6, ScreenMessageStyle.UPPER_CENTER);
            return;
    	}
        
        print("Detecting anomalies!");
        
        PQSCity[] muns = vessel.mainBody.GetComponentsInChildren<PQSCity>(true);
        
        double nearest = double.PositiveInfinity;
        PQSCity anomaly = null;

        foreach (PQSCity mun in muns) // Find the closest anomaly.
        {
            print("Anomaly " + mun.name);
            double dist = (mun.transform.position - part.transform.position).magnitude;
            
            if (dist < nearest)
            {
                nearest = dist;
                anomaly = mun;
            }
        }
        
        string ExpID = "Space";
        string ExpName = "";
        float ExpValue = picscival;
        float ExpData = picdatval;
        
        if (anomaly != null)
        {
            print("Closest anomaly is " + anomaly.name + " and " + nearest + " away!");
            
	        if (nearest < 2500)
	        {
                double targetAngleTo = Vector3d.Dot(part.transform.up + cameraForward, (anomaly.transform.position - part.transform.position).normalized);
                print("Angle: " + targetAngleTo);
                
		        if (targetAngleTo > 1)
		        {
                    ExpID = vessel.mainBody.name + "-" + anomaly.name;
                    ExpValue = (float)((Math.Abs(2500 - nearest) * (targetAngleTo - 1)) / 100) * picscival;
                    ExpName = anomaly.name;
                    ExpData = 20f * picdatval;
		        }
	        }
        }
        
        print(ExpName + " is worth: " + ExpValue);
        
        base.experiment.id = specialExperimentName + ExpID;
        base.experiment.experimentTitle = specialExperimentTitle.Replace("#Anon#", ExpName);
        base.experiment.baseValue = ExpValue;
        base.experiment.dataScale = ExpData;
        base.DeployExperiment();
    }
    
	public void screenshotMethod()
	{
        int resolution = (int)config.camres;
        resolution = (int)Math.Floor((decimal)resolution);
        
		if (resolution < 1)
        {
            resolution=1; // Prevents smaller then 1 resolutions to be taken.
        }
        
        string screenshotFilename = "Screenshot" + Time.realtimeSinceStartup;
        print(screenshotFilename);
        print("Your supersample value was " + resolution + "!");
        Application.CaptureScreenshot(kspDir2 + screenshotFilename + ".png", resolution);
	}
    
    public void Update()
    {
        if (vessel == null)
        {
            return;
        }
        
		if (takingpic)
        {
            timer += Time.deltaTime;

			if (timer > 0.1F && !tookpic)
            {
                screenshotMethod();
                tookpic = true;
			}

			if (timer > config.strtime)
            {
                print("Deactivating camera.");
                toMainCamera();
                DetectAnomaly();
                takingpic = false;
			}
		}
    }
    
    public void FixedUpdate()
    {
        if (vessel == null)
        {
            return;
        }
        
        if (cam == null)
        {
            cam = FlightCamera.fetch;
        }
        
        if ((cam != null) && (origParent == null))
        {
            origParent = cam.transform.parent;
            origClip = Camera.main.nearClipPlane;
            origFoV = Camera.main.fieldOfView;
        }
        
        if (ltcamActive && (part.State == PartStates.DEAD))
        {
            CleanUp();
        }
        
        if ((origParent != null) && (cam != null) && ltcamActive)
        {
            print("Setting camera focus.");
            cam.setTarget(null);
            cam.transform.parent = (cameraTransformName.Length > 0) ? part.FindModelTransform(cameraTransformName) : part.transform;
            cam.transform.localPosition = cameraPosition;
            cam.transform.localRotation = Quaternion.LookRotation(cameraForward, cameraUp);
            Camera.main.nearClipPlane = cameraClip;
        }
        
        base.OnFixedUpdate();
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
    
    public void CleanUp()
    {
        if (ltcamActive)
        {
            toMainCamera();
        }
    }

    new public void OnDestroy()
    {
        CleanUp();
    }
    
	public override string GetInfo()
    {
		if (usesfilm)
		{
            return "Mode: FilmBased";
		}

        return "Mode: Digital";
    }
}