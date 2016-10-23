using UnityEngine;
using System.Collections;

public class CameraEffects : MonoBehaviour {

    public float defaultFOV = 65f;
    public float quantumFOV = 120f;

    private Camera cam;
    private ShipControl shipControl;

	// Use this for initialization
	void Start () {
        cam = GetComponent<Camera>();
        shipControl = GetComponentInParent<ShipControl>();
	}
	
	// Update is called once per frame
	void Update () {
        if (shipControl.stage == ShipControl.FLIGHT_STATE.STAGE_2) {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, quantumFOV, 0.5f * Time.deltaTime);
        } else {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, defaultFOV, 1f * Time.deltaTime);
        }
    }
}
