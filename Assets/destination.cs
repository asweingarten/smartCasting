using UnityEngine;
using System.Collections;

public class destination : MonoBehaviour {
	private Color originalColor;
	private Color originalInteractorColor;

	private void OnTriggerEnter(Collider other) {
		Debug.Log ("Collided");
		// Play a sound if the coliding objects had a big impact.		
		if (other.renderer.material.color == Color.blue) {
			Debug.Log ("Destination reached");

			// save old colors
			originalColor = renderer.material.color;
			originalInteractorColor = other.renderer.material.color;

			// mark with cyan as desitnation reached;
			renderer.material.color = Color.cyan;
			other.renderer.material.color = Color.cyan;

		}
		//transform.parent = other.transform;
		//transform.localPosition = other.transform.position;
		
	}
	
	private void OnTriggerStay(Collider other) {
		// Play a sound if the coliding objects had a big impact.		
		Debug.Log ("Ball exit");
		transform.position = other.transform.position;
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
