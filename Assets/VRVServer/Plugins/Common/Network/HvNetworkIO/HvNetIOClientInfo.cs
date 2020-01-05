using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


// クライアンからサーバへ送る情報
public class HvNetIOClientInfo : HvNetIOBase
{
	public Quaternion Rot { get; private set; }

	public int ImageId { get; private set; }	// 最後にクライアントが処理したフレームのカウント


	public bool _doEvent = false;
	public event Action<Quaternion, int> OnClientInfo = null;

	public override void Setup()
	{
		base.Setup();
		_doEvent = false;
	}


	public override bool OnBufferFull(BinaryReader reader, byte[] buffer, int dataSize)
	{
		var rot = new Quaternion();
		rot.x = reader.ReadSingle();
		rot.y = reader.ReadSingle();
		rot.z = reader.ReadSingle();
		rot.w = reader.ReadSingle();
		this.Rot = rot;
		this.ImageId = reader.ReadInt32();

		_doEvent = true;
//		this.Owner.Log($"rot [{this.Rot}]");

		return true;
	}

	public override void DoEvent()
	{
		if (_doEvent)
		{
			this.OnClientInfo?.Invoke(this.Rot, this.ImageId);
			_doEvent = false;
		}
	}

	// 送信データ作成
	public static void Send(HvNetworkIO net, Quaternion cameraRot, int imageId)
	{
		net.Writer.Begin(HvNetworkIO.Command.ClientInfo, (BinaryWriter writer) =>
		{
			// 傾き情報
			writer.Write(cameraRot.x);
			writer.Write(cameraRot.y);
			writer.Write(cameraRot.z);
			writer.Write(cameraRot.w);
			writer.Write(imageId);	// クライアントが最後に処理したフレーム
		});

		net.Log($"send : [{cameraRot}]");
	}
}

