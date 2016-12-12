using UnityEngine;
using System.Collections;

public class GravityBody : MonoBehaviour {

    private GravityAttractor[] attractors;
    private Transform bodyTransform;
    //private float distanceToAttractor;

    void Start() {
		bodyTransform = GetComponent<Transform>();
        attractors = FindObjectsOfType<GravityAttractor>();
    }

    void FixedUpdate() {
        for (int i = 0; i < attractors.Length; i++) {
			attractors[i].Attract(bodyTransform);
        }
    }

	public GravityAttractor FindClosestAttractor () {
		int closestIndex = 0;
		float closestDist = 1000000f;
		float currentDist;
		for (int i = 0; i < attractors.Length; i++) {
			currentDist = attractors [i].GetDistanceToBody (bodyTransform);
			if (currentDist < closestDist) {
				closestDist = currentDist;
				closestIndex = i;
			}
		}
		return attractors [closestIndex];
	}
}
