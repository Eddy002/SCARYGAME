using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerMana : MonoBehaviour {

	public Slider manaSlider;
	public bool isPlayerSmiling = false;
	public float playerHappiness = 0f;
	public bool paused;

	private float timeBetweenRegen = 0.25f; // mana is regenerating per every 0.25s
	private int manaRegen = 10;
	private int manaDepletion = 12;
	private float timer = 0f;

	GameObject faceController;
	fs.FaceshiftLive face;

	// Use this for initialization
	void Start () {
		faceController = GameObject.Find ("FaceController");
		face = faceController.GetComponent <fs.FaceshiftLive> ();
	}
	

	void OnPauseGame () { paused = true; }
	void OnResumeGame () { paused = false; }
	
	void Update () {
		if (paused == false) {
			playerHappiness = face.getHappiness ();

			if (playerHappiness > 0.25f)
				isPlayerSmiling = true;
			else
				isPlayerSmiling = false;

			timer += Time.deltaTime;
			if (timer >= timeBetweenRegen) {
				timer = 0f;
				if (manaSlider.value < 100)
					manaSlider.value += manaRegen;

				if (isPlayerSmiling) {
					if (manaSlider.value >= manaDepletion)
						manaSlider.value -= manaDepletion;
					else
						manaSlider.value = 0;
				}
			}
		}
	}
}
