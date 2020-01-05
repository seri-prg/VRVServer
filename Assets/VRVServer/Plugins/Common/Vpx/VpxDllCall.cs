using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class VpxDllCall
{
#if UNITY_ANDROID && !UNITY_EDITOR
	public const string import_bin = "vpx";
#else
	public const string import_bin = "libvpx_io_win";
#endif

	// デコード用画像情報
	[StructLayout(LayoutKind.Sequential)]
	public struct DECODE_IMG_INFO
	{
		public int width;
		public int hight;
		public int uStride;
		public int vStride;
		public int yStride;
		public int xShift;		// UVのxを右シフトさせる数(すごく端的に言うと　0:444  1:422)
		public int yShift;
	};


	[DllImport(import_bin)]
	public static extern void EncodeSetup(int sizex, int sizey);

	[DllImport(import_bin)]
	public static extern int EncodeSetup(int width, int height, int bitrate, int cpuUse);
	// RGBAフォーマットの画像を設定します。
	// 画像サイズはEncodeSetupで設定されたものとしてエンコードします
	[DllImport(import_bin)]
	public static extern int EncodeSetFrameRGBA(IntPtr data, int size, int storage, int imgId);

	[DllImport(import_bin)]
	public static extern int EncodeSetFrameYUV(IntPtr data, int size, int storage, int flags, int imgId);


	[DllImport(import_bin)]
	public static extern IntPtr EncodeGetData(ref int size);

	[DllImport(import_bin)]
	public static extern int EncodeClose();





	[DllImport(import_bin)]
	public static extern int DecodeSetup();

	[DllImport(import_bin)]
	public static extern int DecodeGetLastImgId(ref int receiveId, ref int showId);

	// フレームサイズ取得
	// DecodeGetImageでデータが取得された時にサイズが入っている事が保証されます。
	[DllImport(import_bin)]
	public static extern int DecodeGetFrameSize(ref int width, ref int height);

	[DllImport(import_bin)]
	public static extern int DecodeGetImage(IntPtr buffer, int size, int stride, ref int lastFrame);

	[DllImport(import_bin)]
	public static extern int DecodeYUVImage(out IntPtr ybuff, out IntPtr ubuff, out IntPtr vbuff, ref DECODE_IMG_INFO imgInfo,  ref int lastFrame);

	[DllImport(import_bin)]
	public static extern int DecodeSkipYUVImage(int skipFrameMax, out IntPtr ybuff, out IntPtr ubuff, out IntPtr vbuff, ref DECODE_IMG_INFO imgInfo, ref int lastFrame);



	//	[DllImport(import_bin)]
	//	public static extern  IntPtr DecodeGetImageRGBX(ref int lastFrame);

	[DllImport(import_bin)]
	public static extern int DecodeWriteBuffer(IntPtr buffer, int size);

	[DllImport(import_bin)]
	public static extern int DecodeClose();

}
