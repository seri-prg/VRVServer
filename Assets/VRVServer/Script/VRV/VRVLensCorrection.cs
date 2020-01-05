using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;



// レンズ補正
public class VRVLensCorrection
{
	private VRVLensMesh _lensMesh = new VRVLensMesh();
	private CommandBuffer _buffer = null;
	private Camera _camera = null;

	public RenderTexture LensTexture { get; private set; } = null;


	public void Setup(Camera camera)
	{
		_camera = camera;
		// レンズ用メッシュ作成
		_lensMesh.Create(20, 20);

		this.SetupCommandBuffer(Screen.width, Screen.height);
	}


	public void ResizeBufffer(int width, int height)
	{
		this.SetupCommandBuffer(width, height);
	}


	public void Remove()
	{
		// レンズ処理削除
		if ((_buffer != null) && (_camera != null))
		{
			_camera.RemoveCommandBuffer(CameraEvent.AfterEverything, _buffer);
		}
		_buffer = null;

		this.LensTexture?.Release();
		this.LensTexture = null;
	}


	private void SetupCommandBuffer(int width, int height)
	{
		this.Remove();


		// RenderTextureを作る
		LensTexture = new RenderTexture(width, height, 24, GraphicsFormat.R8G8B8A8_UNorm);
		
		LensTexture.Create();

		Debug.Log($"server frame tex = {this.LensTexture.width}, {this.LensTexture.height}");

		var shader = Shader.Find("Unlit/LensUnlitShader");
		var material = new Material(shader);

		// コマンドバッファを作成する
		_buffer = new CommandBuffer();
		_buffer.name = "LensCommand";


		int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
		_buffer.GetTemporaryRT(screenCopyID, -1, -1, 0, FilterMode.Bilinear, GraphicsFormat.R8G8B8A8_UNorm);
		_buffer.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyID);

		// 描画先テクスチャを現在のレンダリングカメラに指定
		//		commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
		_buffer.SetRenderTarget(LensTexture);


		_buffer.SetGlobalTexture("_MyTex", screenCopyID);
		_buffer.DrawMesh(_lensMesh.Mesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1)), material);

		_buffer.ReleaseTemporaryRT(screenCopyID);

		// カメラにコマンドバッファを登録する。
		_camera.AddCommandBuffer(CameraEvent.AfterEverything, _buffer);
	}



}
