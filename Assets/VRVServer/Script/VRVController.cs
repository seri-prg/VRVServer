using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


// VRコントローラ
// Cameraにつけて下さい。
public class VRVController : MonoBehaviour
{
	private Camera _camera = null;

	[SerializeField]
	private Transform _rotTarget = null;

	[SerializeField]
	private int _port = 10022;

	[SerializeField]
	private float _clientUVScale = 0.9f;
	private float _lastClientUVScale = 0.9f;


	[SerializeField]
	private int _bitrate = 600;

	[SerializeField]
	private int _useCpu = 10;



	private int _allowableDelay = 10;	// これ以上クライアントが遅延する場合は更新をサーバからの送信を１度止める。
	private VRVEyeInput _eyeInput = new VRVEyeInput();
	private VRVLensCorrection _lensCorrenction = new VRVLensCorrection();
	private VpxEncoder _encoder = new VpxEncoder();

	public bool IsPlay { get; private set; } = false;	// 送信開始しているか


	public int HmdWidth { get; private set; } = 0;
	public int HmdHeight { get; private set; } = 0;

	private Rect _cameraRectDefault;

	private int _lastClientImageId = 0;

	private bool _waitClient = false;



	// クライアントの遅延フレーム
	public int DelayFrame { get { return (_encoder.ImgId - _lastClientImageId); } }


	private void Start()
	{
		_camera = this.GetComponent<Camera>();

		if (_camera == null)
		{
			Debug.LogError("_camera is not setting");
			return;
		}

		if (_rotTarget == null)
		{
			Debug.LogError("_rotOffset is not setting");
			return;
		}

		_lastClientUVScale = _clientUVScale;

		_cameraRectDefault = _camera.rect;

		// エンコーダセットアップ
		_encoder.OnEncoded += this.WriteStream;   // 書き込みコールバック

		_eyeInput.Setup(_rotTarget);
		_lensCorrenction.Setup(_camera);

		// クライアントから設定が届いた時
		NetworkSender.Instance.NetIO.GetResever<HvNetIOClientSetting>().OnSetting += NetworkSender_OnSetting;
		NetworkSender.Instance.NetIO.GetResever<HvNetIOClientInfo>().OnClientInfo += VRVController_OnClientInfo;
		NetworkSender.Instance.NetIO.OnDisconnected += NetIO_OnDisconnected;

		NetworkSender.Instance.Begin(_port);
	}

	// クライアントから情報が来たとき
	private void VRVController_OnClientInfo(Quaternion rot, int imageId)
	{
		_lastClientImageId = imageId;
	}


	// 切断時処理
	private void NetIO_OnDisconnected()
	{
		_encoder.Close();
		_lensCorrenction.Remove();

		_camera.rect = _cameraRectDefault;
	}


	// クライアントから設定情報が来たとき
	private void NetworkSender_OnSetting(int inWidth, int inHeight)
	{
		// 幅の比率
		float wrate = (float)(inWidth / 2) / inHeight;

		// 現在の解像度に直す
		var width = wrate * Screen.height;

		// 要求された幅の方が長い
		if (width > (float)Screen.width)
		{
			this.HmdWidth = Screen.width;
			this.HmdHeight = (int)((float)inHeight / (float)(inWidth / 2) * Screen.width);

		}
		else
		{
			this.HmdWidth = (int)width;
			this.HmdHeight = Screen.height;
		}


		Debug.Log($"this.HmdWidth = {this.HmdWidth} : this.HmdHeight = {this.HmdHeight}");

		_camera.rect = new Rect(0.0f, 0.0f, (float)this.HmdWidth / Screen.width, 1.0f);
		_lensCorrenction.ResizeBufffer(this.HmdWidth, this.HmdHeight);

		// サーバから送られる解像度を送信
		HvNetIOStartInfo.Send(NetworkSender.Instance.NetIO, this.HmdWidth, this.HmdHeight, 1024);

		// クライアントと繋がったら送信する。
		_encoder.Setup(this.HmdWidth, this.HmdHeight, _bitrate, _useCpu);

		// １度画像IDを揃える
		_waitClient = false;

		// クライントにUV倍率を送る
		HvNetIOServerCommand.Send(NetworkSender.Instance.NetIO, HvNetIOServerCommand.ServerCommand.UVScale, _clientUVScale);

		this.IsPlay = true;
	}



	private void Update()
	{
		if (!Mathf.Approximately(_lastClientUVScale, _clientUVScale))
		{
			HvNetIOServerCommand.Send(NetworkSender.Instance.NetIO, HvNetIOServerCommand.ServerCommand.UVScale, _clientUVScale);
			_lastClientUVScale = _clientUVScale;
		}

	}


	private void OnPostRender()
	{
		RenderTexture.active = _lensCorrenction.LensTexture;

		if (this.IsPlay)
		{
			// クライアントが一定以上遅れたので、１度待つ
			if (_waitClient)
			{
				// 一定以下になるまでクライアントを待つ。
				if (this.DelayFrame <= 2)
				{
					_waitClient = false;
				}
			}
			else
			{
				if (this.DelayFrame > _allowableDelay)
				{
					_waitClient = true;
				}

				_encoder.OnPostRender();
			}

		}
	}

	private void OnDestroy()
	{
		NetworkSender.DoIfActive((NetworkSender net) =>
		{
			// クライアントから設定が届いた時
			net.NetIO.GetResever<HvNetIOClientSetting>().OnSetting -= NetworkSender_OnSetting;
			net.NetIO.GetResever<HvNetIOClientInfo>().OnClientInfo -= VRVController_OnClientInfo;
			net.NetIO.OnDisconnected -= NetIO_OnDisconnected;
		});


		_encoder.Dispose();
	}


	// エンコードデータの通知が来たときの処理
	public void WriteStream(IntPtr ptr, int size)
	{
		var netIO = NetworkSender.Instance.NetIO;
		// 接続が切れている場合は送信しない
		if (!netIO.IsConnected)
			return;

		//		Debug.Log($"encode send[{size}]");

		// デコードへ送信
		HvNetDecoder.Send(netIO, ptr, size);
	}


	// 角度リセット要求
	public static void SendResetTrackingCenter()
	{
		var n = NetworkSender.Instance.NetIO;
		if (n == null)
			return;

		HvNetIOServerCommand.Send(n, HvNetIOServerCommand.ServerCommand.ResetTracking, 0.0f);
	}


#if false
	// クライアントのUV倍率
	public static void SendClientUVScale(float scale)
	{
		var n = NetworkSender.Instance.NetIO;
		if (n == null)
			return;

		HvNetIOServerCommand.Send(n, HvNetIOServerCommand.ServerCommand.UVScale, scale);
	}
#endif
}
