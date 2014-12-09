using UnityEngine;
using System.Collections;

public class Crosshair : MonoBehaviour {

	public Texture crosshairTexture;
	Rect crosshairRect;
	void Start () 
	{
		float crosshairSize = Screen.width * 0.05f;
		crosshairRect = new Rect (Screen.width / 2 - crosshairSize / 2,
		                      Screen.height / 2 - crosshairSize / 2,
		                      crosshairSize, crosshairSize);
	}
	
	void OnGUI()
	{
		GUI.DrawTexture (crosshairRect, crosshairTexture);
		Screen.showCursor = false;
	}
}
