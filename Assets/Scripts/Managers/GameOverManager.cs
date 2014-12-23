using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public PlayerHealth playerHealth;
    Animator anim;
	public static string text;
    
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (playerHealth.currentHealth <= 0)
        {
			text = "Game Over!";
            anim.SetTrigger("GameOver");
        }
		if(ScoreManager.uWin) {
			Object[] objects = FindObjectsOfType (typeof(GameObject));
			foreach (GameObject go in objects) {
				go.SendMessage ("OnPauseGame", SendMessageOptions.DontRequireReceiver);
			}
			text = "You win the game!";
			anim.SetTrigger("Win");
		}
    }
}
