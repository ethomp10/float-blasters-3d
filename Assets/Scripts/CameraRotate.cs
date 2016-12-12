using UnityEngine;
using System.Collections;

public class CameraRotate : MonoBehaviour {

    private Transform camPos;

	// Use this for initialization
	void Start () {
        camPos = transform;
	}
	
	// Update is called once per frame
	void Update () {
        camPos.Rotate(new Vector3(0f, 0.1f, 0f));
	}
}
