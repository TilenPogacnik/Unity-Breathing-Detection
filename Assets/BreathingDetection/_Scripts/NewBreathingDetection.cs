using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof (MicrophoneController))]
public class NewBreathingDetection : MonoBehaviour {

	public Text stateText;


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
		Debug.Log ("Loudness " + micControl.loudness);
		updateVariance ();

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
			if (micControl.loudness > exhaleLoudnessThresholdLow && variance > exhaleVarianceThreshold
			    && (fftAnalysis == null || fftAnalysis.GetExhalePossible())) {
				fastExhalePossible = true;
			}
			
			if ( fastExhalePossible || 
			    (micControl.loudness > exhaleLoudnessThresholdHigh && varianceUnderThresholdCounter > 8) //ALI moč precej velika && zadnjihnekaj varianc pod thresholdom
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
		    ((micControl.loudness < inhaleLoudnessThresholdHigh && variance < inhaleVarianceThreshold) || micControl.loudness < inhaleLoudnessThresholdLow)
		    && (fftAnalysis == null || !fftAnalysis.GetExhalePossible())) {
			
			currentState = Breathing.Inhale; //Change state to inhaling			
			BreathingEvents.TriggerOnInhale(); //Trigger onInhale event			
		}
	}


	void updateVariance(){
		variance = micControl.loudness - prevLoudness;
		prevLoudness = micControl.loudness;
		
		
		//update variance counter
		if (variance < exhaleVarianceThreshold) {
			varianceUnderThresholdCounter++;
			
		} else {
			varianceUnderThresholdCounter = 0;
		}
	}
}
