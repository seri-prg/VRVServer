using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Unity.Collections;
using UnityEngine;

public class HvNetworkReadBuffer
{
	private TcpClient _client = null;

	private byte[] _dataBuffer = null;  // バッファ
	private BinaryReader _reader = null;    // 読み込みバッファリーダー
	private int _totalReadSize = 0;     // バッファに読み込んだトータルサイズ
	private int _needSize = 0;

	private Action<BinaryReader, byte[], int> _callback = null; // 受信完了時コールバック


	public void Setup(TcpClient client)
	{
		_client = client;
	}


	// データ受信
	public bool BeginRead(int size, Action<BinaryReader, byte[], int> callback)
	{
		if (!(_client?.Connected ?? false))
		{
			Debug.Log($"client is not connected.");
			return false;
		}

		this.GetBuffer(size);
		_callback = callback;
		_needSize = size;
		_totalReadSize = 0;

		return this.BeginReadSub();
	}



	// 受信開始
	private bool BeginReadSub()
	{
		var result = true;
		try
		{
			_client?.GetStream().BeginRead(_dataBuffer,
				_totalReadSize, _needSize - _totalReadSize, this.ReadPoolCallback, this);
		}
		catch (Exception e)
		{
			Debug.Log($"BeginRead error [{e.Message}] ");
			_client?.Close();
		}

		return result;
	}



	// バッファの取得
	private byte[] GetBuffer(int needSize)
	{
		// すでに必要以上のバッファを持っている
		if ((_dataBuffer != null) && (_dataBuffer.Length >= needSize))
		{
			_reader.BaseStream.Seek(0, SeekOrigin.Begin);
			return _dataBuffer;
		}

		// バッファを新規作成
		_dataBuffer = new byte[needSize];
		_reader = new BinaryReader(new MemoryStream(_dataBuffer));

		return _dataBuffer;
	}


	// 任意のバッファが溜まるまでBeginReadを繰り返す
	private void ReadPoolCallback(IAsyncResult ar)
	{
		// 既に切断されている場合は無処理
		if ((_client == null) || (!_client.Connected))
			return;

		if (!_client.GetStream().CanRead)
			return;

		try
		{
			var readSize = _client.GetStream().EndRead(ar);
			_totalReadSize += readSize;
			if (_totalReadSize == _needSize)
			{

				// 満たしたなら処理を実行
				// コールバック中にBegiReadを呼ばれても良いように
				// 実行後は即終了
				var c = _callback;
				c?.Invoke(_reader, _dataBuffer, _totalReadSize);
				return;
			}

			// バッファが必要サイズを超えた
			// 送受信に齟齬が起きている
			if (_totalReadSize > _needSize)
			{
				_client?.Close();
				return;
			}

			this.BeginReadSub();

		}
		catch (Exception ee)
		{
			Debug.Log($"{ee.Message}");
			_client?.Close();
		}
	}

	// プールバッファ終了
	public void Close()
	{
		_client = null;
	}


}

