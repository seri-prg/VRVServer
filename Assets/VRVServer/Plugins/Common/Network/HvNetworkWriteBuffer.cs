using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

public class HvNetworkWriteBuffer
{
	private class Buffer
	{
		public Buffer _next = null;
		private MemoryStream _memStream = null;
		private BinaryWriter _stream = null;

		// データバッファ
		public byte[] Data { get; private set; } = new byte[32];

		// データサイズ
		public int Size {  get { return (int)_memStream.Position; } }


		public Buffer()
		{
			_memStream = new MemoryStream(Data);
			_stream = new BinaryWriter(_memStream);
		}

		public void Begin(HvNetworkIO.Command comType, Action<BinaryWriter> func)
		{
			_memStream.Seek(0, SeekOrigin.Begin);   // ストリームのポインタを最初に戻す
			_stream.Write((int)comType);
			_stream.Write((int)0);
			var headerSize = _memStream.Position;

			func(_stream);

			var pos = _memStream.Position;
			_memStream.Seek(sizeof(int), SeekOrigin.Begin);
			_stream.Write((int)(pos - headerSize));
			_memStream.Position = pos;
		}

		// サイズのみ書き込み
		public void Begin(HvNetworkIO.Command comType, int size)
		{
			_memStream.Seek(0, SeekOrigin.Begin);   // ストリームのポインタを最初に戻す
			_stream.Write((int)comType);
			_stream.Write(size);
		}
	}


	private TcpClient _client = null;
	private NetworkStream _writer = null;
	private Buffer _topPtr = null;

	private object _senderSync = new object();

	private byte[] _bigDataBuffer = new byte[1024]; // 大きいデータ用のバッファ
	private byte[] _usingBigDataBuffer = null; // 使用中の大きいデータ用バッファ

	public void Setup(TcpClient client)
	{
		_client = client;
		this._writer = client.GetStream();
	}


	// 空き領域を取得
	private Buffer GetFreeBuffer()
	{
		Buffer result = null;
		if (_topPtr != null)
		{
			result = _topPtr;
			_topPtr = _topPtr._next;
			result._next = null;
		}
		// 未使用が１つもないなら
		else
		{
			result = new Buffer();
		}

		return result;
	}

	// チェック
	private bool CheckWriter()
	{
		if (_writer == null)
		{
			UnityEngine.Debug.Log($"sender : writer is null");
			return false;
		}

		if (!_writer.CanWrite)
		{
			UnityEngine.Debug.Log($"sender : can not wirte !!!");
			return false;
		}

		return true;
	}


	// 開始
	public bool Begin(HvNetworkIO.Command comType, Action<BinaryWriter> func)
	{
		// 
		lock(_senderSync)
		{
			if (!this.CheckWriter())
				return false;

			var buff = this.GetFreeBuffer();
			buff.Begin(comType, func);

			this.BeginWrite(buff);

			return true;
		}
	}

	// 
	public bool Begin(HvNetworkIO.Command comType, IntPtr p, int dataSize)
	{
		lock (_senderSync)
		{
			if (!this.CheckWriter())
				return false;

			var buff = this.GetFreeBuffer();
			buff.Begin(comType, dataSize);

			this.BeginWrite(buff);

			if (dataSize == 0)
				return true;

			this.Write(p, dataSize);

			return true;
		}
	}

	// バッファ非同期書き込み
	private bool Write(IntPtr p, int size)
	{
		if (_writer == null)
			return false;

		if (!_writer.CanWrite)
		{
			UnityEngine.Debug.Log($"sender : can not wirte !!!");
			return false;
		}

		// コピーコストを後に対応。
		// 

		// 使用中の場合待つ
		// 別途メモリを確保してしまう手も。
		var timeCounter = DateTime.Now.Ticks;
		while (_usingBigDataBuffer != null)
		{
			// 繋がっていないor切断されたなら終了
			if (!_client.Connected)
				return false;

			if ((timeCounter + (5 * TimeSpan.TicksPerSecond)) < DateTime.Now.Ticks)
			{
				UnityEngine.Debug.Log($"sender : time out time [{timeCounter + 5.0f} : {Time.time}]");
				return false;
			}
		}

		// 小さければより大きいバッファに拡張
		if (_bigDataBuffer.Length < size)
		{
			_bigDataBuffer = new byte[size];
		}


		Marshal.Copy(p, _bigDataBuffer, 0, size);
		_usingBigDataBuffer = _bigDataBuffer;
		_writer.BeginWrite(_bigDataBuffer, 0, size, this.WriteEnded, _bigDataBuffer);

		return true;
	}


	public void Flush()
	{
		if (_writer == null)
			return;

		_writer.Flush();
	}


	// 送信開始
	private bool BeginWrite(Buffer buff)
	{
		var result = true;
		try
		{
			_writer.BeginWrite(buff.Data,　0, buff.Size, this.WriteEnded, buff);
		}
		catch (Exception e)
		{
			Debug.Log($"BeginRead error [{e.Message}] ");
			_client?.Close();
		}

		return result;
	}


	// 書き込み終了時
	private void WriteEnded(IAsyncResult ar)
	{
		// 既に切断されている場合は無処理
		if ((_client == null) || (!_client.Connected))
			return;

		// バッファの返却
		if (ar.AsyncState is Buffer)
		{
			var buff = ar.AsyncState as Buffer;
			if (buff == null)
				return;

			// 解放データを未使用領域に登録
			buff._next = _topPtr;
			_topPtr = buff;
		}
		else if (ar.AsyncState is byte[])
		{
			// 未使用状態に
			_usingBigDataBuffer = null;
		}
	}


	// 書き込み終了時
	public void OnDisconnected()
	{
		// 未使用状態に
		_usingBigDataBuffer = null;
	}



	// プールバッファ終了
	public void Close()
	{
		_client = null;
		_writer = null;
		_usingBigDataBuffer = null;
	}
}

