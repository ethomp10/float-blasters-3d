using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

	private bool paused = false;
	private Canvas menu;
	private Canvas playerHUD;
	private Weapon playerWeapon;

	void Start ()
	{
		menu = GetComponentInParent<Canvas> ();
		FindPlayer ();
	}

	void Update ()
	{
		if (Input.GetButtonDown ("Pause") && SceneManager.GetActiveScene ().name != "Menu") {
			if (!paused) {
				PauseGame ();
			} else {
				ResumeGame ();
			}
		}
	}

	void FindPlayer ()
	{
		if (SceneManager.GetActiveScene ().name == "BoulderSystem") {
			playerWeapon = GameObject.FindGameObjectWithTag ("PlayerWeapon").GetComponent<Weapon> ();
			playerHUD = GameObject.FindGameObjectWithTag ("PlayerHUD").GetComponent<Canvas> ();
		}
	}

	// Button functions
	public void LoadLevel (string levelName)
	{
		SceneManager.LoadScene (levelName);
		FindPlayer ();
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void PauseGame ()
	{
		Time.timeScale = 0f;
		paused = true;

		playerHUD.enabled = false;
		playerWeapon.canFire = false;
		foreach (LineRenderer crosshair in playerWeapon.crosshairs) {
			crosshair.enabled = false;
		}

		menu.enabled = true;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void ResumeGame ()
	{
		playerWeapon.StartCoroutine (playerWeapon.CapFireRate ());
		foreach (LineRenderer crosshair in playerWeapon.crosshairs) {
			crosshair.enabled = true;
		}

		playerHUD.enabled = true;
		menu.enabled = false;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		Time.timeScale = 1f;
		paused = false;
	}

	public void QuitGame ()
	{
		StopAllCoroutines ();
		Application.Quit ();
	}
}
