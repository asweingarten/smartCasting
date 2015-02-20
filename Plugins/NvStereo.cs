using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


public class NvStereo : MonoBehaviour 
	{
	[DllImport ("NvStereoRenderer")]
	private static extern System.IntPtr NV_GetLog ();	

	[DllImport ("NvStereoRenderer")]
	private static extern int NV_GetLastResult ();
	
	[DllImport ("NvStereoRenderer")]
	private static extern int NV_GetLastError ();
	
	[DllImport ("NvStereoRenderer")]
	private static extern int NV_GetErrorCount ();
	
	
	public bool showLog = false;

	
	void Awake ()
		{
		// Some function in the Plugin DLL must be invoked to ensure it to get loaded.
		
		NV_GetLastResult();
		}

	
	private string GetInfoString()
		{
		return System.String.Format("{0}\nLast result: {1}\nLast error: 0x{2,00000000:X} (count: {3})", Marshal.PtrToStringAnsi(NV_GetLog()), NV_GetLastResult(), NV_GetLastError(), NV_GetErrorCount());
		}
	
	
	void OnGUI ()
		{
		if (showLog)
			GUI.Label(new Rect(10, 10, Screen.width/2, Screen.height), GetInfoString());
		}
	
		
	void OnApplicationQuit ()
		{
		if (showLog)
			Debug.Log(GetInfoString());
		}
	}
