using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton (we make sure this class has only one instance accessible from anywhere)
    public static GameManager Instance;

    // Number of enemies killed
    public int nbEnemiesKilled = 0;

    void Awake()
    {
        Instance = this;
    }

}
