using UnityEngine;
using System.Collections;

public class VoiceRecorder : MonoBehaviour {

	AudioSource _audioSource;
	public AudioSource audioSource
	{
		get {
			if (_audioSource == null) {
				_audioSource = gameObject.AddComponent (typeof (AudioSource)) as AudioSource;
				_audioSource.spatialBlend = 0f;
				_audioSource.loop = true;
			}
			return _audioSource;
		}
	}

	// Use this for initialization
	IEnumerator Start () {
		yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
		audioSource.clip = Microphone.Start(null, true, 1, 44100);
		while (Microphone.GetPosition(null) <= 0) {
			yield return null;
		}
		_audioSource.mute = true;
		audioSource.Play();
	}
}
