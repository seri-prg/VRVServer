using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


// ネイティブからログ出力
// デリゲートを渡してネイティブから実行
// 

public class DllDebugLog : MonoBehaviour
{
	public delegate void DebugLogDelegate(string str);
	DebugLogDelegate debugLogFunc = msg => Debug.Log(msg);

#if UNITY_ANDROID
	public const string _debugLogDll = "vpx";
#else
	public const string _debugLogDll = "libvpx_io_win";
#endif


	[DllImport(_debugLogDll)]
	public static extern void set_debug_log_func(DebugLogDelegate ptr);
	[DllImport(_debugLogDll)]
	public static extern void debug_log_test();


	// Start is called before the first frame update
	void Start()
    {
		set_debug_log_func(debugLogFunc);
		debug_log_test();
	}


}
