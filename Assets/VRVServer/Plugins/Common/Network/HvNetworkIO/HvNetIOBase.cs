using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;


// データ読み込み時のコールバック
public abstract class HvNetIOBase
{
	public HvNetworkIO Owner { get; private set; } = null;


	// セットアップ
	public void Setup(HvNetworkIO owner)
	{
		Owner = owner;
		this.Setup();
	}


	// セットアップ
	public virtual void Setup() { }

	// 受信時処理
	// reader, bufferは同じメモリを指す。
	public abstract bool OnBufferFull(BinaryReader reader, byte[] buffer, int dataSize);

	// 終了処理
	public virtual void Dispose() { }

	// イベント呼び出し
	// (メインスレッドから呼ばれる)
	public virtual void DoEvent() { }
}

