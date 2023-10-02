
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using JohnStairs.RCC.Inputs;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	private static GameManager instance;
	public static GameManager Instance { get { return instance; }}

	public static bool GameIsPaused = false;
	public GameObject pauseMenuUI;

	public GameState state;
	bool gameHasEnded = false;
	public GameObject gameOverUI;
	public static event Action<GameState> GameStateChanged;

	public float sightRange = 2f;
	public float sightAngle = 45f;

	public float steer = 60f; // Maybe make it 30 for avoid behaviour volume
	public float backpedal = 10f;
	public float gas = 3f;
	public float brake = 20f;
	public float brakeAt = 1f; // Check if this can be shared by all behaviours or revert to 5 (1 only for seek)
	public float stopAt = 0.01f;
	public float fleeRange = 5f;

	public bool useFAM = true;
	public GameObject FAMToggleUI;
	public bool easyMode = true;
	public GameObject easyModeToggleUI;
	public GameObject gameMessagesUI;
	private TextMeshProUGUI gameMessagesText;


	private void Awake() {
		//Singleton method
		if ( instance == null ) {
			//First run, set the instance
			instance = this;
			instance.state = GameState.Play;
			
			// DontDestroyOnLoad(gameObject);
 
		}
	}

	private void Start() {
		UpdateGameState(instance.state);
		FAMToggleUI.GetComponent<Toggle>().isOn = useFAM;
		easyModeToggleUI.GetComponent<Toggle>().isOn = easyMode;
		gameMessagesText = gameMessagesUI.GetComponent<TextMeshProUGUI>();
	}

	private void Update() {
		if ( Input.GetKeyDown(KeyCode.Escape) ) {
			Pause();
		}
	}

	public void UpdateGameState(GameState newState) {
		state = newState;

		switch ( newState ) {
			case GameState.Play:
				break;
			case GameState.Pause:
				break;
			case GameState.GameOver:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		if ( GameStateChanged != null ) GameStateChanged.Invoke(newState);
	}

	public void GameOver() {
		if ( gameHasEnded == false ) {
			gameHasEnded = true;
			RPGInputManager.DisableInputActions();
			gameOverUI.SetActive(true);
			Time.timeScale = 0f;
			AudioListener.pause = true;
		}
	}

	// Restart game if player clicks on restart button
	public void RestartGame() {
		gameHasEnded = false;
		RPGInputManager.EnableInputActions();
		gameOverUI.SetActive(false);
		Time.timeScale = 1f;
		AudioListener.pause = false;
		//UpdateGameState(GameState.Play);
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
	
	public void QuitGame() {
		Application.Quit();
	}

	public void SetUseFAM(bool value) {
		useFAM = value;
	}

	public void SetEasyMode(bool value) {
		easyMode = value;
	}


	public void Pause() {
		if( !GameIsPaused ) {
			RPGInputManager.DisableInputActions();
			pauseMenuUI.SetActive(true);
			Time.timeScale = 0f;
			GameIsPaused = true;
			AudioListener.pause = true;
		}
	}

	public void Resume() {
		if ( GameIsPaused ) {
			pauseMenuUI.SetActive(false);
			RPGInputManager.EnableInputActions();
			Time.timeScale = 1f;
			GameIsPaused = false;
			AudioListener.pause = false;
		}
	}

	private Coroutine messageCoroutine;
	public void SetGameMessage(string message) {
		if ( messageCoroutine != null ) {
			StopCoroutine(messageCoroutine);
		}
		messageCoroutine = StartCoroutine(ShowMessage(message, 1f, 1f));
	}

	private IEnumerator ShowMessage(string message, float visibleTime, float fadeTime) {
		gameMessagesText.text = message;
		gameMessagesText.alpha = 1f;
		yield return new WaitForSeconds(visibleTime);
		for ( float t = 0f; t <= fadeTime; t += Time.deltaTime ) {
			gameMessagesText.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
			yield return null;
		}
	}
}

public enum GameState {
	Play,
	Pause,
	GameOver
}


/*

*/