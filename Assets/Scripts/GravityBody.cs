using UnityEngine;
using System.Collections;

public class GravityBody : MonoBehaviour {

    public GameObject[] attractors;
    public bool allowQuantumDrive = false;

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
                if (shipControl.stage == ShipControl.FLIGHT_STATE.QUANTUM) {
                    shipControl.SetStage(ShipControl.FLIGHT_STATE.ASTRO); // Emergency drop
                    Debug.Log("Astro Flight engaged (saftey override)");
                }
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
