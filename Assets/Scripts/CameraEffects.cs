using UnityEngine;
using System.Collections;

public class CameraEffects : MonoBehaviour {

    public float astroFOV = 65f;
    public float quantumFOV = 120f;
    public bool firstPerson = false;

    private ShipControl shipControl;
    private Camera cam;
    private Transform camPos;
    private Vector3 firstPersonPos;
    private Vector3 ThirdPersonPos;

    // Use this for initialization
    void Start () {
        cam = GetComponent<Camera>();
        camPos = GetComponent<Transform>();
        shipControl = GetComponentInParent<ShipControl>();

        firstPersonPos = new Vector3(0f, 1.36f, -2.81f);
        ThirdPersonPos = new Vector3(0f, 3.8f, -12.7f);
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetButtonDown("Toggle Camera")) {
            if (firstPerson) {
                firstPerson = false;
            } else {
                firstPerson = true;
            }
        }

        if (shipControl.stage == ShipControl.FLIGHT_STATE.QUANTUM) {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, quantumFOV, 0.5f * Time.deltaTime);
        } else {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, astroFOV, 1f * Time.deltaTime);
        }

        if (firstPerson) {
            camPos.localPosition = Vector3.Lerp(camPos.localPosition, firstPersonPos, 5f * Time.deltaTime);
        } else {
            camPos.localPosition = Vector3.Lerp(camPos.localPosition, ThirdPersonPos, 5f * Time.deltaTime);
        }
    }
}
