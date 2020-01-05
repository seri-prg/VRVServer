using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;


// 一文字表示
public class HvNetDecoder : HvNetIOBase
{
#if VRV_CLIENT
	public VpxDecoder Decoder { get; private set; } = new VpxDecoder();


	public override void Setup()
	{
		Decoder.Setup();
	}


	public int SetShader(Material material, int settingId)
	{
		return Decoder.SetShaderSetting(material, settingId);
	}


	// メインスレッドから呼んで下さい
	public void UpdateTexture()
	{
		Decoder.UpdateImage();
	}


	// @retval : true:処理が終わった。 false:バッファが足りず処理できなかった。
	public override bool OnBufferFull(BinaryReader reader, byte[] buffer, int dataSize)
	{
		Decoder.Decode(buffer, dataSize);
		return true;
	}


	public override void Dispose()
	{
		Decoder.Dispose();
	}
#else
	// サーバにはデコード要求は来ない
	public override bool OnBufferFull(BinaryReader reader, byte[] buffer, int dataSize)
	{
		return true;
	}
#endif
	// 送信データ作成
	public static void Send(HvNetworkIO net, IntPtr buffer, int size)
	{
		try
		{
			net.Writer.Begin(HvNetworkIO.Command.Decode, buffer, size);
		}
		catch
		{

		}

	}

}

