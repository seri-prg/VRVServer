using Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Networking;


/*
	AudioListenerからoggへencode


*/
public class OggLiveEncoder : MonoBehaviour
{
	private OggCall.OnEncodeRead _read = null;
	private OggCall.OnEncodeWrite _write = null;
	private bool _isPlaying = false;	// 再生状態
	private bool _reqPlaying = false;	// 再生要求

	private float[] _tmpWavBuffer = null;
	private int _sampleRate = 44100;


	[SerializeField]
	private bool _mute = false;


	virtual protected void Start()
    {
		if (this.GetComponent<AudioListener>() == null)
		{
			Debug.LogError("not found AudioListener");
		}

		_sampleRate = AudioSettings.outputSampleRate;
		Debug.Log($"encode sample rate [{_sampleRate}]");
	}


	virtual protected void OnAudioFilterRead(float[] data, int channels)
	{
		// 再生状態の更新
		this.UpdatePlaying();

		if (_isPlaying && (OggCall.COggEncodIsEnded() == 0))
		{
			_tmpWavBuffer = data;
			OggCall.COggEncodUpdate(data.Length / channels);
			_tmpWavBuffer = null;
		}

		// フラグが立っていれば大本の再生をしない
		if (_mute)
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = 0.0f;
			}
		}
	}

	// 再生状態の変更
	private void UpdatePlaying()
	{
		// 前回と状態が変わっていないなら無処理
		if (_isPlaying == _reqPlaying)
			return;

		// 再生要求
		if (_reqPlaying)
		{
			OggCall.COggEncodSetting(2, _sampleRate, 0.1f);

			if (_read == null)
			{
				_read = new OggCall.OnEncodeRead(this.OnEncodeRead);
			}

			if (_write == null)
			{
				_write = new OggCall.OnEncodeWrite(this.OnEncodeWrite);
			}

			if (OggCall.COggEncodBegin(_read, _write) == 0)
			{
				Debug.Log("初期化エラー");
				_reqPlaying = false;
				return;
			}
		}
		// 停止要求
		else
		{
			OggCall.COggEncodClose();
		}

		_isPlaying = _reqPlaying;
	}



	// 再生開始
	public void Play()
	{
		_reqPlaying = true;
	}


	public void Stop()
	{
		_reqPlaying = false;
	}



	private float[] _tmpBuffer = null;


	private int CopyEncodeBuffer(IntPtr outBuffer, uint bufferSize, int offset)
	{
		// 必要なバッファを確保
		var tmpBuffSize = _tmpBuffer?.Length ?? 0;
		if (tmpBuffSize < bufferSize)
		{
			_tmpBuffer = new float[bufferSize];
		}
		
		int destIndex = 0;
		var sourceIndex = 0;
		for (; destIndex < bufferSize; destIndex++)
		{
			// 読み込みバッファがいっぱいになった
			if ((sourceIndex + 1) >= _tmpWavBuffer.Length)
				break;

			_tmpBuffer[destIndex] = _tmpWavBuffer[sourceIndex + offset];
			sourceIndex += 2;
		}

		Marshal.Copy(_tmpBuffer, 0, outBuffer, destIndex);
		return destIndex;
	}


	// 元データ読み込み要求
	private int OnEncodeRead(IntPtr streamPtr1, IntPtr streamPtr2, uint bufferSize)
	{
		if (!_isPlaying)
			return 0;

		if (_tmpWavBuffer == null)
			return 0;



		this.CopyEncodeBuffer(streamPtr1, bufferSize, 0);
		var destIndex = this.CopyEncodeBuffer(streamPtr2, bufferSize, 1);

		return destIndex;
	}


	// oggデータ書き込み
	public virtual uint OnEncodeWrite(IntPtr outPtr, int writeSize, int dataType)
	{

		return 0;
	}



	virtual protected void OnDestroy()
	{
		this.Stop();
		this.UpdatePlaying();
	}



	// Update is called once per frame
	void Update()
    {

	}
}
