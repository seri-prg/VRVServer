using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class VRVEyeInput
{
	private Transform _rotOffset = null;


	public void Setup(Transform rotOffset)
	{
		_rotOffset = rotOffset;
		var netIO = NetworkSender.Instance.NetIO;
		netIO.GetResever<HvNetIOClientInfo>().OnClientInfo += NetworkSender_OnClientInfo;
	}

	// クライアントからの情報が来たとき
	private void NetworkSender_OnClientInfo(Quaternion rot, int frameCount)
	{
		if (_rotOffset != null)
		{
			_rotOffset.rotation = rot;
		}
	}
}
