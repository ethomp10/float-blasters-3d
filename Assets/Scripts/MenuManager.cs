using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    private bool paused = false;
    private Canvas menu;
    private Weapon playerWeapon;

    void Start() {
        menu = GetComponentInParent<Canvas>();
        FindPlayerWeapon();
    }

    void Update() {
        if (Input.GetButtonDown("Pause") && SceneManager.GetActiveScene().name != "Menu") {
            if (!paused) {
                PauseGame();
            } else {
                ResumeGame();
            }
        }
    }

    void FindPlayerWeapon() {
        if (SceneManager.GetActiveScene().name == "BoulderSystem") {
            playerWeapon = GameObject.FindGameObjectWithTag("PlayerWeapon").GetComponent<Weapon>();
        }
    }

    // Button functions
	public void LoadLevel (string levelName) {
        SceneManager.LoadScene(levelName);
        FindPlayerWeapon();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    public void PauseGame () {
        Time.timeScale = 0f;
        paused = true;

        playerWeapon.canFire = false;
        foreach (LineRenderer crosshair in playerWeapon.crosshairs) {
            crosshair.enabled = false;
        }

        menu.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame () {
        playerWeapon.canFire = true;
        foreach (LineRenderer crosshair in playerWeapon.crosshairs) {
            crosshair.enabled = true;
        }

        menu.enabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Time.timeScale = 1f;
        paused = false;
    }

    public void QuitGame () {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}
