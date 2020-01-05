using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;


// 一文字表示
public class HvNetIOTest : HvNetIOBase
{
	// @retval : true:処理が終わった。 false:バッファが足りず処理できなかった。
	public override bool OnBufferFull(BinaryReader reader, byte[] buffer, int dataSize)
	{
		return true;
	}

	// 送信データ作成
	public static void Send(HvNetworkIO net)
	{
		net.Writer.Begin(HvNetworkIO.Command.Test, IntPtr.Zero, 0);
	}

}

