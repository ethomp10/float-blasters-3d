using UnityEngine;
using System.Collections;

public class GravityBody : MonoBehaviour {

    private GravityAttractor[] attractors;
    private Transform position;
    private float distanceToAttractor;

    void Start() {
        position = GetComponent<Transform>();
        attractors = FindObjectsOfType<GravityAttractor>();
    }

    void FixedUpdate() {
        for (int i = 0; i < attractors.Length; i++) {
            attractors[i].Attract(position);
        }
    }
}
