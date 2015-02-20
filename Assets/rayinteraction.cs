using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using System;
using SocketIOClient;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class rayinteraction : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision c)
	{
		if (c.gameObject.name.StartsWith("occluder")) {
			Debug.Log("Hiding "+c.gameObject.name);
			Color co = c.gameObject.renderer.material.color;
			Color hidec = new Color(co.r, co.g, co.b, 0.0f);
			Color showc = new Color (co.r, co.g, co.b, 1.0f);
			float zCut = GameObject.Find("interactor").transform.position.z;		
			float z = c.gameObject.transform.position.z;		
			if ( z < zCut) {
				c.gameObject.renderer.material.color = hidec;
			} else {
				c.gameObject.renderer.material.color = showc;
			}
		}
	}
	
	void OnCollisionStay(Collision c)
	{
	}
	
	void OnCollisionExit(Collision c)
	{
		if (c.gameObject.name.StartsWith("occluder")) {
			Debug.Log("Hiding "+c.gameObject.name);
			Color co = c.gameObject.renderer.material.color;
			Color showc = new Color (co.r, co.g, co.b, 1.0f);
			c.gameObject.renderer.material.color = showc;
		}
	}
	void hideOccluders() {
		
		for (int i = 1; i <= 43; i++) {
			GameObject occluder = GameObject.Find ("occluder"+i.ToString());
			float m = (360.0f-transform.parent.rotation.eulerAngles.x)* Mathf.Deg2Rad - 0.1f;
			float p = UnityEngine.Mathf.Atan2(occluder.transform.position.y,occluder.transform.position.z);
			//Debug.Log ("m = "+m.ToString()+" p = "+p.ToString());
			Color c = occluder.renderer.material.color;
			Color hidec = new Color(c.r, c.g, c.b, 0.0f);
			Color showc = new Color (c.r, c.g, c.b, 1.0f);
			
			if (m < p) {
				//show occluder
				occluder.renderer.material.color = hidec;
			} else {
				//hide occluder
				occluder.renderer.material.color = showc;
			}
		}
	}


}
