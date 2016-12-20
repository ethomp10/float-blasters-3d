using UnityEngine;
using System.Collections;

public class CameraRotate : MonoBehaviour {

    public float rotationSpeed = 0.1f;
    private Transform camPos;

    // Use this for initialization
    void Start() {
        camPos = transform;
    }

    // Update is called once per frame
    void Update() {
        camPos.Rotate(new Vector3(0f, rotationSpeed, 0f));
    }
}
