using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class VRVAudioSender : OggLiveEncoder
{

	private void Start()
	{
		NetworkSender.Instance.NetIO.GetResever<HvNetIOClientSetting>().OnSetting += VRVAudioSender_OnSetting;
		NetworkSender.Instance.NetIO.OnDisconnected += NetIO_OnDisconnected;
	}

	// 切断時
	private void NetIO_OnDisconnected()
	{
		this.Stop();
	}

	// クライアントから設定情報が来た場合
	private void VRVAudioSender_OnSetting(int arg1, int arg2)
	{
		// サウンド送信開始
		this.Play();
	}

	public override uint OnEncodeWrite(IntPtr outPtr, int writeSize, int dataType)
	{
		var result = HvNetOggDecoder.Send(NetworkSender.Instance.NetIO, outPtr, writeSize);
		return (uint)((result) ? writeSize : 0);
	}
}
