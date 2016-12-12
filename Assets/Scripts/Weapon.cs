using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

    // Public
    public float fireRate = 60f;
    public float weaponDamage = 5f;
    public Transform[] firePoints;

    // Private
    private bool canFire = true;

	// Use this for initialization
	void Start () {
        //firePoints = GetComponentsInChildren<Transform>(false);
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
                Ray laser = new Ray(firePoint.position, firePoint.forward);
                RaycastHit hit;

                if (Physics.Raycast(laser, out hit)) {
                    //Debug.Log("Hit " + hit.collider.gameObject);

                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
                        hit.collider.gameObject.GetComponent<EnemyAI>().Damage(weaponDamage);
                    }
                }

                Debug.DrawRay(laser.origin, laser.direction * 1000f, Color.red, 3f);
            }
            StartCoroutine(CapFireRate()); 
        }
    }

    IEnumerator CapFireRate() {
        canFire = false;

        yield return new WaitForSeconds(60f / fireRate);

        canFire = true;
    }
}
