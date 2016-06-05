using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public enum MoodType {
	Calm,
	Talk,
	Hyper
};

public class VoiceAnalyzer : MonoBehaviour {

	[System.Serializable]
	public class FreqThreshold
	{
		public float ThreshHold;
		public float Enhance = 10f;
		public float CurrentValue;
		public float OffsetValue;
	}

	public VoiceRecorder Recorder;
	public AudioSource audioSource {
		get {
			return Recorder.audioSource;
		}
	}

	float [] _Waves;
	public float[] Waves 
	{
		get {
			if (_Waves == null) {
				_Waves = new float[RES];
			}
			AudioListener.GetOutputData(_Waves, 1);
			return _Waves;
		}
	}

	public MoodType Mood;
	public MoodType ReceivedMood;

	public int RES = 1024;
	public List <FreqThreshold> Spectrums = new List<FreqThreshold> ();

	IEnumerator Start () {
		yield return StartCoroutine ( CalcSpectrumOffset () );
		StartCoroutine ( CalcMood () );
	}

	void Update()
	{
		/*var specs = GetSpectrum ();
		for (int i = 1; i < specs.Length - 1; i++) {
			Debug.DrawLine(
				new Vector3(i - 1, specs[i] + 10, 0), 
				new Vector3(i, specs[i + 1] + 10, 0), 
				Color.red);
			Debug.DrawLine(
				new Vector3(i - 1, Mathf.Log(specs[i - 1]) + 10, 2), 
				new Vector3(i, Mathf.Log(specs[i]) + 10, 2), 
				Color.cyan);
			Debug.DrawLine(
				new Vector3(Mathf.Log(i - 1), specs[i - 1] - 10, 1), 
				new Vector3(Mathf.Log(i), specs[i] - 10, 1), 
				Color.green);
			Debug.DrawLine(
				new Vector3(Mathf.Log(i - 1), Mathf.Log(specs[i - 1]), 3), 
				new Vector3(Mathf.Log(i), Mathf.Log(specs[i]), 3), 
				Color.yellow);
		}*/
		//Debug.Log ("Volume; " + string.Format("{0:f4}\r\n", GetAveragedVolume ()) );

		UpdateSpectrum ();

		if (HCINetwork.Instance.IsConnected) {
			if (HCINetwork.Instance.ReceivedInt.Count != 0) {
				ReceivedMood = (MoodType)HCINetwork.Instance.ReceivedInt.Dequeue ();
			}
		}
	}

	IEnumerator CalcMood () {
		float updateTime = 2.0f;
		float cur = 0f;
		while (true) {
			int cnt = 0;
			float [] samples = new float[Spectrums.Count];
			while (updateTime > cur) {
				cur += Time.deltaTime;
				for (int j = 0; j < Spectrums.Count; j++) {
					samples [j] += Spectrums [j].CurrentValue;
				}
				cnt ++;
				yield return null;
			}
			float [] rate = new float[Spectrums.Count];
			for (int i = 0; i < Spectrums.Count; i++) {
				rate [i] = ((samples [i] * Spectrums [i].Enhance) / (float)cnt) / Spectrums [i].OffsetValue;
			}

			if (rate [2] > 10.0f) {
				Mood = MoodType.Hyper;
			} else if (rate [1] > 2.5f){
				Mood = MoodType.Talk;
			} else {
				Mood = MoodType.Calm;
			}

			if (HCINetwork.Instance.IsConnected) {
				HCINetwork.Instance.SendInt ((int)Mood);
			}
			yield return null;
			cur = 0f;
		}
	}

	IEnumerator CalcSpectrumOffset() {
		int CNT = 50;
		float [] samples = new float[Spectrums.Count];
		for (int i = 0; i < CNT; i++) {
			for (int j = 0; j < Spectrums.Count; j++) {
				samples [j] += Spectrums [j].CurrentValue;
			}
			yield return null;
		}
		for (int i = 0; i < Spectrums.Count; i++) {
			Spectrums [i].OffsetValue = (samples [i] * Spectrums [i].Enhance) / (float)CNT;
		}
	}
	
	public void UpdateSpectrum () {
		var specs = GetSpectrum ();
		var deltaFreq = AudioSettings.outputSampleRate / RES;
		Spectrums.Sort ((a, b) => (int)(Mathf.Sign (a.ThreshHold - b.ThreshHold)));
		foreach (var ft in Spectrums) {
			ft.CurrentValue = 0f;
		}
		for (var i = 0; i < RES; ++i) {
			var freq = deltaFreq * i;
			for (int j = 0; j < Spectrums.Count; j++) {
				if (freq <= Spectrums [j].ThreshHold) {
					Spectrums [j].CurrentValue += specs [i];
				}
			}
		}
		for (var i = 0; i < Spectrums.Count; ++i) {
			Spectrums [i].CurrentValue *= Spectrums [i].Enhance;
		}
	}

	public float GetVolume () {
		return Waves.Select(x => x*x).Sum() / Waves.Length;
	}

	//現フレームで再生されている AudioClip から平均的な音量を取得します.
	public float GetAveragedVolume()
	{
		float[] data = new float[256];
		float a = 0;
		audioSource.GetOutputData(data, 0);
		foreach(float s in data)
		{
			a += Mathf.Abs(s);
		}
		//平均を返します.
		return a/256;
	}

	public float [] GetSpectrum () {
		return audioSource.GetSpectrumData(RES, 0, FFTWindow.Hamming);
	}
}
