using UnityEngine;
using System.Collections;


[RequireComponent (typeof (AudioSource))]
public class MicrophoneController : MonoBehaviour {

	private AudioSource aSource;
	public int samples = 1024;
	public int frequency = 44100;
	public bool mute = true; //delete and implement audio mixer
	public float loudness;
	private float loudnessMultiplier = 10.0f; //Multiply loudness with this number

	private float[] fftSpectrum;

	private bool isMicrophoneReady = false;

	IEnumerator Start () {
		aSource = this.GetComponent<AudioSource> ();


		if (Microphone.devices.Length == 0) {
			Debug.LogWarning("No microphone detected.");
		}


		//if using Android or iOS -> request microphone permission
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
			yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

			if (!Application.HasUserAuthorization(UserAuthorization.Microphone)){
				Debug.LogWarning ("Application does not have microphone permission.");
				yield break;
			}
		}

		prepareMicrophone ();
	}
	
	// Update is called once per frame
	void Update () {
		if (isMicrophoneReady) {
			loudness = calculateLoudness();
			calculateSpectrumData();
		}
	}

	void prepareMicrophone(){
		if (Microphone.devices.Length > 0){
			aSource.clip = Microphone.Start(Microphone.devices[0], true, 1, frequency);
			aSource.loop = true;
			aSource.mute = mute;

			//Wait until microphone starts
			while (!(Microphone.GetPosition(Microphone.devices[0]) > 0)){

			}

			aSource.Play();

			isMicrophoneReady = true;

		} else {
				Debug.LogWarning("No microphone detected.");
		}

	}

	float calculateLoudness(){
		float[] microphoneData = new float[samples];
		float sum = 0;

		aSource.GetOutputData (microphoneData, 0);
		for (int i = 0; i < microphoneData.Length; i++) {
			sum += Mathf.Abs(microphoneData[i]);
		}

		return sum/samples*loudnessMultiplier;
	}

	void calculateSpectrumData(){
		float[] spectrum = new float[samples];
		aSource.GetSpectrumData(spectrum, 0, FFTWindow.Hamming);
		fftSpectrum = spectrum;

		SendMessage ("analyzeSpectrumData", fftSpectrum);
	}
}
