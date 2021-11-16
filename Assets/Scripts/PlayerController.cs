using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public void GameOver()
    {
        SceneManager.LoadScene("GameOverScene");
    }
}
