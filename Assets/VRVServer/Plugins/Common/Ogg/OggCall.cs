using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class OggCall
{
#if UNITY_ANDROID && !UNITY_EDITOR
	const string FileLibrary = "tremolo";
#else
	const string FileLibrary = "libvorbisfile";
#endif


	// エンコード
	// 


	// 元データ読み込みコールバック
	// @param streamPtr1: 周波数データ1
	// @param streamPtr2: 周波数データ2
	// @param bufferSize: 読み込み可能な最大サイズ(floatの要素数)
	// @retval : 実際に読み込んだ要素数
	public delegate int OnEncodeRead(IntPtr streamPtr1/* float * */, IntPtr streamPtr2/* float * */, uint bufferSize);


	// 圧縮後のデータ
	// @param ptr: 圧縮後のデータ
	// @param writeSize: 書き込みデータサイズ(byte数)
	// @param dataType : 0:header 1:body
	public delegate uint OnEncodeWrite(IntPtr outPtr /* byte * */, int writeSize, int dataType);



	// 終了したか
	[DllImport(FileLibrary)]
	public static extern int COggEncodIsEnded();

	// エンコード設定
	[DllImport(FileLibrary)]
	public static extern void COggEncodSetting(long channels /*2*/, long rate /*44100*/, float base_quality /*0.1f*/);


	// エンコード開始
	[DllImport(FileLibrary)]
	public static extern int COggEncodBegin(
					[MarshalAs(UnmanagedType.FunctionPtr)] OnEncodeRead readFunc,
					[MarshalAs(UnmanagedType.FunctionPtr)] OnEncodeWrite writeFunc);

	// 
	[DllImport(FileLibrary)]
	public static extern void COggEncodUpdate(int readBufferSize);


	[DllImport(FileLibrary)]
	public static extern void COggEncodClose();







	// デコード
	// 

	public delegate uint OvReadFunc(IntPtr outPtr, uint size, uint nmemb);

	[DllImport(FileLibrary)]
	public static extern int OggSetup(ref int channels, ref long rate, [MarshalAs(UnmanagedType.FunctionPtr)] OvReadFunc func);

	[DllImport(FileLibrary)]
	public static extern long OggRead(ref IntPtr data);

	[DllImport(FileLibrary)]
	public static extern void OggClose();



	// デバッグ出力
	// 


	public delegate void debug_log_func_type(string msg);


	[DllImport(FileLibrary)]
	public static extern void set_debug_log_func([MarshalAs(UnmanagedType.FunctionPtr)] debug_log_func_type func);

	[DllImport(FileLibrary)]
	public static extern void debug_log_test();
}
