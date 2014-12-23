using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WinTextManager : MonoBehaviour
{
    Text text;

    void Awake ()
    {
        text = GetComponent <Text> ();
    }

    void Update ()
	{
		text.text = GameOverManager.text;
    }
}
