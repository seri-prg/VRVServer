// #define SHOW_NETWORK_INFO	// 通信情報表示

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

// プロトコル管理付きネットワーク受信処理
// 
public class HvNetworkIO
{
	public event Action OnConnected = null; // 接続したときに送られる
	public event Action OnDisconnected = null; // 切断したときに送られる
	private bool _doConnected = false;
	private bool _doDisconnected = false;


	public enum Command : byte
	{
		Non,    // コマンド判定
		StartInfo,  // 開始時にサーバから送られる情報
		ClientInfo, // クライアントから送られる情報
		ClientSetting, // 初期設定クライアントから送られる情報
		Decode,
		OggDecode,
		ServerCommand,	// サーバからのコマンド
		Test,       // テスト
		End
	}


	// 受信リスト
	private Dictionary<byte, HvNetIOBase> _commandList = new Dictionary<byte, HvNetIOBase>()
	{
		{ (byte)Command.Decode, new HvNetDecoder() },
		{ (byte)Command.StartInfo, new HvNetIOStartInfo() },
		{ (byte)Command.ClientInfo, new HvNetIOClientInfo() },
		{ (byte)Command.ClientSetting, new HvNetIOClientSetting() },
		{ (byte)Command.OggDecode, new HvNetOggDecoder() },
		{ (byte)Command.ServerCommand, new HvNetIOServerCommand() },
		{ (byte)Command.Test, new HvNetIOTest() },
	};



	private TcpClient _client = null;
	private HvNetworkReadBuffer _reader = new HvNetworkReadBuffer();
	private Command _command = Command.Non; // 受信中のデータ種別


	// データ受信パース後の処理
	private Dictionary<byte, Action<HvNetIOBase>> _receiveCallback = new Dictionary<byte, Action<HvNetIOBase>>();

	public HvNetworkWriteBuffer Writer { get; private set; } = new HvNetworkWriteBuffer();


	// 受信を取得
	public T GetResever<T>() where T : HvNetIOBase
	{
		foreach (var item in _commandList)
		{
			if (item.Value.GetType() == typeof(T))
				return item.Value as T;
		}

		return null;
	}


	// デバッグ用表示名
	public string Prefix { get; set; } = string.Empty;

	public bool IsConnected { get { return (_client == null) ? false : _client.Connected; } }




	[Conditional("SHOW_NETWORK_INFO")]
	public void Log(string msg)
	{
		// 毎フレーム出力されるログをひとまず無視
		if (_command == Command.Decode)
			return;

		if (_command == Command.ClientInfo)
			return;

		if (_command == Command.Test)
			return;


		UnityEngine.Debug.Log($"{this.Prefix} : {msg}");
	}



	// 受信完了時コールバックを設定
	public void AddReceiveCallback(Command comType, Action<HvNetIOBase> f)
	{
		_receiveCallback.Add((byte)comType, f);
	}



	public HvNetworkIO()
	{
	}


	public void Setup()
	{
		foreach (var item in _commandList)
		{
			item.Value.Setup(this);
		}
	}



	public HvNetIOBase Find(Command comType)
	{
		return this.Find((byte)comType);
	}

	// 任意のクラスを返す
	public HvNetIOBase Find(byte comType)
	{
		HvNetIOBase result = null;
		if (_commandList.TryGetValue(comType, out result))
			return result;

		return null;
	}



	public void Begin(TcpClient client)
	{
		_client = client;
		_reader.Setup(_client);
		this.Writer.Setup(_client);

		_doConnected = true;

		// ヘッダロード
		this.ReadHeader();
	}


	// ネットワーク切断
	public void CloseNetwork()
	{
		_client?.Close();
		_client?.Dispose();
		_client = null;
		_reader?.Close();
		_doDisconnected = true;
	}


	// 終了処理
	public void Dispose()
	{
		foreach (var item in _commandList)
		{
			item.Value.Dispose();
		}
	}


	// 受信時イベントの実行
	// (メインスレッドから呼んでください)
	public void DoEvent()
	{
		// 接続イベント
		if(_doConnected)
		{
			this.OnConnected?.Invoke();
			_doConnected = false;
		}

		foreach (var item in _commandList.Values)
		{
			item.DoEvent();
		}

		// 切断イベント
		if (_doDisconnected)
		{
			this.OnDisconnected?.Invoke();
			this.Writer.OnDisconnected();
			_doDisconnected = false;
		}
	}


	// ヘッダロード
	private void ReadHeader()
	{
		if (!this.IsConnected)
		{
			UnityEngine.Debug.Log($"{this.Prefix} is not connected (ReadHeader)");
			return;
		}

		// ヘッダ読み込み
		_reader.BeginRead(HvNetIOCommand.CommandSize, (BinaryReader reader, byte[] buffer, int size) =>
		{
			_command = (Command)reader.ReadInt32();
			var dataSize = reader.ReadInt32();

			this.Log($"command[{_command.ToString()}] receive header size {size} : dataSize[{dataSize}]");

			// 想定外のコマンド
			if (!Enum.IsDefined(typeof(Command), _command))
			{
				UnityEngine.Debug.Log($"undefined command {this.Prefix} command[{(int)_command}]");
				this.CloseNetwork();
				return;
			}


			// 終了通知なら切断
			if (_command == Command.End)
			{
				UnityEngine.Debug.Log($"receive command end {this.Prefix}");
				this.CloseNetwork();
				return;
			}

			// データがない場合は次のヘッダがくる
			if (dataSize == 0)
			{
				this.ReadHeader();  // ヘッダロード
			}
			else
			{
				this.ReadData(dataSize);    // データロード
			}
		});
	}



	// データロード
	private void ReadData(int dataSize)
	{
		if (!this.IsConnected)
		{
			UnityEngine.Debug.Log($"{this.Prefix} is not connected (ReadData)");
			return;
		}

		_reader.BeginRead(dataSize, (BinaryReader reader, byte[] buffer, int size) =>
		{
			var dataFunc = this.Find(_command);
			if (dataFunc == null)
			{
				UnityEngine.Debug.Log($"receive command error {this.Prefix} : [{_command.ToString()}]");
				this.CloseNetwork();
				return;
			}

			// 受信データ表示
			this.Log($"command[{_command.ToString()}] receive [{size}]");

			try
			{
				if (dataFunc.OnBufferFull(reader, buffer, size))
				{
					// 受信時コールバックが登録されているなら
					Action<HvNetIOBase> callback = null;
					if (_receiveCallback.TryGetValue((byte)_command, out callback))
					{
						callback(dataFunc); // 処理を実行
					}
				}
			}
			catch (Exception e)
			{
				UnityEngine.Debug.Log($"error {e.Message}");
				UnityEngine.Debug.Log($"error {e.StackTrace}");
			}

			this.ReadHeader();    // ヘッダロード
		});
	}
}



