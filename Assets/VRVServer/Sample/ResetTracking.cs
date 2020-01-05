using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTracking : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetMouseButtonDown(1))
		{
			VRVController.SendResetTrackingCenter();
		}
	}
}
