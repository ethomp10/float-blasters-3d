using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	private GravityBody shipGB;

	// Use this for initialization
	void Start () {
		shipGB = GetComponent<GravityBody> ();
	}
	
	// Update is called once per frame
	void Update () {
		shipGB.FindClosestAttractor ().SetBodyOrientation (transform);
	}
}
