using UnityEngine;
using System.Collections;
using UnityEngine.Audio;


[RequireComponent (typeof (AudioSource))]
public class MicrophoneController : MonoBehaviour {

	private AudioSource aSource;
	public int samples = 1024;
	private int maxFrequency = 44100;
	private int minFrequency = 0;
	public bool mute = true; //delete and implement audio mixer
	public float loudness;
	private float loudnessMultiplier = 10.0f; //Multiply loudness with this number

	private float[] fftSpectrum;
	public bool useFFT = false;

	private bool isMicrophoneReady = false;
	private AudioMixer aMixer;

	IEnumerator Start () {
		aSource = this.GetComponent<AudioSource> ();

		aMixer = Resources.Load ("MicrophoneMixer") as AudioMixer;		
		if(mute){
			aMixer.SetFloat("MicrophoneVolume",-80);
		}
		else{
			aMixer.SetFloat("MicrophoneVolume",0);
		}

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
			if (useFFT){
				calculateSpectrumData();
			}
		}
	}

	void prepareMicrophone(){
		Debug.Log ("Output sample rate: " + AudioSettings.outputSampleRate);
		if (Microphone.devices.Length > 0){
			Microphone.GetDeviceCaps(Microphone.devices[0], out minFrequency, out maxFrequency);//Gets the maxFrequency and minFrequency of the device
			if (maxFrequency == 0){//These 2 lines of code are mainly for windows computers
				maxFrequency = 44100;
			}
			aSource.clip = Microphone.Start(Microphone.devices[0], true, 1, AudioSettings.outputSampleRate);
			aSource.loop = true;

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
			sum += Mathf.Pow(microphoneData[i],2);//Mathf.Abs(microphoneData[i]);
		}

		return Mathf.Sqrt(sum/samples)*loudnessMultiplier;
	}

	void calculateSpectrumData(){
		float[] spectrum = new float[samples];
		aSource.GetSpectrumData(spectrum, 0, FFTWindow.Hamming);
		fftSpectrum = spectrum;

		SendMessage ("analyzeSpectrumData", fftSpectrum);
	}
}
