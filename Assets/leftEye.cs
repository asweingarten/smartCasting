using UnityEngine;
using System.Collections;

public class leftEye : MonoBehaviour {

	// Use this for initialization
	void Start () {
		((MovieTexture)renderer.material.mainTexture).Play();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
