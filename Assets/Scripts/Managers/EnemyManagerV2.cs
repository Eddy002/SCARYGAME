using UnityEngine;

public class EnemyManagerV2 : MonoBehaviour
{
	public int numEnemyToSpawn;
	public GameObject enemy;
	public Transform[] spawnPoints;

	int level = 1;

	//public int numEnemyLevelOne = 5; // 3 x 20 = 60 pkt
	//public int numEnemyLevelTwo = 7; // 6 x 20 = 120 pkt
	//public int numEnemyLevelThree = 10; // 9 * 20 = 180 plt
	
	void Start ()
	{
		InitializeLevel (1);
	}

	void Update()
	{

	}

	void InitializeLevel(int enemyNum)
	{
		for (int i = 0; i < enemyNum; i++) {
		
			int spawnPointIndex = Random.Range (0, spawnPoints.Length);
		
			Vector3 position = new Vector3(
				spawnPoints [spawnPointIndex].position.x + Random.Range(-3, 3),
				spawnPoints [spawnPointIndex].position.y + Random.Range(-3, 3));
		
			Instantiate (enemy, position, spawnPoints [spawnPointIndex].rotation);
		}
	}

	void StartNewLevel () 
	{
		if(level < 3)
			level++;
		switch(level)
		{
			case 2:
			{
				InitializeLevel(2);
				break;
			}
			case 3:
			{
				InitializeLevel(3);
				break;
			}
		}
	}
}
