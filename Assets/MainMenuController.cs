using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] string gameScene = "GameScene";
    [SerializeField] string tutorialScene = "TutorialScene";
    [SerializeField] string mainMenuScene = "MainMenu";
    [SerializeField] bool forceRes = false;
    [SerializeField] TMP_Text autoRunOnOFF;



    [SerializeField] SettingsObject settings;

    private void Start()
    {
        if (forceRes)
            Screen.SetResolution(640, 360, FullScreenMode.FullScreenWindow);

        autoRunOnOFF.text = settings.autoRun ? "ON" : "OFF";
    }
    public void LoadGameScene()
    {
        SceneManager.LoadScene(gameScene);
    }

    public void LoadTutorialScene()
    {
        SceneManager.LoadScene(tutorialScene);
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
    
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void SetAspect43() { settings.aspectRatio = AspectRatio.A4_3; }
    public void SetAspect169() { settings.aspectRatio = AspectRatio.A16_9; }
    public void SetAspect219() { settings.aspectRatio = AspectRatio.A21_9; }
    public void ToggleAutoRun() 
    { 
        settings.autoRun = !settings.autoRun;
        autoRunOnOFF.text = settings.autoRun ? "ON" : "OFF";
    }
    public void TurnOnCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


}

