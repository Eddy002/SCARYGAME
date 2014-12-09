using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    public float timeBetweenAttacks = 1.0f;
    public int attackDamage = 2;
	public float happinessFrom = 0.4f;
	public float happinessTo = 1f;


    Animator anim;
    GameObject player;
    PlayerHealth playerHealth;
    EnemyHealth enemyHealth;
    bool playerInRange;
    float timer;

	GameObject faceController;
	fs.FaceshiftLive face;

	//HappyDetector blabla;


    void Awake ()
    {
        player = GameObject.FindGameObjectWithTag ("Player");
        playerHealth = player.GetComponent <PlayerHealth> ();
        enemyHealth = GetComponent<EnemyHealth>();
        anim = GetComponent <Animator> ();
		//blabla = GameObject.FindGameObjectWithTag ("HappyDetector");
		faceController = GameObject.Find ("FaceController");
		face = faceController.GetComponent <fs.FaceshiftLive> ();
    }


    void OnTriggerEnter (Collider other)
    {
        if(other.gameObject == player)
        {
            playerInRange = true;
        }
    }


    void OnTriggerExit (Collider other)
    {
        if(other.gameObject == player)
        {
            playerInRange = false;
        }
    }


    void Update ()
    {
        //timer += Time.deltaTime;
		Debug.Log("|sdfgh");
        if(/*timer >= timeBetweenAttacks &&*/ playerInRange && enemyHealth.currentHealth > 0)
        {
			if(face.getHappiness() >=  happinessFrom && face.getHappiness() <= happinessTo)
			{
				enemyHealth.currentHealth = 0;
				// stworek powinien umrzec i dac playerowi punkty
			}
			else
			{
				Attack ();
			}
        }


		/*
		 * if in range then check smile status
		 * 
		 */

    }


    void Attack ()
    {
        timer = 0f;

        if(playerHealth.currentHealth > 0)
        {
            playerHealth.TakeDamage (attackDamage);
        }
    }
}
