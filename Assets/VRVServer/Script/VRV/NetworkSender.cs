using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;


// エンコードしたデータを送信
// カメラ用コンポーネント
public class NetworkSender : MonoBehaviourSingleton<NetworkSender>
{
	private HvServer _server = new HvServer();

	// 入出力管理
	public HvNetworkIO NetIO { get { return _server.NetIO; } }

	// 稼働中か否か
	public bool IsBegin {  get { return _server.IsBegin; } }

	public IPAddress IP { get { return _server.IpAddr; } }
	public int Port { get { return _server.Port; } }


	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void Init()
	{
		NetworkSender.Create();
	}

	// サーバ開始
	public void Begin(int port)
	{
		_server.Start(port);
	}

	private void Start()
	{
	}



	private void Update()
	{
		_server.NetIO.DoEvent();
	}


	private void OnDestroy()
	{
		_server.Stop();
	}
}

