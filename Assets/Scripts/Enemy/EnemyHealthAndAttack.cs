using UnityEngine;
using System.Collections;

public class EnemyHealthAndAttack : MonoBehaviour {

	public float timeBetweenAttacks = 1.0f;
	public int attackDamage = 4;
	public float happinessFrom = 0.5f;
	public float happinessTo = 1f;
	public int startingHealth = 100;
	public int currentHealth;
	public float sinkSpeed = 2.5f;
	public int scoreValue = 10;
	public AudioClip deathClip;
	
	Animator anim;
	GameObject player;
	PlayerHealth playerHealth;
	EnemyHealth enemyHealth;
	bool playerInRange;
	float timer;

	AudioSource enemyAudio;
	ParticleSystem hitParticles;
	CapsuleCollider capsuleCollider;
	bool isDead;
	bool isSinking;

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

		enemyAudio = GetComponent <AudioSource> ();
		hitParticles = GetComponentInChildren <ParticleSystem> ();
		capsuleCollider = GetComponent <CapsuleCollider> ();
		
		currentHealth = startingHealth;

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
	
	// Update is called once per frame
	void Update ()
	{
		if(isSinking)
		{
			transform.Translate (-Vector3.up * sinkSpeed * Time.deltaTime);
		}

		//timer += Time.deltaTime;
		if(/*timer >= timeBetweenAttacks &&*/ playerInRange && enemyHealth.currentHealth > 0)
		{
			if(face.getHappiness() >=  happinessFrom && face.getHappiness() <= happinessTo)
			{
				TakeDamage(9999);
			}
			else
			{
				Attack ();
			}
		}
	}
	
	
	void Attack ()
	{
		timer = 0f;
		
		if(playerHealth.currentHealth > 0)
		{
			playerHealth.TakeDamage (attackDamage);
		}
	}

	public void TakeDamage (int amount/*, Vector3 hitPoint*/)
	{
		if(isDead)
			return;
		
		enemyAudio.Play ();
		
		currentHealth -= amount;
		
		//hitParticles.transform.position = hitPoint;
		//hitParticles.Play();
		
		if(currentHealth <= 0)
		{
			Death ();
		}
	}
	
	
	void Death ()
	{
		isDead = true;
		
		capsuleCollider.isTrigger = true;
		
		anim.SetTrigger ("Dead");
		
		enemyAudio.clip = deathClip;
		enemyAudio.Play ();
	}
	
	
	public void StartSinking ()
	{
		GetComponent <NavMeshAgent> ().enabled = false;
		GetComponent <Rigidbody> ().isKinematic = true;
		isSinking = true;
		ScoreManager.score += scoreValue;
		Destroy (gameObject, 2f);
	}
}
