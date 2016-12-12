using UnityEngine;
using System.Collections;

public class CameraEffects : MonoBehaviour {

    // Public
    public float astroFOV = 65f;
    public float quantumFOV = 120f;
    public float camRotationSpeed;

    // Private
    private Camera cam;
    private Transform player;
    private ShipControl shipControl;

    private Transform targetCamPos;
    private Transform camPos;

    private Vector3 initialOffset;
    private Vector3 currentOffset;



    void Start() {

        cam = GetComponent<Camera>();
        camPos = GetComponent<Transform>();
        targetCamPos = camPos;

        player = GetComponentInParent<Transform>();
        shipControl = GetComponentInParent<ShipControl>();
    }

    // Update is called once per frame
    void Update() {
        //camPos.localPosition = Vector3.Lerp(camPos.localPosition);

        if (shipControl.stage == ShipControl.FLIGHT_STATE.QUANTUM) {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, quantumFOV, 0.5f * Time.deltaTime);
        } else {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, astroFOV, 1f * Time.deltaTime);
        }
    }
}
