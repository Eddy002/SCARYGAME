using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static int score;
	public static int level = 1;
	public static bool isLevelCompleted;
	public static bool uWin = false;

	private int scoreNeededToFinish = 60;
	
    Text text;


    void Awake ()
    {
        text = GetComponent <Text> ();
        score = 0;
    }

    void Update ()
    {
		if(score == scoreNeededToFinish)
		{
			Debug.LogError("1");
			isLevelCompleted = true;
			score = 0;
		}

        text.text = "Score: " + score;
    }

	void StartNewLevel() 
	{
		if(level < 3)
			level++;
		isLevelCompleted = false;
		uWin = false;
		if(level == 2)
		{
			scoreNeededToFinish = 120;
		}
		else if(level == 3)
		{
			scoreNeededToFinish = 180;
		} else {
			uWin = true;
		}
	}
}
