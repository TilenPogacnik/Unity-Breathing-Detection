using UnityEngine;
using System.Collections;

public class DisableBreathingDetection : MonoBehaviour {

	private NewVideoController videoController;
	public string VideoName;

	public GameObject BreathingDetection;

	// Use this for initialization
	void Start () {
		BreathingDetection.SetActive (false);
		videoController = this.GetComponent<NewVideoController> ();
		StartCoroutine (WaitForVideosToLoad ());
		videoController.SetVideoOnEnd (VideoName, enableBreathingDetection);
	}
	
	void enableBreathingDetection(){
		BreathingDetection.SetActive (true);
	}

	IEnumerator WaitForVideosToLoad(){
		while (!videoController.allVideosLoaded) {
			yield return new WaitForEndOfFrame();
		}
		}
	
}
