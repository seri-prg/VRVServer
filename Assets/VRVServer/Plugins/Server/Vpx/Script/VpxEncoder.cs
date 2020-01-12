using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;



// フレームバッファをエンコード
// 
public class VpxEncoder
{
	private const int VPX_EFLAG_FORCE_KF = 1 << 0;  // キーフレーム設定


	private Texture _srcTexture = null;   // フレームバッファを受け取るテクスチャ

	public int Width { get; private set; }
	public int Hight { get; private set; }

	public int ImgId { get; private set; } = 0;

	private bool _enableBackground = true;
	private Color32[] _pixBuffer = null;
	private int _pixBufferSize = 0;
	private Task _backgroundEncoder = null;

	private AsyncGPUReadbackRequest _readReqest;

	private enum UpState
	{
		Non,			// 何もしていない
		ReadPixel,		// 画素ロード中
		SendBuffer,		// データ送信中
	}


	private UpState _updateState = UpState.Non;

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



	public void Setup(int width, int hight, int bitrate, int useCpu, Texture srcTexture)
	{
		this.Width = Mathf.Min(Screen.width, width);
		this.Hight = Mathf.Min(Screen.height, hight);

		VpxDllCall.EncodeSetup(this.Width, this.Hight, bitrate, useCpu);
		_srcTexture = null;

		// 出力
		this.WriteBuffer();

		_enableBackground = true;

		_updateState = UpState.Non;
		_srcTexture = srcTexture;

		_backgroundEncoder = Task.Run(BackGroundEncode);
	}


	// バックグラウンドでエンコード開始
	private void BackGroundEncode()
	{
		while(_enableBackground)
		{
			// 更新がなければ無処理
			while (_updateState != UpState.SendBuffer)
			{
				if (!_enableBackground)
					return;
			}

			this.Encode();

			_updateState = UpState.Non; // 無処理状態に戻る
		}
	}


	// 画像更新
	// フレームバッファを転送用フォーマットに変換してバッファに保存
	// OnPostRender()内で実行してください。
	public void OnPostRender()
	{
		switch (_updateState)
		{
			case UpState.Non:

				// フレームバッファを取得
				if (_srcTexture != null)
				{
					_updateState = UpState.ReadPixel;

					_readReqest = AsyncGPUReadback.Request(_srcTexture, 0,
						(AsyncGPUReadbackRequest readReq) =>
					{
						if (!readReq.hasError)
						{
							Profiler.BeginSample("_srcTexture.GetPixels32");

							int nextSize = readReq.GetData<Color32>().Length;
							if ((_pixBuffer?.Length ?? 0) < nextSize)
							{
								_pixBuffer = new Color32[nextSize];
							}
							readReq.GetData<Color32>().CopyTo(_pixBuffer);
							_pixBufferSize = nextSize;

							Profiler.EndSample();
						}

						_updateState = UpState.SendBuffer;
					});
				}
				break;
			case UpState.ReadPixel:
#if false
				// pixcelリード中なら終了まで待つ
				if (!_readReqest.done)
				{
					Profiler.BeginSample("_readReqest.WaitForCompletion");
					_readReqest.WaitForCompletion();
					Profiler.EndSample();
				}
#endif
				break;
			case UpState.SendBuffer:
				break;
			default:
				break;
		}
	}


	// フレームバッファを変換して任意のバッファに保存
	private bool Encode()
	{
		try
		{
			this.ImgId++;
			var gcH = GCHandle.Alloc(_pixBuffer, GCHandleType.Pinned);

//			var size = _pixBuffer.Length * Marshal.SizeOf(typeof(Color32));
			var size = _pixBufferSize * Marshal.SizeOf(typeof(Color32));
			VpxDllCall.EncodeSetFrameYUV(gcH.AddrOfPinnedObject(), size, this.Width, 0, this.ImgId);

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

