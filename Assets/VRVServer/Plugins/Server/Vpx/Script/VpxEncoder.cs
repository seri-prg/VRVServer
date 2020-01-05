using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;



// フレームバッファをエンコード
// 
public class VpxEncoder
{
	private const int VPX_EFLAG_FORCE_KF = 1 << 0;	// キーフレーム設定


	private Texture2D _srcTexture = null;	// フレームバッファを受け取るテクスチャ

	public int Width { get; private set; }
	public int Hight { get; private set; }

	public int ImgId { get; private set; } = 0;

	private bool _updateBuffer = false;
	private bool _enableBackground = true;
	private Color32[] _pixBuffer = null;
	private Task _backgroundEncoder = null;

	public event Action<IntPtr, int> OnEncoded = null;	// エンコード時




	private void WriteBuffer()
	{
		while (true)
		{
			int size = 0;
			var ptr = VpxDllCall.EncodeGetData(ref size);

			if (ptr == IntPtr.Zero)
				break;

			if (size == 0)
				break;


			this.OnEncoded?.Invoke(ptr, size);
		}
	}



	public void Setup(int width, int hight, int bitrate, int useCpu)
	{
		this.Width = Mathf.Min(Screen.width, width);
		this.Hight = Mathf.Min(Screen.height, hight);

		VpxDllCall.EncodeSetup(this.Width, this.Hight, bitrate, useCpu);
		_srcTexture = null;

		// 出力
		this.WriteBuffer();

		_enableBackground = true;
		_updateBuffer = false;

		_backgroundEncoder = Task.Run(BackGroundEncode);
	}


	// バックグラウンドでエンコード開始
	private void BackGroundEncode()
	{
		while(_enableBackground)
		{
			// 更新がなければ無処理
			while (!_updateBuffer)
			{
				if (!_enableBackground)
					return;
			}

			// 本来Encodeは別スレッドで行う。
			// エンコード中にOnPostRenderに再度来た場合は転送が間に合っていないので
			// そのフレームはスキップ
			this.Encode();

			_updateBuffer = false;
		}
	}




	// 画像更新
	// フレームバッファを転送用フォーマットに変換してバッファに保存
	// OnPostRender()内で実行してください。
	public void OnPostRender()
	{
		// 送信中なら無処理
		if (_updateBuffer)
			return;

		// テクスチャが生成されていない場合
		if (_srcTexture == null)
		{
			_srcTexture = new Texture2D(this.Width, this.Hight);
		}


		// フレームバッファを取得
		if (_srcTexture != null)
		{
			_srcTexture.ReadPixels(new Rect(0, 0, _srcTexture.width, _srcTexture.height), 0, 0);
			_srcTexture.Apply();
			_pixBuffer = _srcTexture.GetPixels32();
		}

		_updateBuffer = true;
	}


	// フレームバッファを変換して任意のバッファに保存
	private bool Encode()
	{
		try
		{
			this.ImgId++;
			var gcH = GCHandle.Alloc(_pixBuffer, GCHandleType.Pinned);
			VpxDllCall.EncodeSetFrameYUV(gcH.AddrOfPinnedObject(),
						_pixBuffer.Length * Marshal.SizeOf(typeof(Color32)), this.Width, 0, this.ImgId);
			gcH.Free();
		}
		catch (Exception e)
		{
			Debug.LogError($"error : {e.Message}");
		}

		this.WriteBuffer();

		return true;	// 更新あり
	}


	public void Close()
	{
		_enableBackground = false;
		_backgroundEncoder?.Wait();
		_backgroundEncoder = null;

		VpxDllCall.EncodeClose();
		_srcTexture = null;
	}

	// 開放
	public void Dispose()
	{
		this.OnEncoded = null;
		this.Close();
	}

}

