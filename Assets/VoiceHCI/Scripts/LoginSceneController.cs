using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;

public class LoginSceneController : MonoBehaviour {

	public static LoginSceneController Instance;
	
	public string IPAddress = "127.0.0.1";

	public GUISkin Skin;
	public int Width = 400;
	public int Height = 300;
	public int WindowMargin = 10;

	bool canMoveToScene = false;
	bool disconnected = false;

	int SH {
		get { return Screen.height; }
	}
	
	int SW {
		get { return Screen.width; }
	}

	Rect WindowRect {
		get { return new Rect (0, 0, Width, Height); }
	}

	Rect ContentRect {
		get { return new Rect (WindowMargin, WindowMargin, Width - 2*WindowMargin, Height - 2*WindowMargin); }
	}

	string loginName = "";
	string port = "36497";

	void Awake () {
		if (Instance != null && this != Instance) {
			Destroy (this);
			return;
		} else {
			Instance = this;
		}
	}

	void Start () {
		DontDestroyOnLoad (transform.gameObject);
		loginName = "Guest" + UnityEngine.Random.Range (0, 999);
		HCINetwork.Instance.OnClientConnected = () => { canMoveToScene = true; };
		HCINetwork.Instance.OnConnectedToServer = () => { canMoveToScene = true; };
		HCINetwork.Instance.OnDisconnected = () => { disconnected = true; };
	}

	void Update () {
		if (canMoveToScene) {
			Application.LoadLevel ("VoiceHCI");
			canMoveToScene = false;
		}
		if (disconnected) {
			Application.LoadLevel ("LoginScene");
			disconnected = false;
		}
	}

	void OnApplicationQuit () {
		HCINetwork.Instance.Disconnect ();
	}

	void OnGUI () {
		GUI.Box (WindowRect, "");
		GUI.skin = Skin;
		GUILayout.BeginArea (ContentRect);
		GUILayout.Label ("ノンバーバルコミュニケーション");

		if (!HCINetwork.Instance.IsConnected) {

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Name: ", GUILayout.Width (70), GUILayout.Height (50));
			loginName = GUILayout.TextField (loginName, GUILayout.Height (50));
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("IPAddr: ", GUILayout.Width (80), GUILayout.Height (50));
			IPAddress = GUILayout.TextField (IPAddress, GUILayout.Height (50));
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Port: ", GUILayout.Width (80), GUILayout.Height (50));
			port = GUILayout.TextField (port, GUILayout.Height (50));
			GUILayout.EndHorizontal ();

			GUILayout.Space (20);

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Connect", GUILayout.Height (40))) {
				HCINetwork.Instance.Connect (Dns.GetHostEntry (IPAddress).AddressList [0], int.Parse(port));
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Start Server", GUILayout.Height (40))) {
				HCINetwork.Instance.CreateServer (int.Parse(port), 1);
			}
			GUILayout.EndHorizontal ();

		} else {

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Disconnect", GUILayout.Height (40))) {
				HCINetwork.Instance.Disconnect ();
			}
			GUILayout.EndHorizontal ();

		}

		GUILayout.EndArea ();
	}

}
