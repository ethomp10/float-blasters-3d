using UnityEngine;
using System.Collections;

public class GravityAttractor : MonoBehaviour {

    public float gravityStrength = 1000f;

    private Vector3 attractorToBody;
    private Vector3 gravity;
    private float radius;

    public void Attract(Transform body) {
        attractorToBody = (body.position - GetComponent<Transform>().position);
        radius = GetDistanceToBody(body);
        gravity = (-attractorToBody.normalized * (gravityStrength * 50f / radius));
        body.GetComponent<Rigidbody>().AddForce(gravity);
    }

    public float GetDistanceToBody(Transform body) {
        attractorToBody = (body.position - GetComponent<Transform>().position);
        return attractorToBody.magnitude;
    }
}
