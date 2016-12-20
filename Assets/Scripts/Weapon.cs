using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

    // Public
    public bool canFire = true;
    public float fireRate = 100f;
    public float weaponDamage = 10f;
    public Transform[] firePoints;
    public LineRenderer[] crosshairs;

    // Private
    private AudioSource speaker;
    private LineRenderer[] lasers;
    private float laserWidth = 0.0f;

    // Use this for initialization
    void Start() {
        speaker = GetComponent<AudioSource>();

        // Assign each laser to the correct gun
        lasers = new LineRenderer[firePoints.Length];
        for (int i = 0; i < firePoints.Length; i++) {
            lasers[i] = firePoints[i].GetComponent<LineRenderer>();
        }
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Fire")) {
            Fire();
        }

        foreach (LineRenderer laser in lasers) {
            laser.SetWidth(laserWidth, laserWidth);
        }

        if (laserWidth > 0) {
            laserWidth -= 0.005f;
        } else {
            laserWidth = 0;
        }
    }

    void Fire() {

        if (canFire) {
            // Raycast
            for (int i = 0; i < firePoints.Length; i++) {
                Ray beam = new Ray(firePoints[i].position, firePoints[i].forward);
                RaycastHit hit;

                if (Physics.Raycast(beam, out hit)) {

                    Debug.DrawRay(beam.origin, beam.direction * 1000f, Color.red, 3f);
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
                        hit.collider.gameObject.GetComponent<EnemyAI>().Damage(weaponDamage);
                    }

                    if (hit.collider.GetComponent<Rigidbody>()) {
                        hit.collider.GetComponent<Rigidbody>().AddForceAtPosition(beam.direction * 500f, hit.point);
                    }

                    lasers[i].SetPosition(1, new Vector3(0f, 0f, hit.distance));
                } else {
                    lasers[i].SetPosition(1, new Vector3(0f, 0f, 400f));
                }
            }

            // Draw lasers
            laserWidth = 0.1f;

            // Laser sound
            speaker.Play();
            StartCoroutine(CapFireRate());
        }
    }

    IEnumerator CapFireRate() {
        canFire = false;
        yield return new WaitForSeconds(60f / fireRate);
        canFire = true;
    }
}
