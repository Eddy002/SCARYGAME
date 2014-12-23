using UnityEngine;
using System.Collections;

public class StartManager : MonoBehaviour
{
		private Animator anim;
		private float startTime = 0f;
		private float endTime = 2f;
		private Object[] objects;
		public static bool pressed = false;

		// much relevant
		/* here --> */
		public static bool boolSeby = false; // <-- zmienna na zyczenie
		// such important


		void Awake ()
		{
				anim = GetComponent<Animator> ();
				objects = FindObjectsOfType (typeof(GameObject));
		}
	
		void Update ()
		{
				if (pressed) {
						if (ScoreManager.isLevelCompleted) {
								startTime = 0f;
								foreach (GameObject go in objects) {
										go.SendMessage ("StartNewLevel", SendMessageOptions.DontRequireReceiver);
								}
						}
		
						if (startTime < endTime) {
								startTime += Time.deltaTime;
								anim.SetTrigger ("StartGame");
								foreach (GameObject go in objects) {
										go.SendMessage ("OnPauseGame", SendMessageOptions.DontRequireReceiver);
								}
								boolSeby = true;
						} else {
								anim.SetTrigger ("StartGame1");
								foreach (GameObject go in objects) {
										go.SendMessage ("OnResumeGame", SendMessageOptions.DontRequireReceiver);
								}
								boolSeby = false;
						}
				} else if (Input.GetKeyDown ("space")) {
						pressed = true;
				} else {
						foreach (GameObject go in objects) {
								go.SendMessage ("OnPauseGame", SendMessageOptions.DontRequireReceiver);
						}
				}
		}
}