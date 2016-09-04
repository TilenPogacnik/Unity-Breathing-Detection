using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

public class BreathingDetectionTestingManager : MonoBehaviour {

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

	public string FileName;
	public string SavePath;
	public AudioSource Audio;
	public bool OutputXml;

	//[XmlArray("BreathingEvents")]
	//private List<BreathingEvent> bEvents;
	private BreathingEventContainer beContainer;


	void OnEnable(){
		BreathingEvents.onExhale += HandleExhale;
		BreathingEvents.onInhale += HandleInhale;
	}

	void OnDisable(){
		BreathingEvents.onExhale -= HandleExhale;
		BreathingEvents.onInhale -= HandleInhale;
	}


	void Start () {
		//bEvents = new List<BreathingEvent> ();
		beContainer = new BreathingEventContainer ();
		beContainer.BreathingEventList = new List<BreathingEvent> ();

		if (SavePath == "") {
			SavePath = Application.streamingAssetsPath;
		}
	}

	void HandleExhale(){
		beContainer.BreathingEventList.Add (new BreathingEvent ("Exhale", Audio.time));
		/*if (!OutputXml) {
			streamWriter.WriteLine ("EXHALE " + Audio.time);
		} else {
			xmls.Serialize(streamWriter
		}*/
	}

	void HandleInhale(){
		beContainer.BreathingEventList.Add (new BreathingEvent ("Inhale", Audio.time));
		/*if (!OutputXml) {
			streamWriter.WriteLine ("INHALE " + Audio.time);
		}*/
	}

	[ContextMenu ("EndRecording")]
	public void EndRecording(){
		if (!OutputXml){
			StreamWriter streamWriter = new StreamWriter (SavePath + "/" + FileName);
			streamWriter.WriteLine ("DATA: " + Audio.clip.name);

			foreach (BreathingEvent be in beContainer.BreathingEventList) {
				streamWriter.WriteLine (be.Type + " " +be.Time);
			}
			streamWriter.Flush();
			streamWriter.Close();

		} else {

			beContainer.SaveXml(Path.Combine(SavePath, FileName));
		}
	}
	
}
