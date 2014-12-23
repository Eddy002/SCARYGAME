using UnityEngine;
using System.Collections;

public class EnemyHealthAndAttack : MonoBehaviour {

	public float timeBetweenAttacks = 0.1f;
	public int attackDamage = 4;
	float happinessFrom = 0.1f;
	float happinessTo = 1f;
	public int startingHealth = 100;
	public int currentHealth;
	public float sinkSpeed = 4.5f;
	int scoreValue = 20;
	public AudioClip deathClip;
	public bool isAgressive;

	Animator anim;
	GameObject player;
	PlayerHealth playerHealth;
	bool playerInRange;
	float timer = 0f;
	float distanceToPlayer;

	public PlayerMana mana; 

	AudioSource enemyAudio;
	ParticleSystem hitParticles;
	CapsuleCollider capsuleCollider;
	bool isDead;
	bool isSinking;

	GameObject faceController;
	fs.FaceshiftLive face;


	void Awake ()
	{
		player = GameObject.FindGameObjectWithTag ("Player");
		playerHealth = player.GetComponent <PlayerHealth> ();
		anim = GetComponent <Animator> ();

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
	
	protected bool paused;
	void OnPauseGame () { paused = true; }
	void OnResumeGame () { paused = false; }
	
	void Update () { if (!paused) {
		if (isSinking) {
			transform.Translate (-Vector3.up * sinkSpeed * Time.deltaTime);
		}

		timer += Time.deltaTime;
		if (timer >= timeBetweenAttacks && playerInRange && currentHealth > 0) 
		{
			if ((face.getHappiness () >= happinessFrom) && (face.getHappiness () <= happinessTo)) 
			{
				TakeDamage (40);
				timer = 0f;
			} 
			else 
			{
				if (playerHealth.currentHealth > 0) 
				{
					playerHealth.TakeDamage (attackDamage);
				}
				timer = 0f;
			}
		}
	} }

	public void TakeDamage (int amount)
	{
		if(isDead)
			return;
		
		enemyAudio.Play ();
		
		currentHealth -= amount;

		hitParticles.transform.position = this.transform.position;
		hitParticles.Play();
		
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
