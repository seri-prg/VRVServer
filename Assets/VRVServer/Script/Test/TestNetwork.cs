#define DB_CLIENT
// #define DB_SERVER

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;




public class TestNetwork : MonoBehaviour
{
	public static TestNetwork Instance { get; private set; }

	private HvNetDecoder _decoder = null;


#if DB_SERVER
	private HvServer _server = new HvServer();
#endif

#if DB_CLIENT
	private bool _playClient = false;
#endif


	private void Awake()
	{
		TestNetwork.Instance = this;
	}


	// Start is called before the first frame update
	void Start()
    {

#if DB_SERVER
		_server.Start(10021);
#if DB_CLIENT
		// testクライアント起動
		this.StartClient(_server.IpAddr, _server.Port);
#endif
#endif

	}


#if DB_CLIENT
	// クライアントタスク起動
	public void StartClient(IPAddress addr, int port)
	{
		_playClient = true;
		// testクライアント起動
		Task.Run(() => TestClient(addr, port));
	}
#endif

	// Update is called once per frame
	void Update()
    {
		if (_decoder != null)
		{
			var mesh = this.GetComponent<MeshRenderer>();
//			mesh.material.SetTexture("_MainTex", _decoder.UpdateTexture());
		}
	}


	private void OnDestroy()
	{
		TestNetwork.Instance = null;

#if DB_SERVER
		_server.Stop();
#endif

#if DB_CLIENT
		_playClient = false;
#endif
	}



	// クライアント接続テスト
	public async Task TestClient(IPAddress ipaddr, int port)
	{
		await Task.Delay(1000);
		var netIO = new HvNetworkIO();

		try
		{
			Debug.Log("TestClient : -Start TestClient");

			_decoder = netIO.GetResever<HvNetDecoder>();

			netIO.Prefix = "client";

			var client = new TcpClient(ipaddr.ToString(), port);
			netIO.Begin(client);  // 接続開始


			// Test送信
			HvNetIOTest.Send(netIO);

			// クライアント情報送信
			HvNetIOClientInfo.Send(netIO, Quaternion.identity, -1);

			try
			{
				do
				{
					Thread.Sleep(500);

				} while (netIO.IsConnected && _playClient);
			}
			catch (Exception e)
			{
				Debug.Log($"client error : {e.Message}");
			}


			Debug.Log("TestClient : out loop");
		}
		catch (Exception e)
		{
			Debug.Log(e.Message);
			throw;
		}
		finally
		{
			netIO.CloseNetwork();
			Thread.Sleep(50);
			netIO.Dispose();

			Debug.Log("-End TestClient");
		}
	}


}
