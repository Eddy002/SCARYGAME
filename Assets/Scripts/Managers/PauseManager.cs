using UnityEngine;

public class PauseManager : MonoBehaviour
{
		Animator anim;
		GameObject faceController;
		fs.FaceshiftLive face;
		float tempRotX = 0;
		bool isSendPause = false, isSendStart = false;

		void Awake ()
		{
				anim = GetComponent<Animator> ();
				faceController = GameObject.Find ("FaceController");
				face = faceController.GetComponent <fs.FaceshiftLive> ();
		}
	
		void Update ()
		{
				// if rotation X is same in two following cycles then face is probably not detected
				// and pause state can start
				if (StartManager.pressed) {
						if (tempRotX == face.getXHeadRotation () || StartManager.boolSeby) {

								if (!isSendPause) {
										Object[] objects = FindObjectsOfType (typeof(GameObject));
										foreach (GameObject go in objects) {
												go.SendMessage ("OnPauseGame", SendMessageOptions.DontRequireReceiver);
										}
										anim.SetTrigger ("PauseGame");
										isSendPause = true;
								} else {
										isSendStart = false;
								}
						} else {
								if (!isSendStart) {
										anim.SetTrigger ("PlayingGame");
										Object[] objects = FindObjectsOfType (typeof(GameObject));
										foreach (GameObject go in objects) {
												go.SendMessage ("OnResumeGame", SendMessageOptions.DontRequireReceiver);
										}
										isSendStart = true;
								} else {
										isSendPause = false;
								}
						}
						tempRotX = face.getXHeadRotation ();
				}
		}
}