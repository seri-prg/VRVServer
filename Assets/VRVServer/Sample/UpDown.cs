using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDown : MonoBehaviour
{
	float _timeCounter = 0.0f;
	float _baseY = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
		_baseY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
		_timeCounter += Time.deltaTime;

		var pos = transform.position;
		pos.y = _baseY + Mathf.Sin(_timeCounter) * 1.5f;
		transform.position = pos;
	}
}
