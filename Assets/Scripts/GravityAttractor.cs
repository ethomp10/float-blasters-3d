using UnityEngine;
using System.Collections;

public class GravityAttractor : MonoBehaviour {

    public float gravityStrength = 1000f;

    private Vector3 attractorToBody;
    private Vector3 gravity;
    private float radius;

    public void Attract(Transform body) {
		attractorToBody = GetVectorToBody(body);
        radius = GetDistanceToBody(body);
        gravity = (-attractorToBody.normalized * (gravityStrength * 10f / radius));
        body.GetComponent<Rigidbody>().AddForce(gravity, ForceMode.Acceleration);
    }

	public void SetBodyOrientation(Transform body) {
		attractorToBody = GetVectorToBody(body);
		body.transform.rotation = Quaternion.FromToRotation (body.transform.up, attractorToBody.normalized) * body.rotation;
	}

	private Vector3 GetVectorToBody(Transform body) {
		return (body.position - GetComponent<Transform> ().position);
	}

	public float GetDistanceToBody(Transform body) {
		attractorToBody = GetVectorToBody(body);
		return attractorToBody.magnitude;
	}
}
