using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnPlayButtonPressed()
    {
         SceneManager.LoadScene("MainScene");
    }
}
