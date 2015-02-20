using UnityEngine;
using System.Collections;

public class destinationcollision : MonoBehaviour {
	Color originalColor = Color.red;
	Color originalPhoneColor = Color.blue;
	Vector3 distanceFromTargetCenter;
	
	private void OnTriggerEnter(Collider other) {
		// Play a sound if the coliding objects had a big impact.		
		Debug.Log ("Ball entered");
		originalColor = renderer.material.color;
		renderer.material.color = Color.cyan;

	}
	
	private void OnTriggerStay(Collider other) {
		// Play a sound if the coliding objects had a big impact.		
		Debug.Log ("Ball exit");
	}
	
	private void OnTriggerExit(Collider other) {
		// Play a sound if the coliding objects had a big impact.		
		Debug.Log ("Ball exit");
		/*		if (other.renderer.material.color == Color.red) {
			transform.position = other.transform.position;
		}
*/		renderer.material.color = originalColor;
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
