using UnityEngine;
using System.Collections;

public class DebugBreathingDetection : MonoBehaviour {

#if UNITY_EDITOR
	[SerializeField] private KeyCode TriggerKeyCode;

	void Update () {
		if (Input.GetKeyDown (TriggerKeyCode)) {
			BreathingEvents.TriggerOnExhale ();
		} 
		if (Input.GetKeyUp (TriggerKeyCode)) {
			BreathingEvents.TriggerOnInhale ();
		}
	}
#endif
}
