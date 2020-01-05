using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;


// クライアント接続時にサーバからくる情報
public class HvNetIOStartInfo : HvNetIOBase
{
	public int Width { get; private set; } = 0;
	public int Hight { get; private set; } = 0;
	public int Size { get; private set; } = 0;

	public event Action<int, int, int> OnSetupStartInfo = null;

	private bool _doEvent = false;


	public override void Setup()
	{
		base.Setup();
		_doEvent = false;
	}


	public override bool OnBufferFull(BinaryReader reader, byte[] buffer, int dataSize)
	{
		// 画像サイズ
		this.Width = reader.ReadInt32();
		this.Hight = reader.ReadInt32();
		this.Size = reader.ReadInt32();

		// イベント実行
		_doEvent = true;

		this.Owner.Log($"receive : size[{this.Width},{this.Hight}] bufferSize[{this.Size}]");
		return true;
	}


	// イベント実行
	public override void DoEvent()
	{
		if (_doEvent)
		{
			// イベント最後に来た値を飛ばす
			this.OnSetupStartInfo?.Invoke(this.Width, this.Hight, this.Size);
			_doEvent = false;
		}
	}

	// 送信データ作成
	public static void Send(HvNetworkIO net, int width, int height, int bufferSize)
	{
		net.Writer.Begin(HvNetworkIO.Command.StartInfo, (BinaryWriter writer) =>
		{
			writer.Write(width);
			writer.Write(height);
			writer.Write(bufferSize);
		});

		net.Log($"send : size[{width},{height}] bufferSize[{bufferSize}]");
	}
}

