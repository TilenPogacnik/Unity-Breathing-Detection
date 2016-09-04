using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(PlayMakerFSM))]
public class BreathingDetectionPlaymakerEvents : MonoBehaviour {

	List<PlayMakerFSM> playmakerFSMs;

	void OnEnable(){
		BreathingEvents.onInhale += HandleOnInhale;
		BreathingEvents.onExhale += HandleOnExhale;
		VideoEvents.onVideoEnded += HandleVideoEnded;
	}


	void OnDisable(){
		BreathingEvents.onInhale -= HandleOnInhale;
		BreathingEvents.onExhale -= HandleOnExhale;
		VideoEvents.onVideoEnded -= HandleVideoEnded;

	}

	void Start () {
		//Find all PlayMakerFSM scripts attached to this GameObject
		playmakerFSMs = this.gameObject.GetComponents<PlayMakerFSM> ().ToList();
		if (playmakerFSMs == null || playmakerFSMs.Count < 1) {
			Debug.LogError (gameObject.name + ": Could not find any objects of type PlayMakerFSM attached to this GameObject"); 
		}
	}
	
	void HandleOnExhale(){
		if (playmakerFSMs != null && playmakerFSMs.Count > 0) {

			//Sends an OnExhale event to all PlaymakerFSM scripts on this object
			foreach (PlayMakerFSM playmakerFSM in playmakerFSMs) {
				playmakerFSM.Fsm.Event ("OnExhale");
			}

		} else {
			Debug.LogWarning("There is no PlayMakerFSM to send the event to.");
		}
	}

	void HandleOnInhale(){
		if (playmakerFSMs != null && playmakerFSMs.Count > 0) {

			//Sends an OnInhale event to all PlaymakerFSM scripts on this object
			foreach (PlayMakerFSM playmakerFSM in playmakerFSMs) {
				playmakerFSM.Fsm.Event("OnInhale");
			}

		} else {
			Debug.LogWarning("There is no PlayMakerFSM to send the event to.");
		}
	}

	void HandleVideoEnded ()
	{
		if (playmakerFSMs != null && playmakerFSMs.Count > 0) {
			
			//Sends an OnVideoEnd event to all PlaymakerFSM scripts on this object
			foreach (PlayMakerFSM playmakerFSM in playmakerFSMs) {
				playmakerFSM.Fsm.Event("OnVideoEnd");
			}
			
		} else {
			Debug.LogWarning("There is no PlayMakerFSM to send the event to.");
		}
	}
}
