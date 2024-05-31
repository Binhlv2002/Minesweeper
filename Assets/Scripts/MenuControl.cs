using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControl : MonoBehaviour
{
    public TextMeshProUGUI winTimeDisplay;
    private void Start()
    {
        // Đọc thời gian từ PlayerPrefs
        float winTime = PlayerPrefs.GetFloat("WinTime", 0f);

        // Hiển thị thời gian lên TextMeshProUGUI
        winTimeDisplay.text = "HIGH SCORE: " + winTime.ToString("F2");
    }
    public void LoadEasyScene()
    {
        SceneManager.LoadScene("Easy");
    }

    public void LoadHardScene()
    {
        SceneManager.LoadScene("Hard");
    }
}
