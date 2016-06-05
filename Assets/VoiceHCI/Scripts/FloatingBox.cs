using UnityEngine;
using System.Collections;

public class FloatingBox : MonoBehaviour {

	Vector3 CalmRot = new Vector3 (0,90f,0);
	Vector3 TalkRot = new Vector3 (0,0,0f);
	Vector3 HyperRot = new Vector3 (0,-90f,0);

	public bool isMe = true;
	public VoiceAnalyzer Analyzer;
	public Transform Box;
	public float Smooth = 5f;
	Quaternion targetRot;

	// Update is called once per frame
	void Update () {
		switch (Analyzer.Mood) {
			case MoodType.Calm:
				targetRot = Quaternion.Euler (CalmRot);
				break;
			case MoodType.Talk:
				targetRot = Quaternion.Euler (TalkRot);
				break;
			case MoodType.Hyper:
				targetRot = Quaternion.Euler (HyperRot);
				break;
			}
		if (isMe) {
		} else {
			switch (Analyzer.ReceivedMood) {
			case MoodType.Calm:
				targetRot = Quaternion.Euler (CalmRot);
				break;
			case MoodType.Talk:
				targetRot = Quaternion.Euler (TalkRot);
				break;
			case MoodType.Hyper:
				targetRot = Quaternion.Euler (HyperRot);
				break;
			}
		}
		Box.localRotation = Quaternion.Lerp (Box.localRotation, targetRot, Smooth * Time.deltaTime);
	}
}
