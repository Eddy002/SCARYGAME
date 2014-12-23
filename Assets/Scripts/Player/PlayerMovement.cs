using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float speed = 6f;            // The speed that the player will move at.
	
	Vector3 movement;                   // The vector to store the direction of the player's movement.
	int floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.

	void Awake () {
		floorMask = LayerMask.GetMask ("Floor");
	}
	
	public bool paused;
	void OnPauseGame () { paused = true; }
	void OnResumeGame () { paused = false; }

	void FixedUpdate () {if (!paused) {
		// Store the input axes.
		float h = Input.GetAxisRaw ("Horizontal");
		float v = Input.GetAxisRaw ("Vertical");
		
		// Move the player around the scene.
		Move (h, v);
	}}
	
	void Move (float h, float v) {
		// Set the movement vector based on the axis input.
		movement.Set (h, 0f, v);
		
		// Normalise the movement vector and make it proportional to the speed per second.
		movement = movement.normalized * speed * Time.deltaTime;

	}
}