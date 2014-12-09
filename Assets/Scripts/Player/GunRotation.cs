using UnityEngine;
using System.Collections;

public class GunRotation : MonoBehaviour {
	
	//Camera camera;
	GameObject gameObject; 
	Camera camera;
	Transform trans;

	// Use this for initialization
	void Start () {
		//camera = GetComponentInParent<Camera> ();
		gameObject = GameObject.FindGameObjectsWithTag("MainCamera")[0];
		camera = gameObject.camera;
		trans = GetComponent<Transform> ();
	}
	
	// Update is called once per frame
	void Update () {
		trans.rotation = camera.transform.rotation;
	}
}
