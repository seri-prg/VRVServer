using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class VRVLensMesh
{
	public Mesh Mesh { get; private set; } = null;


	public bool Create(int wcount, int hcount)
	{
		if (this.Mesh == null)
		{
			this.Mesh = new Mesh();
		}


		var num = wcount * hcount;
		if (num <= 0)
			return false;

		var point = new Vector3[num];
		var uv = new Vector2[num];

		// 3角形のindex数
		var index = new int[(wcount - 1) * (hcount - 1) * 2 * 3];

		// index設定
		var icount = 0;
		for (int y = 0; y < hcount - 1; y++)
		{
			for (int x = 0; x < wcount - 1; x++)
			{
				var yoffset = y * wcount;
				index[icount] = x + yoffset;
				index[icount + 1] = x + yoffset + 1;
				index[icount + 2] = x + yoffset + wcount + 1;

				index[icount + 3] = x + yoffset;
				index[icount + 4] = x + yoffset + wcount + 1;
				index[icount + 5] = x + yoffset + wcount;

				icount += 6;
			}
		}

		var xscale = 1.0f / (wcount - 1);
		var yscale = 1.0f / (hcount - 1);


		// 頂点設定
		icount = 0;
		var meshScale = new Vector2(1.0f, 1.0f);
		for (int y = 0; y < hcount; y++)
		{
			for (int x = 0; x < wcount; x++)
			{
				point[icount] = new Vector2(x * xscale, y * yscale) * meshScale;
				icount++;
			}
		}


#if false
		// UV設定
		icount = 0;
		for (int y = 0; y < hcount; y++)
		{
			for (int x = 0; x < wcount; x++)
			{
				uv[icount] = new Vector2(x * xscale, y * yscale);
				icount++;
			}
		}
#endif

		this.Mesh.vertices = point;
		this.Mesh.uv = this.LenzGridUV(wcount, hcount).ToArray();
		this.Mesh.triangles = index;

		return true;
	}


	// uv
	private List<Vector2> LenzGridUV(int capture_width, int capture_height)
	{
		// ワーピング用データを準備する
		var cx = (capture_width - 1) / 2.0f;
		var cy = (capture_height - 1) / 2.0f;
		var k = new[] { 1.0f, 0.18f, 0.115f };  // 樽型


		var uvScale = new Vector2(1.0f / (capture_width - 1), 1.0f / (capture_height - 1));

		var map = new List<Vector2>();
		for (int i = 0; i < capture_width * capture_height; ++i)
		{
			var x = (i % capture_width) - cx;
			var y = (i / capture_width) - cy;
			var dx = x / cy;
			var dy = y / cy;
			var d2 = dx * dx + dy * dy;
			var d4 = d2 * d2;
			var t = (k[0] + k[1] * d2 + k[2] * d4);

			map.Add(new Vector2(x * t + cx, y * t + cy) * uvScale);
		}


		return map;
	}



}
