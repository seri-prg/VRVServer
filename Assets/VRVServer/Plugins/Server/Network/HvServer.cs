using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class HvServer
{
	public IPAddress IpAddr { get; private set; }
	public int Port { get; private set; }


	public HvNetworkIO NetIO { get; private set; } = new HvNetworkIO();

	public bool IsBegin { get; private set; } = false;	// サーバ稼動状態



	private IPAddress GetIPAddress()
	{
		var ipentry = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in ipentry.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				return ip;
			}
		}
		return null;
	}



	// サーバ開始
	public void Start(int port)
	{
		// 既に動いている場合は無視
		if (IsBegin)
			return;

		this.Port = port;
		this.IpAddr = this.GetIPAddress();

		IsBegin = true;
		// サーバ開始
		Task.Run(Begin);
	}


	public void Stop()
	{
		IsBegin = false;
	}




	// サーバ開始
	private async Task Begin()
	{
		//TcpListenerオブジェクトを作成する
		var server = new TcpListener(this.IpAddr, this.Port);
		server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		
		try
		{
			//Listenを開始する
			server.Start();

			var ipEndPoint = (IPEndPoint)server.LocalEndpoint;
			Debug.Log($"start Listen({ipEndPoint.Address}:{ipEndPoint.Port})");

			while (IsBegin)
			{
				//接続要求があったら受け入れる
				//			TcpClient client = server.AcceptTcpClient();
				TcpClient client = await server.AcceptTcpClientAsync();

				//クライアントからのTCP接続は別スレッドに投げる
				var ct = Task.Run(() => ClientStream(client));

				// クライアントは１つまで。
				while(IsBegin && !(ct?.IsCompleted ?? true))
				{
					Thread.Sleep(50);
				}
			}
		}
		catch (Exception e)
		{
			Debug.Log($"error : {e.Message}");
		}
		finally
		{
			// サーバ終了
			server.Stop();
		}

	}



	// 各クライアント毎に処理するスレッド
	private void ClientStream(TcpClient client)
	{

		Debug.Log($"Accept client({((IPEndPoint)client.Client.RemoteEndPoint).Address}:{((IPEndPoint)client.Client.RemoteEndPoint).Port})と接続しました。");
		client.LingerState = new LingerOption(true, 0); // 切断時は即切る

		NetIO.Setup();

		// 受信処理作成
		// 本来はクライアント毎に持つべきもの。
		NetIO.Prefix = "server";

		// 開始前コールバック
		NetIO.Begin(client);	// 接続開始

		// テスト送信
//		HvNetIOTest.Send(networkIO);

		//接続されている限り読み続ける
		while (client.Connected && IsBegin)
		{
			Thread.Sleep(3000);
			HvNetIOTest.Send(NetIO);	// 定期的にテスト送信
		}

		Debug.Log($"server : out loop conedted:{client.Connected} _beginServer:{IsBegin} ");

		// 終了を通知
//		HvNetIOCommand.SendEndCommand(NetIO);
		NetIO.Writer.Flush();
		NetIO.CloseNetwork();
		Thread.Sleep(50);
		NetIO.Dispose();

		Thread.Sleep(100);
	}


#if false
	private void Broadcast(string message)
	{
		if (string.IsNullOrEmpty(message))
		{
			return;
		}

	
		foreach (var key in _clientDict.Keys)
		{
			if (key.Connected)
			{
				var stream = key.GetStream();
				stream.WriteAsync(Encoding.ASCII.GetBytes(message), 0, message.Length); // 順番無視で速度重視
				Debug.Log($"Send Done:{message}");
			}
		}
	}
#endif

}
