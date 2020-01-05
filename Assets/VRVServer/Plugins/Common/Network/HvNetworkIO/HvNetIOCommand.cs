using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;


// コマンド判定用
public class HvNetIOCommand
{
	public const int CommandSize = 8;


	// 終了コマンド作成
	public static void SendEndCommand(HvNetworkIO net)
	{
		net.Writer.Begin(HvNetworkIO.Command.End,IntPtr.Zero, 0);
	}

}
