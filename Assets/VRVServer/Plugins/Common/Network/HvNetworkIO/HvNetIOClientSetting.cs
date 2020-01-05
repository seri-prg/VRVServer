using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


// サーバ接続時にクライアンから送る情報
public class HvNetIOClientSetting : HvNetIOBase
{
	public int Width { get; private set; } = 0;
	public int Hight { get; private set; } = 0;

	public bool _doEvent = false;
	public event Action<int, int> OnSetting = null;


	public override void Setup()
	{
		base.Setup();
		_doEvent = false;
	}


	public override bool OnBufferFull(BinaryReader reader, byte[] buffer, int dataSize)
	{
		this.Width = reader.ReadInt32();
		this.Hight = reader.ReadInt32();
		this.Owner.Log($"receive : width[{this.Width}] height[{this.Hight}]");

		_doEvent = true;
		return true;
	}


	public override void DoEvent()
	{
		if (_doEvent)
		{
			// クライアントから情報来た場合
			this.OnSetting?.Invoke(this.Width, this.Hight);
			_doEvent = false;
		}
	}


	// 送信データ作成
	public static void Send(HvNetworkIO net, int width, int height)
	{
		net.Writer.Begin(HvNetworkIO.Command.ClientSetting, (BinaryWriter writer) =>
		{
			writer.Write(width);
			writer.Write(height);
		});

		net.Log($"send : [{width},{height}]");
	}
}

