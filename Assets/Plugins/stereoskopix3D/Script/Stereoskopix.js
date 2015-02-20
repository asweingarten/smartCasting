/*

MODIFIED BY EDY: 
This is a modified version of Stereoskopix 3D v027 for supporting the
Active Stereoscopic plugin from Dembeta. The "Alternate" method 
now communicates with the plugin for triggering the active stereo 
mode.

The script requires a DUMMY CAMERA: Don't Clear Background, Render 
mask = nothing. This camera triggers the OnRenderImage calls
required for the script to run.

The actual cameras capturing the L-R pictures are children of this
one. Use this script together with the StereoCam.js script also
included.

Information on the original script follows.

.-------------------------------------------------------------------
|  Unity Stereoskopix 3D v027
|-------------------------------------------------------------------
|  This all started when TheLorax began this thread:
|  http://forum.unity3d.com/threads/11775 
|-------------------------------------------------------------------
|  There were numerous contributions to the thread from 
|  aNTeNNa trEE, InfiniteAlec, Jonathan Czeck, monark and others.
|-------------------------------------------------------------------
|  checco77 of Esimple Studios wrapped the whole thing up
|  in a script & packaged it with a shader, materials, etc. 
|  http://forum.unity3d.com/threads/60961 
|  Esimple included a copyright & license:
|  Copyright (c) 2010, Esimple Studios All Rights Reserved.
|  License: Distributed under the GNU GENERAL PUBLIC LICENSE (GPL) 
| ------------------------------------------------------------------
|  I tweaked everything, added options for Side-by-Side, Over-Under,
|  Swap Left/Right, etc, along with a GUI interface: 
|  http://forum.unity3d.com/threads/63874 
|-------------------------------------------------------------------
|  Wolfram then pointed me to shaders for interlaced/checkerboard display.
|-------------------------------------------------------------------
|  In this version (v026), I added Wolfram's additional display modes,
|  moved Esimple's anaglyph options into the script (so that only one
|  material is needed), and reorganized the GUI.
|-------------------------------------------------------------------
|  The package consists of
|  1) this script ('stereoskopix3D.js')
|  2) a shader ('stereo3DViewMethods.shader') 
|  3) a material ('stereo3DMat')
|  4) a demo scene ('demoScene3D.scene') - WASD or arrow keys travel, 
|     L button grab objects, L button lookaround when GUI hidden.
|-------------------------------------------------------------------
|  Instructions: (NOTE: REQUIRES UNITY PRO) 
|  1. Drag this script onto your camera.
|  2. Drag 'stereoMat' into the 'Stereo Materials' field.
|  3. Hit 'Play'. 
|  4. Adjust parameters with the GUI controls, press the tab key to toggle.
|  5. To save settings from the GUI, copy them down, hit 'Stop',
|     and enter the new settings in the camera inspector.
'-------------------------------------------------------------------
|  Perry Hoberman <hoberman (at) bway.net
|-------------------------------------------------------------------
*/


#pragma strict
@script RequireComponent (Camera)


public var stereoMaterial : Material;


enum mode3D { Anaglyph, SideBySide, OverUnder, Interlace, Checkerboard, Alternate };
public var format3D = mode3D.Anaglyph;
 
enum modeSBS {Squeezed,Unsqueezed};
public var sideBySideOptions = modeSBS.Squeezed;

public var interaxial : float = 0.25;
public var zeroParallax : float = 6.0;

enum method3D {Parallel,ToedIn};
public var cameraMethod = method3D.Parallel;

enum cams3D {LeftRight, LeftOnly, RightOnly, RightLeft};
public var cameraSelect = cams3D.LeftRight;


private var leftCam : Camera;				// These cameras must NOT use RenderTextures
private var rightCam : Camera;
private var leftCamRT : RenderTexture;
private var rightCamRT : RenderTexture;

private var stereoEnabled = false;


function Start () {

	if (!stereoMaterial) {
		Debug.LogError("No Stereo Material Found. Please drag 'stereoMat' into the Stereo Material Field");
		this.enabled = false;
		return;
	}
	
	UpdateView();
	
	camera.cullingMask = 0;
	camera.backgroundColor = Color (0,0,0,0);
	camera.clearFlags = CameraClearFlags.Nothing;
	camera.enabled = false;
}


private var m_screenWidth = 0;
private var m_screenHeight = 0;


private function EnsureRenderTextures()
	{
	if (m_screenWidth != Screen.width || m_screenHeight != Screen.height || leftCamRT == null || rightCamRT == null)
		{
		if (leftCamRT) leftCamRT.Release();
		if (rightCamRT) rightCamRT.Release();
		leftCamRT = new RenderTexture (Screen.width, Screen.height, 24);
		rightCamRT = new RenderTexture (Screen.width, Screen.height, 24);
		
		stereoMaterial.SetTexture ("_LeftTex", leftCamRT);
		stereoMaterial.SetTexture ("_RightTex", rightCamRT);

		if (stereoEnabled)
			{
			leftCam.targetTexture = leftCamRT;
			rightCam.targetTexture = rightCamRT;
			}
		
		m_screenWidth = Screen.width;
		m_screenHeight = Screen.height;
		}
	}



function LateUpdate() {
	UpdateView();
}


function isStereoEnabled()
	{
	return stereoEnabled;
	}


function EnableStereo(CameraLeft : Camera, CameraRight : Camera, CameraRigLeft : Transform, CameraRigRight : Transform)
	{
	if (!CameraLeft || !CameraRight) return;

	leftCam = CameraLeft;
	rightCam = CameraRight;
	
	// Store left
	
	camera.projectionMatrix = leftCam.projectionMatrix;
	camera.nearClipPlane = leftCam.nearClipPlane;
	camera.farClipPlane = leftCam.farClipPlane;
	camera.aspect = leftCam.aspect;	
	
	// Set up stereoscopic cameras
	
	camera.depth = Mathf.Max(leftCam.depth, rightCam.depth) + 1;
	
	leftCam.enabled = false;		// Cameras will render on demand at OnRenderImage
	rightCam.enabled = false;
	
	camera.enabled = true;
	stereoEnabled = true;
	}
	
	
function DisableStereo()
	{
	if (!stereoEnabled) return;
		
	leftCam.targetTexture = null;
	rightCam.targetTexture = null;
	if (leftCamRT) leftCamRT.Release();
	if (rightCamRT) rightCamRT.Release();
	leftCamRT = null;
	rightCamRT = null;
	
	leftCam.ResetProjectionMatrix();
	rightCam.ResetProjectionMatrix();
	
	leftCam.transform.localPosition.x = 0.0;
	
	camera.enabled = false;
	stereoEnabled = false;
	}



function UpdateView() 
	{
	if (!stereoEnabled)
		return;

	switch (cameraSelect) {
		case cams3D.LeftRight:
			leftCam.transform.position = transform.position + transform.TransformDirection(-interaxial/2, 0, 0);
			rightCam.transform.position = transform.position + transform.TransformDirection(interaxial/2, 0, 0);
			break;
		case cams3D.LeftOnly:
			leftCam.transform.position = transform.position + transform.TransformDirection(-interaxial/2, 0, 0);
			rightCam.transform.position = transform.position + transform.TransformDirection(-interaxial/2, 0, 0);
			break;
		case cams3D.RightOnly:
			leftCam.transform.position = transform.position + transform.TransformDirection(interaxial/2, 0, 0);
			rightCam.transform.position = transform.position + transform.TransformDirection(interaxial/2, 0, 0);
			break;
		case cams3D.RightLeft:
			leftCam.transform.position = transform.position + transform.TransformDirection(interaxial/2, 0, 0);
			rightCam.transform.position = transform.position + transform.TransformDirection(-interaxial/2, 0, 0);
			break;
	}
	
	if (cameraMethod == method3D.ToedIn) {
		leftCam.projectionMatrix = camera.projectionMatrix;
		rightCam.projectionMatrix = camera.projectionMatrix;
		leftCam.transform.LookAt (transform.position + (transform.TransformDirection (Vector3.forward) * zeroParallax));
		rightCam.transform.LookAt (transform.position + (transform.TransformDirection (Vector3.forward) * zeroParallax));
	} else {
		leftCam.transform.rotation = transform.rotation;
	    rightCam.transform.rotation = transform.rotation;
	    switch (cameraSelect) {
			case cams3D.LeftRight:
				leftCam.projectionMatrix = projectionMatrix(true);
				rightCam.projectionMatrix = projectionMatrix(false);
				break;
			case cams3D.LeftOnly:
				leftCam.projectionMatrix = projectionMatrix(true);
				rightCam.projectionMatrix = projectionMatrix(true);
				break;
			case cams3D.RightOnly:
				leftCam.projectionMatrix = projectionMatrix(false);
				rightCam.projectionMatrix = projectionMatrix(false);
				break;
			case cams3D.RightLeft:
				leftCam.projectionMatrix = projectionMatrix(false);
				rightCam.projectionMatrix = projectionMatrix(true);
				break;
		}
	}
}


private var m_frameNum = 0;

function SwitchEyes()
	{
	m_frameNum++;
	Debug.Log("Eyes switched!  " + m_frameNum);
	}

	
private var m_testCount = 0;
	

function OnRenderImage (source:RenderTexture, destination:RenderTexture) 
	{
	EnsureRenderTextures();
	
	// Alternate mode adapted to active stereoscopic 3D via plugin
	
	if (format3D == mode3D.Alternate)
		{
		leftCam.Render();
		Graphics.Blit(leftCamRT, destination);
		GL.IssuePluginEvent (1);
		
		rightCam.Render();
		Graphics.Blit(rightCamRT, destination);
		GL.IssuePluginEvent (2);
		
		return;
		}
	
	leftCam.targetTexture = leftCamRT;
	rightCam.targetTexture = rightCamRT;
	
	leftCam.Render();	
	rightCam.Render();

	RenderTexture.active = destination;
	GL.PushMatrix();
	GL.LoadOrtho();
	
	switch (format3D) 
		{
		case mode3D.Anaglyph:
			stereoMaterial.SetPass(0);
			DrawQuad(0);
			break;
			
		case mode3D.SideBySide:
		case mode3D.OverUnder:
			stereoMaterial.SetPass(1);
			DrawQuad(1);
			stereoMaterial.SetPass(2);
			DrawQuad(2);
			break;
			
		case mode3D.Interlace:
		case mode3D.Checkerboard:
			stereoMaterial.SetPass(3);
			DrawQuad(3);
			break;

		default:
			break;
		}
	GL.PopMatrix();
	
	if (format3D == mode3D.SideBySide)
		GL.IssuePluginEvent (0);
	}
	

	
private function DrawQuad(cam : int) {
	if (format3D == mode3D.Anaglyph) {
	   		GL.Begin (GL.QUADS);      
	      	GL.TexCoord2( 0.0, 0.0 ); GL.Vertex3( 0.0, 0.0, 0.1 );
	      	GL.TexCoord2( 1.0, 0.0 ); GL.Vertex3( 1, 0.0, 0.1 );
	      	GL.TexCoord2( 1.0, 1.0 ); GL.Vertex3( 1, 1.0, 0.1 );
	      	GL.TexCoord2( 0.0, 1.0 ); GL.Vertex3( 0.0, 1.0, 0.1 );
	   		GL.End();
	} else {
		if (format3D==mode3D.SideBySide) {
			if (cam==1) {
		   		GL.Begin (GL.QUADS);      
		      	GL.TexCoord2( 0.0, 0.0 ); GL.Vertex3( 0.0, 0.0, 0.1 );
		      	GL.TexCoord2( 1.0, 0.0 ); GL.Vertex3( 0.5, 0.0, 0.1 );
		      	GL.TexCoord2( 1.0, 1.0 ); GL.Vertex3( 0.5, 1.0, 0.1 );
		      	GL.TexCoord2( 0.0, 1.0 ); GL.Vertex3( 0.0, 1.0, 0.1 );
		   		GL.End();
			} else {
		   		GL.Begin (GL.QUADS);      
		      	GL.TexCoord2( 0.0, 0.0 ); GL.Vertex3( 0.5, 0.0, 0.1 );
		      	GL.TexCoord2( 1.0, 0.0 ); GL.Vertex3( 1.0, 0.0, 0.1 );
		      	GL.TexCoord2( 1.0, 1.0 ); GL.Vertex3( 1.0, 1.0, 0.1 );
		      	GL.TexCoord2( 0.0, 1.0 ); GL.Vertex3( 0.5, 1.0, 0.1 );
		   		GL.End();
			}
		} else if (format3D == mode3D.OverUnder) {
			if (cam==1) {
		   		GL.Begin (GL.QUADS);      
		      	GL.TexCoord2( 0.0, 0.0 ); GL.Vertex3( 0.0, 0.5, 0.1 );
		      	GL.TexCoord2( 1.0, 0.0 ); GL.Vertex3( 1.0, 0.5, 0.1 );
		      	GL.TexCoord2( 1.0, 1.0 ); GL.Vertex3( 1.0, 1.0, 0.1 );
		      	GL.TexCoord2( 0.0, 1.0 ); GL.Vertex3( 0.0, 1.0, 0.1 );
		   		GL.End();
			} else {
		   		GL.Begin (GL.QUADS);      
		      	GL.TexCoord2( 0.0, 0.0 ); GL.Vertex3( 0.0, 0.0, 0.1 );
		      	GL.TexCoord2( 1.0, 0.0 ); GL.Vertex3( 1.0, 0.0, 0.1 );
		      	GL.TexCoord2( 1.0, 1.0 ); GL.Vertex3( 1.0, 0.5, 0.1 );
		      	GL.TexCoord2( 0.0, 1.0 ); GL.Vertex3( 0.0, 0.5, 0.1 );
		   		GL.End();
			} 
		} else if (format3D == mode3D.Interlace || format3D == mode3D.Checkerboard) {
	   		GL.Begin (GL.QUADS);      
	      	GL.TexCoord2( 0.0, 0.0 ); GL.Vertex3( 0.0, 0.0, 0.1 );
	      	GL.TexCoord2( 1.0, 0.0 ); GL.Vertex3( 1, 0.0, 0.1 );
	      	GL.TexCoord2( 1.0, 1.0 ); GL.Vertex3( 1, 1.0, 0.1 );
	      	GL.TexCoord2( 0.0, 1.0 ); GL.Vertex3( 0.0, 1.0, 0.1 );
	   		GL.End();
		}
	}
} 

function PerspectiveOffCenter(
    left : float, right : float,
    bottom : float, top : float,
    near : float, far : float ) : Matrix4x4 
	{       
    var x =  (2.0 * near) / (right - left);
    var y =  (2.0 * near) / (top - bottom);
    var a =  (right + left) / (right - left);
    var b =  (top + bottom) / (top - bottom);
    var c = -(far + near) / (far - near);
    var d = -(2.0 * far * near) / (far - near);
    var e = -1.0;

    var m : Matrix4x4;
    m[0,0] = x;  m[0,1] = 0;  m[0,2] = a;  m[0,3] = 0;
    m[1,0] = 0;  m[1,1] = y;  m[1,2] = b;  m[1,3] = 0;
    m[2,0] = 0;  m[2,1] = 0;  m[2,2] = c;  m[2,3] = d;
    m[3,0] = 0;  m[3,1] = 0;  m[3,2] = e;  m[3,3] = 0;
    return m;
}

function projectionMatrix(isLeftCam : boolean) : Matrix4x4 {
   var left : float;
   var right : float;
   var a : float;
   var b : float;
   var FOVrad : float;
   var aspect: float = camera.aspect;
   var tempAspect: float;
   if (sideBySideOptions == modeSBS.Unsqueezed && format3D == mode3D.SideBySide) {
   		FOVrad = camera.fieldOfView / 90.0 * Mathf.PI;
   		tempAspect = aspect/2;
   } else {
   		FOVrad = camera.fieldOfView / 180.0 * Mathf.PI;
   		tempAspect = aspect;
   }
 
   a = camera.nearClipPlane * Mathf.Tan(FOVrad * 0.5);
   b = camera.nearClipPlane / (zeroParallax + camera.nearClipPlane);
   
   if (isLeftCam) {
      left  = - tempAspect * a + (interaxial/2) * b;
      right =   tempAspect * a + (interaxial/2) * b;
   }
   else {
      left  = - tempAspect * a - (interaxial/2) * b;
      right =   tempAspect * a - (interaxial/2) * b;
   }

   return PerspectiveOffCenter(left, right, -a, a, camera.nearClipPlane, camera.farClipPlane);
    
} 