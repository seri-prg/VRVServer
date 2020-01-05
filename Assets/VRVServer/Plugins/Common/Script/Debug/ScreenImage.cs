using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenImage : MonoBehaviour
{
	private Texture2D _texture;
	private int _imageAnim = 0;
	// Start is called before the first frame update
	void Start()
    {
		var render = this.GetComponent<SpriteRenderer>();


		_texture = new Texture2D(640, 480, TextureFormat.BGRA32, false);

#if true
		var sprite = Sprite.Create(_texture,
				new Rect(0.0f, 0.0f, _texture.width, _texture.height),
				new Vector2(0.5f, 0.5f),
				1.0f);  // 1メートルあたりのピクセル数

		render.sprite = sprite;
#endif


		// 縦のサイズ(この値×２メートル)
		Camera.main.orthographicSize = _texture.height / 2;
	}


	public void UpdateImage()
	{
		var pix = new Color32[_texture.width * _texture.height];

		var index = 0;
		for (int y = 0; y < _texture.height; y++)
		{
			for (int x = 0; x < _texture.width; x++)
			{
				var r = (int)(((float)x / _texture.width) * 255.0f);
				var g = (int)(((float)y / _texture.height) * 255.0f);
				var b = _imageAnim;

				pix[index] = new Color32((byte)r, (byte)g, (byte)b, 255);
				index++;
			}
		}

		_texture.SetPixels32(pix);
		_texture.Apply();

		_imageAnim++;

	}




	// Update is called once per frame
	void Update()
    {
		this.UpdateImage();
    }
}
