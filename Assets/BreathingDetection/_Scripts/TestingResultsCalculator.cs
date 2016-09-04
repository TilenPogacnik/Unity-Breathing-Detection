using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;

public class TestingResultsCalculator : MonoBehaviour {
	
	[XmlRoot("BreathingData")]
	public class BreathingEventContainer{
		
		[XmlArray("BreathingEvents")]
		[XmlArrayItem("Event")]
		public List<BreathingEvent> BreathingEventList = new List<BreathingEvent> ()	;
		
		public BreathingEventContainer(){
			//BreathingEventList = new List<BreathingEvent>();
		}
		
		public void SaveXml(string path){
			var xmls = new XmlSerializer(typeof(BreathingEventContainer));
			using (var stream = new FileStream(path, FileMode.Create)){
				xmls.Serialize(stream, this);
			}	
		}

		public static BreathingEventContainer Load(string path){
			var xmls = new XmlSerializer (typeof(BreathingEventContainer));
			using (var stream = new FileStream(path, FileMode.Open)) {
				return xmls.Deserialize(stream) as BreathingEventContainer;
			}
		}
	}
	
	public class BreathingEvent{
		[XmlAttribute("type")]
		public string Type;
		
		public float Time;
		
		private BreathingEvent(){
			
		}
		
		public BreathingEvent(string type, float time){
			Type = type;
			Time = time;
		}
	}
	
	public string LoadPath;
	public string CorrectResultsFileName;
	public string AlgorithmResultsFileName;

	
	private BreathingEventContainer correctResultsContainer;
	private List<BreathingEvent> correctResultsList;

	private BreathingEventContainer algorithmResultsContainer;
	private List<BreathingEvent> algorithmResultsList;

	public float allowedError = 0.2f;

	public int TPExhale;
	public int FPExhale;
	public int FNExhale;
	public float RecallExhale;
	public float PrecisionExhale;
	public float FMeasureExhale;

	public int TPInhale;
	public int FPInhale;
	public int FNInhale;
	public float RecallInhale;
	public float PrecisionInhale;
	public float FMeasureInhale;



	
	void Start () {
		if (LoadPath == "") {
			LoadPath = Application.streamingAssetsPath;
		}

		correctResultsContainer = new BreathingEventContainer ();
		correctResultsContainer = BreathingEventContainer.Load (Path.Combine (LoadPath, CorrectResultsFileName));
		correctResultsList = correctResultsContainer.BreathingEventList;


		algorithmResultsContainer = new BreathingEventContainer ();
		algorithmResultsContainer = BreathingEventContainer.Load (Path.Combine (LoadPath, AlgorithmResultsFileName));
		algorithmResultsList = algorithmResultsContainer.BreathingEventList;

		Invoke ("AnalyzeData", 1.0f);

	}

	void AnalyzeData(){
		ICollection<BreathingEvent> correctCollection = correctResultsList;
		ICollection<BreathingEvent> algorithmCollection = algorithmResultsList;


		foreach (BreathingEvent correctBreathingEvent in correctCollection.Reverse()) {
			foreach (BreathingEvent algorithmBreathingEvent in algorithmCollection.Reverse()){

				if (correctBreathingEvent.Type == algorithmBreathingEvent.Type){
					if (Mathf.Abs(correctBreathingEvent.Time - algorithmBreathingEvent.Time) <= allowedError){

						correctResultsList.Remove(correctBreathingEvent);
						algorithmResultsList.Remove(algorithmBreathingEvent);

						if (algorithmBreathingEvent.Type == "Exhale"){
							TPExhale ++;
						} else {
							TPInhale ++;
						}
						break;
					}
				}
			}
		}

		foreach (BreathingEvent correctBreathingEvent in correctResultsList){
			if (correctBreathingEvent.Type == "Exhale"){
				Debug.Log (correctBreathingEvent.Time);
				FNExhale ++;
			} else {
				FNInhale ++;
			}
			
		}

		foreach (BreathingEvent algorithmBreathingEvent in algorithmResultsList){
			if (algorithmBreathingEvent.Type == "Exhale"){
				Debug.Log (algorithmBreathingEvent.Time);

				FPExhale ++;
			} else {
				FPInhale ++;
			}
		}

		PrecisionExhale = (float)TPExhale / (float)(TPExhale + FPExhale);
		RecallExhale = (float)TPExhale / (float)(TPExhale + FNExhale);
		FMeasureExhale = 2 * (PrecisionExhale * RecallExhale) / (PrecisionExhale + RecallExhale);

		PrecisionInhale = (float)TPInhale / (float)(TPInhale + FPInhale);
		RecallInhale = (float)TPInhale / (float)(TPInhale + FNInhale);
		FMeasureInhale = 2 * (PrecisionInhale * RecallInhale) / (PrecisionInhale + RecallInhale);

		Debug.Log ("EXHALE:");
		Debug.Log ("TP: " + TPExhale + ", FP: " + FPExhale + ", FN: " + FNExhale + ", Precision: " + PrecisionExhale + ", Recall: " + RecallExhale + ", F-Measure: " + FMeasureExhale);

		Debug.Log ("INHALE:");
		Debug.Log ("TP: " + TPInhale + ", FP: " + FPInhale + ", FN: " + FNInhale + ", Precision: " + PrecisionInhale + ", Recall: " + RecallInhale + ", F-Measure: " + FMeasureInhale);
	}

}
