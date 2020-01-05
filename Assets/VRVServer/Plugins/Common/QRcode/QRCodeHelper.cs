using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;

public class QRCodeHelper
{
	static public string Read(Texture2D tex)
	{
		BarcodeReader reader = new BarcodeReader();
		int w = tex.width;
		int h = tex.height;
		var pixel32s = tex.GetPixels32();
		var r = reader.Decode(pixel32s, w, h);
		return r.Text;
	}


	static public Result Read(WebCamTexture tex)
	{
		BarcodeReader reader = new BarcodeReader();
		int w = tex.width;
		int h = tex.height;
		var pixel32s = tex.GetPixels32();
		return reader.Decode(pixel32s, w, h);
	}


	public static string ReadString(WebCamTexture tex)
	{
		var r = QRCodeHelper.Read(tex);

		if (r != null)
			return r.Text;
		else
			return "error";
	}


	static public Texture2D CreateQRCode(string str, int w, int h)
	{
		var tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
		var content = Write(str, w, h);
		tex.SetPixels32(content);
		tex.Apply();
		return tex;
	}

	static Color32[] Write(string content, int w, int h)
	{
		Debug.Log(content + " / " + w + " / " + h);

		var writer = new BarcodeWriter
		{
			Format = BarcodeFormat.QR_CODE,
			Options = new QrCodeEncodingOptions
			{
				Width = w,
				Height = h
			}
		};
		return writer.Write(content);
	}
}
