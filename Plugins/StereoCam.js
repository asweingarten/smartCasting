
#pragma strict 
@script RequireComponent(Camera)
@script RequireComponent(Stereoskopix)


// This script controls the sterereoscopic settings.
// Stereoscopic cameras must be childen of this GameObject and be referenced here.
//
// This GameObject must have a dummy camera (Don't clear background, culling mask = nothing) 
// which is used to intercept the Render calls and communicate with the plugin.
//
// (See the modified version of the Stereoskopix plugin included).


enum StereoModes { Disabled, Anaglyph, Interlaced, Active, SideBySide };

var CamL : Camera;
var CamR : Camera;
var stereo = StereoModes.Disabled;

var eyeDistance = 0.06;
var parallaxDistance = 3.0;

var autoEyeDistance = false;
var autoEyeParallax = false;
var autoEyeLayerMask : LayerMask = (1<<32)-1;
var autoEyeMinRange = 0.25;
var autoEyeMaxRange = 0.75;
var autoEyeDistanceMin = 0.01;
var autoEyeDistanceMax = 0.06;
var autoEyeDistanceSpeed = 2.0;
var autoEyeParallaxMin = 1.0;
var autoEyeParallaxMax = 3.0;
var autoEyeParallaxSpeed = 2.0;


private var m_StereoScript : Stereoskopix;

function Start ()
	{
	m_StereoScript = GetComponent(Stereoskopix) as Stereoskopix;
	}
	
	
private function DoAutoEyeParameters()
	{
	var Hit : RaycastHit;
	var touch = false;
	var distanceRatio : float;
	
	if (autoEyeDistance || autoEyeParallax)
		{
		touch = Physics.Raycast (transform.position, transform.forward, Hit, Mathf.Infinity, autoEyeLayerMask);
		distanceRatio = Mathf.InverseLerp(autoEyeMinRange, autoEyeMaxRange, Hit.distance);
		if (!touch) distanceRatio = 1.0;
		}
		
	if (autoEyeDistance)
		{
		var newDistance = Mathf.Lerp(autoEyeDistanceMin, autoEyeDistanceMax, distanceRatio);
		eyeDistance = Mathf.Lerp(eyeDistance, newDistance, autoEyeDistanceSpeed * Time.deltaTime);
		}
		
	if (autoEyeParallax)
		{
		var newParallax = Mathf.Lerp(autoEyeParallaxMin, autoEyeParallaxMax, distanceRatio);
		parallaxDistance = Mathf.Lerp(parallaxDistance, newParallax, autoEyeParallaxSpeed * Time.deltaTime);
		}
	}
	

function Update () 
	{
	DoAutoEyeParameters();
	
	if (parallaxDistance < 0.2) parallaxDistance = 0.2;
	if (eyeDistance < 0.0) eyeDistance = 0.0;
	if (eyeDistance > 0.2) eyeDistance = 0.2;
	
	if (stereo != StereoModes.Disabled && CamR)
		{
		// Enable stereoscopic 3D mode always as Parallel 
		// (uses camera.fieldOfView to calculate camera's parallel frustrums)
		
		if (!m_StereoScript.isStereoEnabled() || m_StereoScript.cameraMethod != method3D.Parallel)
			{
			// EnableStereo will disable the children cameras and render them on demand
			
			m_StereoScript.DisableStereo();
			m_StereoScript.cameraMethod = method3D.Parallel;
			m_StereoScript.EnableStereo(CamL, CamR, null, null);
			}
			
		// Apply stereoscopic 3D parameters to the Scereoskopix script
			
		switch (stereo)
			{
			case StereoModes.Anaglyph: m_StereoScript.format3D = mode3D.Anaglyph; break;
			case StereoModes.Interlaced: m_StereoScript.format3D = mode3D.Interlace; break;
			case StereoModes.Active: m_StereoScript.format3D = mode3D.Alternate; break;
			case StereoModes.SideBySide: m_StereoScript.format3D = mode3D.SideBySide; break;
			}
		
		m_StereoScript.zeroParallax = parallaxDistance;
		m_StereoScript.interaxial = eyeDistance;
		}
	else
		{
		// Stereoscopic 3D disabled.
		// DisableStereo relocates the left camera at the local center of the GameObject.
		// We use the left camera for non-stereoscopic 3D render.
		
		m_StereoScript.DisableStereo();
		
		CamL.enabled = true;
		CamL.fieldOfView = camera.fieldOfView;
		
		if (CamR) CamR.enabled = false;
		}
	}
	

function SwitchEyes()
	{
	m_StereoScript.SwitchEyes();
	}
	
	
function SetCameraClearFlags (flags : CameraClearFlags, clColor : Color)
	{
	CamL.clearFlags = flags;
	CamL.backgroundColor = clColor;
	
	if (CamR) 
		{
		CamR.clearFlags = flags;
		CamR.backgroundColor = clColor;
		}
	}
	
