﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonRetry : MonoBehaviour
{
    public void Retry()
    {
        SceneManager.LoadScene("MainScene");
    }
}