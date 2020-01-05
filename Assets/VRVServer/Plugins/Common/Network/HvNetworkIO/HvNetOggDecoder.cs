using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;


// 一文字表示
public class HvNetOggDecoder : HvNetIOBase
{
#if VRV_CLIENT
	private OggDecodStream _stream = null;
	private long _totalSize = 0;
	private bool _start = false;   // 開始しているか
	private bool _doEvent = false; // イベント発行するか


	public event Action OnStart = null;


	public override void Setup()
	{
		_totalSize = 0;
		_start = false;   // 開始しているか
		_doEvent = false; // イベント発行するか
	}


	// ストリーム設定
	public void SetStream(OggDecodStream stream)
	{
		_stream = stream;
	}


	// メインスレッドから呼んで下さい
	public void UpdateTexture()
	{
	}


	// @retval : true:処理が終わった。 false:バッファが足りず処理できなかった。
	public override bool OnBufferFull(BinaryReader reader, byte[] buffer, int dataSize)
	{
		_stream.Write(buffer, dataSize);
		_totalSize += dataSize;

		if ((_totalSize > 32) && !_start)
		{
			_doEvent = true;
			_start = true;
		}

		return true;
	}

	public override void DoEvent()
	{
		if (_doEvent)
		{
			OnStart();
			_doEvent = false;
		}
	}


	public override void Dispose()
	{
	}
#else
	// サーバにはデコード要求は来ない
	public override bool OnBufferFull(BinaryReader reader, byte[] buffer, int dataSize)
	{
		return true;
	}
#endif
	// 送信データ作成
	public static bool Send(HvNetworkIO net, IntPtr buffer, int size)
	{
		return net.Writer.Begin(HvNetworkIO.Command.OggDecode, buffer, size);
	}

}

