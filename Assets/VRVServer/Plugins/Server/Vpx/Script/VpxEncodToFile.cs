using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;


// エンコードしたデータをファイルに出力
public class VpxEncodToFile
{
	private FileStream _fs = null;


	public void Setup(string path, VpxEncoder encoder)
	{
		// バッファ出力
		if (_fs != null)
			return;

//		_fs = new FileStream(Application.dataPath + "/test.vp9", FileMode.Create, FileAccess.Write);
		_fs = new FileStream(Path.Combine(Application.dataPath, path), FileMode.Create, FileAccess.Write);
		// エンコード時イベント登録
		encoder.OnEncoded += this.DebugWriteFile;
	}


	// エンコードデータの通知が来たときの処理
	public void DebugWriteFile(IntPtr ptr, int size)
	{
		byte[] buffer = new byte[size];
		Marshal.Copy(ptr, buffer, 0, size);
		_fs.Write(buffer, 0, size);
	}


	public void Dispose()
	{
		if (_fs != null)
		{
			_fs.Flush();
			_fs.Close();
			_fs = null;
		}
	}
}

