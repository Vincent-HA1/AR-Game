using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Start_Game : MonoBehaviour
{
    //Run from the play button
    //Starts the game
    public void StartGame()
    {
        GameManager.gameStart = true;
        gameObject.SetActive(false);
    }
}
