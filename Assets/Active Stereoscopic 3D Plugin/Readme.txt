Active Stereoscopic 3D Plugin for Unity
Developed by Edy for Dembeta SL.
Version 2.1

Bring true active stereoscopic 3D to your games!
By using two Unity Camera objects this pluging provides a true active stereoscopic 3D signal to the 3D hardware. Perfect support for stereoscopic real-time shadows and image effects. Settings include eye distance and parallax with parallel frustrum.
Windows only. Requires Unity Pro and the NVidia 3D Vision driver. Supported hardware includes IR emitters, DLP TVs (DVI/HDMI cable), LCD displays (dual-link DVI cable), anaglyph glasses and any HDMI 1.4-compliant stereoscopic 3D device (TV, bluray, etc).
Devices not directly supported by 3D Vision require the software NVidia 3DTV Play to be installed (http://www.nvidia.com/object/3dtv-play-overview.html).

For FAQ and detailed information please visit the plugin's site:
http://dembeta.repositoryhosting.com/trac/dembeta_active-stereoscopic-3d

Prerequisites:

- NVIDIA 3D Vision driver for Windows is required.
	http://www.nvidia.com/object/3d-drivers-downloads.html
- Test your stereoscopic hardware using the 3D Vision Test assistant.
	NVidia Control Panel > 3D Vision > Set up stereoscopic 3D > Test Stereoscopic 3D > Run Setup Wizard
- Configure the 3D Vision settings:
	NVidia Control Panel > 3D Vision > Set up stereoscopic 3D
	* Enable stereoscopic 3D
	* Depth: Min (move the slider to the leftmost end).

Setup:

1- Import the package
2- Move the Plugins folder from Active Stereoscopic 3D Plugin to the root of your Project. Otherwise the plugin will fail to load the DLL.
3- A Stereoscopic Main Camera prefab is provided, which includes all necessary components for composing the stereoscopic 3D image and communicating with the 3D Vision driver. Use this camera instead of the regular Main Camera.
4- See below for detailed instructions on using the Plugin.

Instructions for building the Demo project:

1- Create a new Unity3D project which includes the Character Controller package. 
2- Import the Active Stereoscopic 3D package into the project (Asset Store > Downloads).
	IMPORTANT: Move the Plugins folder from Active Stereoscopic 3D Plugin to the root of your Project.
3- Open the Demo Scene and click Play. 
	You should see a side-by-side picture in the Game window. This is ok and means that the plugin is working properly. 3D Vision doesn't work inside the Unity Editor, but the plugin is combining the two cameras correctly in the Game view.
4- Prepare to build the scene.
	At File > Build Settings choose PC and Mac Standalone, Target platform: Windows.
	Ensure the Demo Scene is the only one at the section Scenes in Build. Drag it from the Project tree if it's not there.
5- Build and Run the scene.
	Choose a resolution and disable Windowed (stereoscopic 3D works in full screen only).
	You should see the scene in active stereoscopic 3D using your 3D hardware.
	If you still see the side-by-side picture, enable the 3D Vision mode using the keyboard shortcut (Ctrl-T by default).
	
Instructions for using Active Stereoscopy in your existing project:

1- Import the package into your project (Asset Store > Downloads)
	IMPORTANT: Move the Plugins folder from Active Stereoscopic 3D Plugin to the root of your Project.
2- Change your Main Camera with the provided Stereoscopic Main Camera prefab.
	Ensure to update all references to your old Main Camera so they now point to the Stereoscopic Main Camera.
3- Scripts and effects:
	* Non visual scripts and effects (i.e. movement or behavior) should go into the Stereoscopic Main Camera gameobject.
	* Visual effects should go into the children Camera L and Camera R cameras. They are the cameras that actually take the pictures.
4- Clicking Play should show you a side-by-side picture in the game view.
5- Build the scene following the same instructions as explained above for the Demo project.
	* PC and Mac Standalone, Target platform: Windows.
	* Always full-screen.
	* Press Ctrl-T if running the built project shows a side-by-side picture.


For FAQ and detailed information please visit the plugin's site:
http://dembeta.repositoryhosting.com/trac/dembeta_active-stereoscopic-3d


Changelog:

v2.1

	Fixed regression where Deferred Lighting stop being supported by the plugin.
	Fixed the NvStereo.showLog option not working and/or crashing Unity.

V2.0

	Support for Unity 4.
	Performance improved by skipping unnecessary screen blits.

V1.0

	Initial release