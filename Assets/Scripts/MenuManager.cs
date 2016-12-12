using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    private bool paused = false;
    private Canvas menu;

    void Start() {
        menu = GetComponentInParent<Canvas>();
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

	public void LoadLevel (string levelName) {
        SceneManager.LoadScene(levelName);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void PauseGame () {
        Time.timeScale = 0f;
        menu.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        paused = true;
    }

    public void ResumeGame () {
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
