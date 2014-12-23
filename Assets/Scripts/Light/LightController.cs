using UnityEngine;
using System.Collections;

public class LightController : MonoBehaviour {

	// public powoduje ze mozemy to parametryzowac dla kazdego swiatla oddzielnie
	public Color sadColor;
	public float sadIntensity;
	public Color happyColor;
	public float happyIntensity;

	GameObject faceController;
	fs.FaceshiftLive face;

	Light lightRef;
	Gradient g = new Gradient();
	
	// Use this for initialization
	void Start () {
		faceController = GameObject.Find ("FaceController");
		face = faceController.GetComponent <fs.FaceshiftLive> ();

		// Populate the color keys at the relative time 0 and 1 (0 and 100%)
		GradientColorKey[] gck = new GradientColorKey[2];
		gck [0] = new GradientColorKey (sadColor, 0.0f);
		gck [1] = new GradientColorKey (happyColor, 0.8f);
		
		// Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
		GradientAlphaKey[] gak = new GradientAlphaKey[2];
		gak [0] = new GradientAlphaKey (sadIntensity, 0.0f);
		gak [1] = new GradientAlphaKey (happyIntensity, 0.8f);
		
		g.SetKeys(gck, gak);
		
		lightRef = GetComponent<Light> ();
	}

	void Update () {
		if(face.getHappiness() != -1)
			lightRef.color = g.Evaluate (face.getHappiness());
	}
}
