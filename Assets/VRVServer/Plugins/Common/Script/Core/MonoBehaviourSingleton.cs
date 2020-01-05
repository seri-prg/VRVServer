using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance = null;

	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				var obj = new GameObject(typeof(T).Name);
				_instance = obj.AddComponent<T>();

				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			return _instance;
		}
	}


	// インスタンスが存在するなら処理を実行
	// 主にOnDestory時の解放処理用
	public static void DoIfActive(Action<T> func)
	{
		if (_instance == null)
			return;

		func?.Invoke(_instance);
	}


	public static void Create()
	{
		var i = MonoBehaviourSingleton<T>.Instance;
	}
}
