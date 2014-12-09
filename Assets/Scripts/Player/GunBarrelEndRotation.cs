using UnityEngine;
using System.Collections;

public class GunBarrelEndRotation : MonoBehaviour {

	Transform trans;
	
	const float x = (float)0.26779;
	const float y = (float)1.1902;
	const float z = (float)1.1296;

	float newX, newY, newZ;
	
	// Use this for initialization
	void Start () {
		trans = GetComponent<Transform> ();
	}
	
	// Update is called once per frame
	void Update () {
		newX = trans.position.x + x;
		newY = trans.position.y + y;
		newZ = trans.position.z + z + 2f;
		trans.position.Set(newX, newY, newZ);
	}
}
