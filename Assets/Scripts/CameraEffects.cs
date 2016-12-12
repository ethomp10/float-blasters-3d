using UnityEngine;
using System.Collections;

public class CameraEffects : MonoBehaviour {

    // Public
    public float astroFOV = 65f;
    public float quantumFOV = 120f;

    // Private
    private Camera cam;
    private ShipControl shipControl;

    void Start() {
        cam = GetComponent<Camera>();
        shipControl = GetComponentInParent<ShipControl>();
    }

    // Update is called once per frame
    void Update() {
        //camPos.localPosition = Vector3.Lerp(camPos.localPosition);

        if (shipControl.activeMode == ShipControl.FLIGHT_MODE.QUANTUM) {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, quantumFOV, 0.5f * Time.deltaTime);
        } else {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, astroFOV, 1f * Time.deltaTime);
        }
    }
}
