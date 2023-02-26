using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public enum AspectRatio
{
    A4_3,
    A16_9,
    A21_9
}
public class GameManager : MonoBehaviour
{
    [SerializeField] int totalSeconds = 5;
    float startTime;
    [SerializeField] TMP_Text minutesText;
    [SerializeField] TMP_Text secondsText;
    [SerializeField] TMP_Text Survive;
    [SerializeField] float surviveTime;
    [SerializeField] float musicBPM = 190;

    [SerializeField] Vector2 spawnRange = new Vector2(50,70);
    [SerializeField] GameObject smallCat;
    [SerializeField] GameObject bigCat;
    [SerializeField] GameObject droneCat;

    [SerializeField] MeshRenderer forcefield;
    Color forcefieldColor;

    float smallSpawnTime;
    float bigSpawnTime;
    float droneSpawnTime;


    [SerializeField] GameObject winScreen;

    public int bar;

    bool playerDead;

    [SerializeField] SettingsObject settings;

    void Start()
    {
        startTime = Time.time;
        Invoke("TurnOffSurvive", surviveTime);
    }
    private void OnEnable()
    {
        PlayerController.onPlayerDeath += PlayerDied;
    }
    private void OnDisable()
    {
        PlayerController.onPlayerDeath -= PlayerDied;
    }
    void PlayerDied() { playerDead = true; }

    void Update()
    {
        HandleTimer();
        HandleSpawning();

        forcefield.material.SetColor("_Color", forcefieldColor);
    }
    void HandleTimer()
    {
        if (playerDead)
            return;

        float timePassed = Time.time - startTime;
        float secondsRemaining = Mathf.Floor(totalSeconds - timePassed);
        secondsRemaining = Mathf.Clamp(secondsRemaining, 0, float.MaxValue);

        if (secondsRemaining == 0)
            Win();

        int minutes = Mathf.FloorToInt(secondsRemaining / 60);
        int seconds = Mathf.FloorToInt(secondsRemaining - minutes * 60);

        string minString = minutes >= 10 ? minutes.ToString() : "0" + minutes.ToString();
        string secString = seconds >= 10 ? seconds.ToString() : "0" + seconds.ToString();

        minutesText.text = minString;
        secondsText.text = secString;

    }
    void TurnOffSurvive()
    {
        Survive.gameObject.SetActive(false);
    }

    void Win()
    {
        winScreen.SetActive(true);
        foreach (Transform child in transform)        
            Destroy(child.gameObject);
        
        Debug.Log("YOU WIN!");
    }

    void HandleSpawning()
    {
        float timePassed = Time.time - startTime;
        bar = Mathf.FloorToInt((musicBPM / 60) * timePassed / 4) + 1;

        if (bar <= 1)
        {
            forcefieldColor = Color.black;
            SpawnEnemy(smallCat, 5, 100, ref smallSpawnTime);
        }
        else if (bar < 9)
        {
            SpawnEnemy(smallCat, 1, 5, ref smallSpawnTime);
        }
        else if (bar < 33)
        {
            SpawnEnemy(smallCat, 1, 3, ref smallSpawnTime);
        }
        else if (bar < 37)
        {
            forcefieldColor = new Color (0.8f, 0, 0, 1);
            SpawnEnemy(bigCat, 1, 10, ref bigSpawnTime);
        }
        else if (bar < 54)
        {
            SpawnEnemy(bigCat, 1, 5, ref bigSpawnTime);
        }
        else if (bar < 66)
        {
            forcefieldColor = new Color(0.9f, 0.9f, 0.9f, 1);
            SpawnEnemy(droneCat, 1, 5, ref droneSpawnTime);
        }
        else if (bar < 82)
        {
            SpawnEnemy(droneCat, 1, 5, ref droneSpawnTime);
            SpawnEnemy(smallCat, 1, 3, ref smallSpawnTime);
        }
        else if (bar < 98)
        {
            SpawnEnemy(droneCat, 1, 10, ref droneSpawnTime);
            SpawnEnemy(smallCat, 1, 3, ref smallSpawnTime);
        }
        else if (timePassed < 128)
        {
            forcefieldColor = new Color(0f, 0.3f, 0.9f, 1);
            SpawnEnemy(smallCat, 1, 3, ref smallSpawnTime);
        }
        else if (timePassed < 140)
        {
            SpawnEnemy(smallCat, 1, 3, ref smallSpawnTime);
            SpawnEnemy(bigCat, 1, 5, ref bigSpawnTime);
        }
        else
        {
            SpawnEnemy(smallCat, 1, 3, ref smallSpawnTime);
            SpawnEnemy(bigCat, 1, 5, ref bigSpawnTime);
            SpawnEnemy(droneCat, 1, 5, ref droneSpawnTime);
        }
    }
    void SpawnEnemy(GameObject _enemy, int _amount, float spawnPeriod, ref float spawnTime)
    {
        if (Time.time - spawnTime < spawnPeriod)
            return;

        spawnTime = Time.time;
        for (int i = 0; i < _amount; i++)
        {
            Instantiate(_enemy, GetSpawnPosition(), Quaternion.identity, transform);
        }
    }
    Vector3 GetSpawnPosition()
    {
        var pos = new Vector3(Random.Range(-spawnRange.y, spawnRange.y), 0, Random.Range(-spawnRange.y, spawnRange.y));
        //if spawn is too close, try again
        if (pos.magnitude < spawnRange.x)
            return GetSpawnPosition();
        return pos;
    }
}
