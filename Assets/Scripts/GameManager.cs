
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	private static GameManager instance;
	public static GameManager Instance { get { return instance; }}

	public static bool GameIsPaused = false;
	public GameObject pauseMenuUI;

	public GameStructure structure;
	public GameState state;
	public static bool GameHasEnded = false;
	public GameObject gameOverUI;
	public static event Action<GameStructure> GameStructureChanged;
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
	private Coroutine messageCoroutine;
    private bool logNPC;
	//public GameObject NPCLoggingToggleUI;
	public TextMeshProUGUI NPCLoggingUI;


	private void Awake() {
		//Singleton method
		if ( instance == null ) {
			//First run, set the instance
			instance = this;
			instance.structure = useFAM ? GameStructure.FAM : GameStructure.FSM;
			instance.state = GameState.Play;
			
			// DontDestroyOnLoad(gameObject);
 
		}
	}

	private void Start() {
		//UpdateGameState(instance.state);
		FAMToggleUI.GetComponent<Toggle>().isOn = useFAM;
		easyModeToggleUI.GetComponent<Toggle>().isOn = easyMode;
		gameMessagesText = gameMessagesUI.GetComponent<TextMeshProUGUI>();
	}

	private void Update() {
		if ( Input.GetKeyDown(KeyCode.Escape) ) {
			Pause();
		}
	}

	public void UpdateGameStructure(GameStructure newStructure) {
		// If we are already in the state, do nothing
		if (structure == newStructure) {
			return;
		}

		if (!Enum.IsDefined(typeof(GameStructure), newStructure)) {
			Debug.LogError("Invalid game state: " + newStructure);
			return;
		}

		structure = newStructure;

		GameStructureChanged?.Invoke(structure);
	}

	public void UpdateGameState(GameState newState) {
		// If we are already in the state, do nothing
		if (state == newState) {
			return;
		}

		if (!Enum.IsDefined(typeof(GameState), newState)) {
			Debug.LogError("Invalid game state: " + newState);
			return;
		}

		state = newState;

		GameStateChanged?.Invoke(state);
	}

	public void GameOver() {
		if ( GameHasEnded == false ) {
			GameHasEnded = true;
			UpdateGameState(GameState.Pause);
			gameOverUI.SetActive(true);
			Time.timeScale = 0f;
			AudioListener.pause = true;
		}
	}

	// Restart game if player clicks on restart button
	public void RestartGame() {
		GameHasEnded = false;
		gameOverUI.SetActive(false);
		Time.timeScale = 1f;
		AudioListener.pause = false;
		UpdateGameState(GameState.Play);
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
	
	public void QuitGame() {
		Application.Quit();
	}

	public void SetUseFAM(bool value) {
		useFAM = value;
		if ( useFAM ) {
			UpdateGameStructure(GameStructure.FAM);
		} else {
			UpdateGameStructure(GameStructure.FSM);
		}
	}

	public void SetEasyMode(bool value) {
		easyMode = value;
	}

	public void SetNPCLogging(bool value) {
		logNPC = value;
	}

	public void SetNPCLoggingText(string text) {
		NPCLoggingUI.text = text;
	}

	public void Pause() {
		if( !GameIsPaused && !GameHasEnded ) {
			UpdateGameState(GameState.Pause);
			pauseMenuUI.SetActive(true);
			Time.timeScale = 0f;
			GameIsPaused = true;
			AudioListener.pause = true;
		}
	}

	public void Resume() {
		if ( GameIsPaused ) {
			UpdateGameState(GameState.Play);
			pauseMenuUI.SetActive(false);
			Time.timeScale = 1f;
			GameIsPaused = false;
			AudioListener.pause = false;
		}
	}

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

public enum GameStructure {
	FSM,
	FAM
}

public enum GameState {
	Play,
	Pause
}


/*

*/