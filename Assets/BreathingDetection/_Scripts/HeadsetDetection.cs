using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class HeadsetDetection : MonoBehaviour {

	private string[] microphones;
	private Text micText;
	// Use this for initialization
	void Start () {
		micText = GameObject.Find ("MicText").GetComponent<Text> ();
		CheckMic ();
	}

	public void CheckMic(){
		microphones = Microphone.devices;
		micText.text = "";
		foreach (string	 str in microphones){
			micText.text += str + "\n";
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
