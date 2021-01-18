using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneUIManager : Singleton<MainSceneUIManager>
{
    public GameObject Panel;
    public Text Time;
    public Text Score;


    public void Initialize()
    {
        if (Panel.activeSelf)
            Panel.SetActive(false);

        GameManager.Instance.Car.Finished += Car_Finished;
    }

    private void Car_Finished()
    {
        Panel.SetActive(true);
        Time.text += GameManager.Instance.RaceTime.ToString();
        Score.text += GameManager.Instance.Score.ToString();
    }

    public void OnClickRestart()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OnClickMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }
}
