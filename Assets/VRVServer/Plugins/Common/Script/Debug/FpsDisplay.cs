using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsDisplay : MonoBehaviour
{
	// 変数
	private int frameCount;
	private float prevTime;
	private float fps;

	private GUIStyle _style;


	// 初期化処理
	void Start()
	{
		_style = new GUIStyle();
		_style.fontSize = 30;
		_style.normal.textColor = Color.white;
		frameCount = 0;
		prevTime = 0.0f;
	}

	private void OnGUI()
	{
#if SHOW_DEBUG_INFO
		GUILayout.Label($"fps:{fps}", _style);
#endif
	}

	// 更新処理
	void Update()
	{
		frameCount++;
		float time = Time.realtimeSinceStartup - prevTime;

		if (time >= 0.5f)
		{
			fps = frameCount / time;
			frameCount = 0;
			prevTime = Time.realtimeSinceStartup;
		}
	}
}
