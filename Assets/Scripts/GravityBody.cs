using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GravityBody : MonoBehaviour {

    public GameObject[] attractors;
    public bool allowQuantum = false;
    public Image uiQuantumReady;

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
                allowQuantum = false;
                if (shipControl.stage == ShipControl.FLIGHT_STATE.QUANTUM) {
                    shipControl.SetStage(ShipControl.FLIGHT_STATE.ASTRO); // Emergency drop
                    Debug.Log("Astro Flight engaged (saftey override)");
                }
                break;
            } else {
                allowQuantum = true;
            }
        }

        // Quantum Flight  light
        if (allowQuantum) {
            uiQuantumReady.color = Color.green;
        } else {
            uiQuantumReady.color = Color.red;
        }
    }

    void FixedUpdate() {
        if (shipControl.stage == ShipControl.FLIGHT_STATE.ASST_OFF) {
            for (int i = 0; i < attractors.Length; i++) {
                attractors[i].GetComponent<GravityAttractor>().Attract(position);
            }
        }
    }
}
