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
public class rayOcclusionRemovalDepthCursor : MonoBehaviour {

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
}
