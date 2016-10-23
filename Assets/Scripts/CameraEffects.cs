using UnityEngine;
using System.Collections;

public class CameraEffects : MonoBehaviour {

    public float astroFOV = 65f;
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
        if (shipControl.stage == ShipControl.FLIGHT_STATE.QUANTUM) {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, quantumFOV, 0.5f * Time.deltaTime);
        } else {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, astroFOV, 1f * Time.deltaTime);
        }
    }
}
