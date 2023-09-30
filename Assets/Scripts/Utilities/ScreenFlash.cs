/*
 *	Script that makes the screen flash red when the player takes damage
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ScreenFlash : MonoBehaviour {
	private Image _image = null;
	private Coroutine _currentFlashRoutine = null;
	private int numOfFlash = 3;

	private void Awake() {
		_image = GetComponent<Image>();
	}

	public void StartFlash(float secondsForOneFlash, float maxAlpha, Color newColor) {
		_image.color = newColor;

		maxAlpha = Mathf.Clamp(maxAlpha, 0, 1);

		if ( _currentFlashRoutine != null ) {
			StopCoroutine(_currentFlashRoutine);
		}

		_currentFlashRoutine = StartCoroutine(Flash(secondsForOneFlash, numOfFlash, maxAlpha));
		
	}

	IEnumerator Flash(float secondForOneFlash, int numOfFlash, float maxAlpha) {
		for (int i = 0; i < numOfFlash; i++) {
			float flashInDuration = secondForOneFlash / 2;
			for ( float t = 0; t <= flashInDuration; t += Time.deltaTime ) {
				Color colorThisFrame = _image.color;
				colorThisFrame.a = Mathf.Lerp(0, maxAlpha, t / flashInDuration);
				_image.color = colorThisFrame;
				yield return null;
			}
			
			float flashOutDuration = secondForOneFlash / 2;
			for ( float t = 0; t <= flashInDuration; t += Time.deltaTime ) {
				Color colorThisFrame = _image.color;
				colorThisFrame.a = Mathf.Lerp(maxAlpha, 0, t / flashOutDuration);
				_image.color = colorThisFrame;
				yield return null;
			}
		}
		
		
		// Ensure the end alpha is set to 0
		_image.color = new Color32(0, 0, 0, 0);
	}
}
