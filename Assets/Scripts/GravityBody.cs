using UnityEngine;
using System.Collections;

public class GravityBody : MonoBehaviour {

    private GravitySource[] attractors;
    private Transform bodyTransform;
    //private float distanceToAttractor;

    void Start() {
        bodyTransform = GetComponent<Transform>();
        attractors = FindObjectsOfType<GravitySource>();
    }

    void FixedUpdate() {
        for (int i = 0; i < attractors.Length; i++) {
            attractors[i].Attract(bodyTransform);
        }
    }

    public GravitySource FindClosestAttractor() {
        int closestIndex = 0;
        float closestDist = float.MaxValue;
        float currentDist;
        for (int i = 0; i < attractors.Length; i++) {
            currentDist = attractors[i].GetDistanceToBody(bodyTransform);
            if (currentDist < closestDist) {
                closestDist = currentDist;
                closestIndex = i;
            }
        }
        return attractors[closestIndex];
    }
}
