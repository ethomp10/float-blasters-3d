using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour {

    public float minScale = 15f;
    public float maxScale = 20f;

    private Transform portalTransform;
    private bool grow;

    void Start() {
        portalTransform = GetComponent<Transform>();
        grow = true;
    }

    void Update() {
        if (grow) {
            portalTransform.localScale = Vector3.Slerp(portalTransform.localScale, portalTransform.localScale * 2f, 0.2f * Time.deltaTime);
            if (portalTransform.localScale.magnitude >= maxScale) {
                grow = false;
            }
        } else {
            portalTransform.localScale = Vector3.Slerp(portalTransform.localScale, portalTransform.localScale * 0.2f, 0.2f * Time.deltaTime);
            if (portalTransform.localScale.magnitude <= minScale) {
                grow = true;
            }
        }
    }
}
