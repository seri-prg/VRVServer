using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;


// サーバからクライアントへのコマンド
public class HvNetIOServerCommand : HvNetIOBase
{
	public enum ServerCommand : int
	{
		ResetTracking,	// 頭の位置をリセット
		UVScale,		// クライアント側のUV倍率
	}

	private Dictionary<ServerCommand, float> _reciveCommand = new Dictionary<ServerCommand, float>();

	public event Action<IReadOnlyDictionary<ServerCommand, float>> OnReciveCommand = null;

	private bool _doEvent = false;


	public override void Setup()
	{
		base.Setup();
		_reciveCommand.Clear();
		_doEvent = false;
	}



	public override bool OnBufferFull(BinaryReader reader, byte[] buffer, int dataSize)
	{
		// 画像サイズ
		var command = (ServerCommand)reader.ReadInt32();
		var value = reader.ReadSingle();

		// 同じコマンドの要求は後から来たものを優先させる
		_reciveCommand[command] = value;

		// イベント実行
		_doEvent = true;

		this.Owner.Log($"receive :  server command[{command.ToString()}] value[{value}]");
		return true;
	}


	// イベント実行
	public override void DoEvent()
	{
		if (_doEvent)
		{
			// イベント最後に来た値を飛ばす
			this.OnReciveCommand?.Invoke(_reciveCommand);
			_reciveCommand.Clear();
			_doEvent = false;
		}
	}

	// 送信データ作成
	public static void Send(HvNetworkIO net, ServerCommand command, float data)
	{
		net.Writer.Begin(HvNetworkIO.Command.ServerCommand, (BinaryWriter writer) =>
		{
			writer.Write((int)command);
			writer.Write(data);
		});

		net.Log($"send : server command[{command.ToString()}] value[{data}]");
	}
}

