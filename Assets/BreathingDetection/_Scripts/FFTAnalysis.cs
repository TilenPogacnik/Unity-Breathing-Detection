using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class FFTAnalysis : MonoBehaviour {

	[Header("FFT variables")]
	[Space(10f)]

	public float fftSumThreshold = 0.0005f; //threshold that fftSum must exceed to consider the sound to be breathing
	private float fftSum = 0f;

	private bool exhalePossible = false;

	//Debugging vars
	public LineRenderer lRend; 
	
	/*
	 * Analyse FFT data
	 */
	void analyzeSpectrumData(float[] fftSpectrum){
		//Calculate sum of smoothed FFT data
		fftSum = 0.0f;


		foreach (float f in fftSpectrum) {
			fftSum += f;
		}


		//Debug.Log ("FFTSum: " + fftSum);
		if (fftSum > fftSumThreshold) {
			exhalePossible = true;
		} else {
			exhalePossible = false;
		}
	}

	public bool GetExhalePossible(){
		return exhalePossible;
	}

	public float GetFFTSum(){
		return fftSum;
	}

}
