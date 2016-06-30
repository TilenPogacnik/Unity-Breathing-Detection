﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof (MicrophoneController))]
public class NewBreathingDetection : MonoBehaviour {

	public Text stateText;
	public Text varianceText;


	enum Breathing{Inhale, Exhale}; //Two possible breathing states 
	private Breathing currentState = Breathing.Inhale; 

	private MicrophoneController micControl; //microphone controller for loudness and variance calculation

	private float prevLoudness = 0f; //loudness in previous frame
	private float variance = 0f; //variance of current frame
	private float loudnessMultiplier = 10.0f; //We multiply the loudness so the numbers are more natural
	private int varianceUnderThresholdCounter = 0; //counts how many frames the variance spent under threshold

	private bool fastExhalePossible = false;

	[Header("Hysteresis variables")]
	public float exhaleLoudnessThresholdHigh = 0.5f; //determines how loud the exhale needs to be to change state to Exhale
	public float exhaleLoudnessThresholdLow = 0.2f;  ///determines how loud the exhale needs to be to change state to Exhale
	public float inhaleLoudnessThresholdHigh = 0.1f; //determines how silent should the exhale be to change state to Inhale
	public float inhaleLoudnessThresholdLow = 0.05f; //determines how silent should the exhale be to change state to Inhale
	
	public float exhaleVarianceThreshold = 2; //determines how large should the variance be to change state to Exhale
	public float inhaleVarianceThreshold = -3; //determines how silent should the variance be to change state to Inhale

	private FFTAnalysis fftAnalysis;

	public LineRenderer lRend; 

	//test variables
	private List<float> loudnessList = new List<float>();
	private int maxListCount = 10;
	public float minimizedLoudness = 999f;


	void Start () {
		micControl = this.GetComponent<MicrophoneController> ();
		if (micControl == null) {
			Debug.LogError("Cannot find MicrophoneController attached to this object.");
		}

		fftAnalysis = this.GetComponent<FFTAnalysis> ();
		if (fftAnalysis == null) {
			Debug.LogError("Cannot find FFTAnalysis attached to this object.");
		}

	}
	
	void Update () {
		updateVariance ();

		minimizeLoudness ();

		switch (currentState) {
			
			case (Breathing.Inhale):
				checkIfExhaling();
				break;
				
			case (Breathing.Exhale):
				checkIfInhaling();
				break;

			default:
				Debug.Log ("This should never happen.");
				break;
		}
		stateText.text = "Current state: " + currentState.ToString();

	}

	/*
	 * This function checks if all the criteria to transition from Inhale state to Exhale state have been met and then transitions to Exhale state
	 * 
	 * Criteria:
	 * Microphone loudness and variance have to be higher than our thresholds.
	 * OR 
	 * Microphone loudness has to be very loud and variance has to be under threshold for the last X frames
	 * 
	 */
	
	void checkIfExhaling(){
		
		if (currentState == Breathing.Inhale) {
			if (minimizedLoudness > exhaleLoudnessThresholdLow && variance > exhaleVarianceThreshold
			    && (fftAnalysis == null || fftAnalysis.GetExhalePossible())) {
				fastExhalePossible = true;
			}
			
			if ( fastExhalePossible || 
			    (minimizedLoudness > exhaleLoudnessThresholdHigh && varianceUnderThresholdCounter > 8) //ALI moč precej velika && zadnjihnekaj varianc pod thresholdom
			    && (fftAnalysis == null || fftAnalysis.GetExhalePossible())) { 
				varianceUnderThresholdCounter = 0;
				fastExhalePossible = false; 
				
				currentState = Breathing.Exhale; //Change state to exhaling
				BreathingEvents.TriggerOnExhale (); //Trigger onExhale event
			} 
			
		}
	}
	
	
	/*
	 * This function checks if all the criteria to transition from Exhale state to Inhale state have been met and then transitions to Inhale state
	 * 
	 * Criteria:
	 * Microphone loudness and variance have to be lower than our thresholds.
	 * OR 
	 * Microphone loudness has to be much lower than our inhale loudness threshold
	 * 
	 */
	
	void checkIfInhaling(){
		//Moč pod  exhale thresholdom && varianca < inhale threshold
		//ALI loudness občutno pod thresholdom
		if (currentState == Breathing.Exhale &&
		    ((minimizedLoudness < inhaleLoudnessThresholdHigh && variance < inhaleVarianceThreshold) || minimizedLoudness < inhaleLoudnessThresholdLow)
		    && (fftAnalysis == null || !fftAnalysis.GetExhalePossible())) {
			
			currentState = Breathing.Inhale; //Change state to inhaling			
			BreathingEvents.TriggerOnInhale(); //Trigger onInhale event			
		}
	}


	void updateVariance(){
		variance = micControl.loudness - prevLoudness;
		prevLoudness = micControl.loudness;

		varianceText.text = ("Variance: " + Mathf.Round (variance*100.0f)/100.0f);
		
		//update variance counter
		if (variance < exhaleVarianceThreshold) {
			varianceUnderThresholdCounter++;
			
		} else {
			varianceUnderThresholdCounter = 0;
		}
	}

	void minimizeLoudness(){
		loudnessList.Add (prevLoudness);

		if (prevLoudness <= minimizedLoudness) {
			minimizedLoudness = prevLoudness;
		}

		//Remove oldest loudness from list and recalculate minimizedLoudness (only if the oldest loudness is the currentMinimizedLoudness)
		if (loudnessList.Count >= maxListCount) {
			if (loudnessList[0] <= minimizedLoudness){
				//Find new minimizedLoudness
				float min = loudnessList[1];
				for(int i = 1; i < loudnessList.Count; i++){
					if (loudnessList[i] < min){
						min =loudnessList[i];
					}
				}
				minimizedLoudness = min;

			}
			loudnessList.RemoveAt(0);

		} 

		/*string dLog = "";
		for (int i = 0; i < loudnessList.Count; i++) {
			dLog += loudnessList[i] + ", ";
		}
		Debug.Log (dLog);
		Debug.Log("MinimizedLoudness: " + minimizedLoudness + " Past loudness: " + prevLoudness);
		*/
	}
}
