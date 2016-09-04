using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

public class BreathingXMLExport : MonoBehaviour {
	
	[XmlRoot("Data")]
	public class FrameDataContainer{
		
		[XmlArray("Frames")]
		[XmlArrayItem("Frame")]
		public List<FrameData> FrameDataList = new List<FrameData> ()	;
		
		public FrameDataContainer(){
			//BreathingEventList = new List<BreathingEvent>();
		}
		
		public void SaveXml(string path){
			var xmls = new XmlSerializer(typeof(FrameDataContainer));
			using (var stream = new FileStream(path, FileMode.Create)){
				xmls.Serialize(stream, this);
			}	
		}
	}
	
	public class FrameData{

		public float Time;
		public float Energy;
		public float MinimizedEnergy;
		public float FFTMaximum;
		
		private FrameData(){
			
		}
		
		public FrameData(float time, float energy, float minimizedEnergy, float fftMaximum){
			Time = time;
			Energy = energy;
			MinimizedEnergy = minimizedEnergy;
			FFTMaximum = fftMaximum;
		}
	}
	
	public string FileName;
	public string SavePath;
	public MicrophoneController micController;
	public BreathingDetection bDetection;

	private FrameDataContainer fdContainer;
	private float timer;

	private bool isRecording;

	void Start () {
		fdContainer = new FrameDataContainer ();
		fdContainer.FrameDataList = new List<FrameData> ();

		timer = 0.0f;
		isRecording = true;

		if (SavePath == "") {
			SavePath = Application.streamingAssetsPath;
		}
	}

	void FixedUpdate(){
		timer += Time.fixedDeltaTime;
		if (isRecording) {
			fdContainer.FrameDataList.Add (new FrameData (timer, micController.loudness, bDetection.minimizedLoudness, micController.getPitch ()));
		}
	}
	
	[ContextMenu ("EndRecording")]
	public void EndRecording(){			
			isRecording = false;
			fdContainer.SaveXml(Path.Combine(SavePath, FileName));
	}
	
}
