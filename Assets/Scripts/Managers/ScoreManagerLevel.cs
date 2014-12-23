using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManagerLevel : MonoBehaviour
{
		Text text;

		void Awake ()
		{
				text = GetComponent <Text> ();
		}

		void Update ()
		{
				if (StartManager.pressed) {
						if (ScoreManager.level <= 3)
								text.text = "Level " + ScoreManager.level;
						else
								text.text = "";
				} else {
						text.text = @"In order to move Your character move Your head up and down or turn it left or right.
The goal of the game is to catch all the creatures.
To do this you have to get near the monster and smile.
Enemy can attack you if you are close enough and not smilling.
Press space key to continue.";
				}
		}
}
