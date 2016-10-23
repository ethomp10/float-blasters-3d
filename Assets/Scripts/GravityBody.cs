using UnityEngine;
using System.Collections;

public class GravityBody : MonoBehaviour {

    public GameObject[] attractors;
    public bool allowQuantumDrive = true;

    private Transform position;
    private ShipControl shipControl;
    private float distanceToAttractor;

    // Use this for initialization
    void Start() {
        position = GetComponent<Transform>();
        shipControl = GetComponent<ShipControl>();
        attractors = GameObject.FindGameObjectsWithTag("Planet");
    }

    void Update() {
        for (int i = 0; i < attractors.Length; i++) {
            distanceToAttractor = attractors[i].GetComponent<GravityAttractor>().GetDistanceToBody(position);
            if (distanceToAttractor <= 1000f) {
                allowQuantumDrive = false;
                if (shipControl.stage == ShipControl.FLIGHT_STATE.STAGE_2) {
                    shipControl.SetStage(ShipControl.FLIGHT_STATE.STAGE_1); // Emergency drop
                    Debug.Log("Flight Stage 1 (safety override)");
                }
            } else {
                allowQuantumDrive = true;
            }
        }
    }

    void FixedUpdate() {
        if (!shipControl.flightAssist) {
            for (int i = 0; i < attractors.Length; i++) {
                attractors[i].GetComponent<GravityAttractor>().Attract(position);
            }
        }
    }
}
