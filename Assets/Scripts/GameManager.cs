
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
	public GameObject FAMtoggleUI;
	public bool easyMode = true;
	public GameObject easyModeToggleUI;

	private void Awake() {
		//Singleton method
		if ( instance == null ) {
			//First run, set the instance
			instance = this;
			instance.state = GameState.Play;
			
			DontDestroyOnLoad(gameObject);
 
		} else if ( instance != this ) {
			//Instance is not the same as the one we have, destroy old one, and reset to newest one
			GameState gs = instance.state;
			instance = this;
			instance.state = gs;
			/*
			GameDifficulty gd = Instance.gameDifficulty;
			//Debug.Log(gs);
			Destroy(Instance.gameObject);
			Instance = this;
			Instance.state = gs;
			Instance.gameDifficulty = gd;
			Debug.Log(gameDifficulty.difficulty);
			*/
			DontDestroyOnLoad(gameObject);
		}
	}

	private void Start() {
		UpdateGameState(instance.state);
		FAMtoggleUI.GetComponent<Toggle>().isOn = useFAM;
		easyModeToggleUI.GetComponent<Toggle>().isOn = easyMode;
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
			default:
				throw new ArgumentOutOfRangeException();
		}

		if ( GameStateChanged != null ) GameStateChanged.Invoke(newState);
	}

	private IEnumerator HandleGameWin() {
		if ( gameHasEnded == false ) {
			gameHasEnded = true;
			yield return new WaitForSeconds(5f);
			//Victory();
		}
	}

	private void HandleGameLose() {
		
		if ( gameHasEnded == false ) {
			gameHasEnded = true;
			//Defeat();
		}
	}

	// Restart game if player clicks on restart button
	public void RestartGame() {
		gameHasEnded = false;
		UpdateGameState(GameState.Play);
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
}

public enum GameState {
	Play,
	Pause
}


/*

*/