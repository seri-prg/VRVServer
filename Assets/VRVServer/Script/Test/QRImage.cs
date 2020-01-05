using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QRImage : MonoBehaviour
{
	private Image _image = null;


    void Start()
    {
		_image = this.GetComponent<Image>();
		NetworkSender.Instance.NetIO.OnConnected += NetIO_OnConnected;

		this.StartCoroutine(ShowQRImage());
	}


	// クライアントと接続した場合
	private void NetIO_OnConnected()
	{
		// 表示を消す
		_image.enabled = false;
	}


	// サーバのQRコード表示
	private IEnumerator ShowQRImage()
	{
		var net = NetworkSender.Instance;
		while (!net.IsBegin)
		{
			yield return null;
		}

		var qrText = $"{net.IP.ToString()}:{net.Port}";
		var texture = QRCodeHelper.CreateQRCode(qrText, 256, 256);

		_image.sprite = Sprite.Create(texture,
						new Rect(0.0f, 0.0f, texture.width, texture.height),
						new Vector2(0.5f, 0.5f));
	}

	void Update()
    {
	}
}
