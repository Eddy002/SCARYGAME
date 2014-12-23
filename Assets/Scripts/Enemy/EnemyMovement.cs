using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    Transform player;
    PlayerHealth playerHealth;
    EnemyHealthAndAttack enemyHealthAndAttack;
    NavMeshAgent nav;

	float changeMoveTimer;
	float timeBetweenChanges = 0f;

	Transform trans;
	Vector3 currentDestination;


    void Awake ()
    {
        player = GameObject.FindGameObjectWithTag ("Player").transform;
        playerHealth = player.GetComponent <PlayerHealth> ();
        enemyHealthAndAttack = GetComponent <EnemyHealthAndAttack> ();
        nav = GetComponent <NavMeshAgent> ();
		trans = GetComponent<Transform> ();
    }

	protected bool paused;
	void OnPauseGame () { paused = true; }
	
	void OnResumeGame () { paused = false; }
	
	void Update () 
	{

		if (!paused) {
			changeMoveTimer += Time.deltaTime;

			if (enemyHealthAndAttack.currentHealth > 0 && playerHealth.currentHealth > 0) {
				if (changeMoveTimer >= timeBetweenChanges) {
					currentDestination = new Vector3 (Random.Range (-20, 20), 1, Random.Range (-20, 20));
					nav.SetDestination (currentDestination);
					timeBetweenChanges = Random.Range (2, 10);
					changeMoveTimer = 0f;
				}
			} else {
				nav.enabled = false;
			}

			// if destination has been reached find another one
			// also to do is mechanism that somehow avoids obstacles 
			// or force bunnies (or bears or whatever) from ocasional stuck 
			float distance = Vector3.Distance (trans.position, currentDestination);
			if (distance < 1f) {
				timeBetweenChanges = 0f;
			}
		}
	}
}
