using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] GameObject deathScreen;
    bool dead = false;
    private void OnEnable()
    {
        PlayerController.onPlayerDeath += OnDeath;
    }
    private void OnDisable()
    {
        PlayerController.onPlayerDeath -= OnDeath;
    }
    private void Update()
    {
        if (dead && Input.GetKeyDown(KeyCode.R))
            RestartScene();
    }
    void OnDeath()
    {
        deathScreen.SetActive(true);
        dead = true;
    }
    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
