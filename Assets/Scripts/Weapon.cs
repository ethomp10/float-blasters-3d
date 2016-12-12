using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

    // Public
    public float fireRate = 60f;

    // Private
    private Transform[] firePoints;
    private bool canFire = true;

	// Use this for initialization
	void Start () {
        firePoints = GetComponentsInChildren<Transform>(false);
	}

    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Fire")) {
            Fire();
        }
    }

    void Fire() {

        if (canFire) {
            foreach (Transform firePoint in firePoints) {
                if (firePoint.GetInstanceID() != transform.GetInstanceID()) {
                    Ray laser = new Ray(firePoint.position, firePoint.forward);
                    RaycastHit hit;

                    if (Physics.Raycast(laser, out hit)) {
                        Debug.Log("Hit " + hit.collider.gameObject);
                    }

                    Debug.DrawRay(laser.origin, laser.direction * 1000f, Color.red, 3f);
                }
            }

            canFire = false;
            StartCoroutine(CapFireRate()); 
        }
    }

    IEnumerator CapFireRate() {
        yield return new WaitForSeconds(60f / fireRate);

        canFire = true;
    }
}
