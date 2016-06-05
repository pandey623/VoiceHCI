using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public enum NetEvent {
	StartServer,
	Connect,
	Disconnect,
	Error
}

public class HCINetwork {

	static HCINetwork _Instance;
	public static HCINetwork Instance {
		get {
			if (_Instance == null) {
				_Instance = new HCINetwork ();
			}
			return _Instance;
		}
	}

	const int WaitTime = 1;
	
	Socket serverSocket;
	Socket clientSocket;
	TcpClient tcpClient;

	Thread _dispatchThread;
	Thread _dataThread;

	public bool Acceptable = true;
	public Action OnClientConnected;
	public Action OnStartServer;
	public Action OnConnectedToServer;
	public Action OnConnectionFailed;
	public Action OnDisconnected;
	public bool IsServer {
		get { return serverSocket != null; }
	}
	public bool IsConnected {
		get { return IsServer || tcpClient != null; }
	}
	
	public bool CreateServer(int port, int connectionNum) {
		// 既にサーバとして起動
		if (IsServer) {
			Debug.LogError ( "Connection Failed. You are already a server." );
			return false;
		}

		try {
			serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
			serverSocket.Listen(connectionNum);
		} catch {
			Debug.LogError ( string.Format ("Start Server Failed. Port:{0} ConnectNum:{1}", port ,connectionNum) );
			return false;
		}

		Debug.Log ( string.Format ("Started Server. Port:{0} ConnectNum:{1}", port ,connectionNum) );

		if (OnStartServer != null) {
			OnStartServer ();
		}
		
		return LaunchThread();
	}

	public bool Connect(IPAddress ipaddr, int port) {
		// 既にサーバとして起動
		if (IsServer) {
			Debug.LogError ( "Connection Failed. You are already a server." );
			return false;
		}

		try {
			tcpClient = new System.Net.Sockets.TcpClient();
			//clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			tcpClient.NoDelay = true;
			tcpClient.Connect(ipaddr, port);
		} catch {
			tcpClient = null;
			return false;
		}

		bool result = LaunchThread();
		if (!result) {
			if (OnConnectionFailed != null) {
				OnConnectionFailed ();
			}
		} else {
			Debug.Log ( string.Format ("Connected to server. IPAddr:{0}, Port:{1}", ipaddr, port) );
			if (OnConnectedToServer != null) {
				OnConnectedToServer ();
			}
		}

		return result;
	}

	public bool Disconnect () {
		if (!IsConnected) {
			Debug.LogError ( "You are not in connection!" );
			return false;
		}
		if (IsServer) {
			try {
				serverSocket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
			} catch {
			}
			serverSocket.Close ();
			if (clientSocket != null) {
				clientSocket.Close ();
			}
			serverSocket = null;
			clientSocket = null;
		} else {
			tcpClient.Close ();
			tcpClient = null;
		}
		Debug.Log ( "Disconnected" );
		if (OnDisconnected != null) {
			OnDisconnected ();
		}
		return true;
	}

	public Queue <int> ReceivedInt = new Queue<int> ();
	void QueueReceived (byte[] receivedBuffer) {
		byte protocol = GetProtocol (receivedBuffer);
		byte[] realData = GetReceivedData (receivedBuffer);

		Debug.Log ("Received Data, header:" + protocol + " length:" + realData.Length);

		switch ((int)protocol) {
		case 0: // Int
			int value = BitConverter.ToInt32 (realData, 0);
			ReceivedInt.Enqueue (value);
			break;
		default:
			break;
		}
	}

	public void SendInt (int value) {
		if (!IsConnected) {
			return;
		}

		byte[] SendBuffer = InsertProtocol (BitConverter.GetBytes (value), 0);
		if (IsServer && clientSocket != null) {
			int i = clientSocket.Send (SendBuffer);
			Debug.Log ("Sent Int from Server:" + i);
		} else if (tcpClient != null) {
			// Get stream
			NetworkStream stream = tcpClient.GetStream ();
			// send to server
			stream.Write(SendBuffer, 0, SendBuffer.Length);
			stream.Flush(); // flush
			Debug.Log ("Sent Int from Client");
		}
	}

	byte[] InsertProtocol (byte[] sendBuffer, byte header) {
		List <byte> bl = sendBuffer.ToList ();
		bl.Insert (0, header);
		return bl.ToArray ();
	}

	byte GetProtocol (byte[] receivedBuffer) {
		return receivedBuffer [0];
	}

	byte[] GetReceivedData (byte[] receivedBuffer) {
		List <byte> bl = receivedBuffer.ToList ();
		bl.RemoveAt (0);
		return bl.ToArray ();
	}

	bool LaunchThread() {
		try {
			_dispatchThread = new Thread(new ThreadStart(Dispatch));
			_dispatchThread.Start();
			_dataThread = new Thread(new ThreadStart(DataStream));
			_dataThread.Start();
		} catch {
			return false;
		}
		return true;
	}
	
	void Dispatch() {
		while (Acceptable) {
			Accept();
			Thread.Sleep(WaitTime);
		}
	}

	void DataStream() {
		while (Acceptable) {
			byte[] ReceiveData = new byte[4096];
			if (IsServer && clientSocket != null) {
				Debug.Log ("server check receivement");
				// Receive from client
				int ResSize = clientSocket.Receive (ReceiveData, ReceiveData.Length, System.Net.Sockets.SocketFlags.None);
				QueueReceived (ReceiveData);
			} else if (!IsServer) {
				Debug.Log ("client check receivement");
				// Get stream
				NetworkStream stream = tcpClient.GetStream ();
				// Receive from server
				stream.Read (ReceiveData, 0, ReceiveData.Length);
				QueueReceived (ReceiveData);
			}
			Thread.Sleep(WaitTime);
		}
	}
	
	void Accept() {
		if (IsServer && serverSocket.Poll(0, SelectMode.SelectRead)) {
			Debug.Log ( "Accepted client" );
			clientSocket = serverSocket.Accept();
			if (OnClientConnected != null) {
				OnClientConnected ();
			}
		}
	}
}
