using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {
    // Public
    public float runSpeed = 10f;
    public float jumpHeight = 10f;
    public float lookSensitivity = 5f;
    public Transform camPos;

    // Private
    private Rigidbody playerRB;
    private GravityBody playerGB;

    // Use this for initialization
    void Start() {
        playerRB = GetComponent<Rigidbody>();
        playerGB = GetComponent<GravityBody>();
    }

    // Update is called once per frame
    void Update() {
        Move();
        Rotate();
        if (Input.GetButtonDown("Jump")) {
            Jump();
        }
    }

    void Move() {
        float zInfluence = Input.GetAxis("Move Forward/Back");
        float xInfluence = Input.GetAxis("Move Right/Left");
        Vector3 moveDirection = (transform.forward * zInfluence + transform.right * xInfluence).normalized;
        Vector3 targetPosition = moveDirection * runSpeed;

        playerRB.MovePosition(transform.position + targetPosition * Time.deltaTime);

    }

    void Rotate() {
        float yInfluence = Input.GetAxis("Look Right/Left") * lookSensitivity;
        float xInfluence = Input.GetAxis("Look Up/Down") * lookSensitivity;

        playerGB.FindClosestAttractor().SetBodyOrientation(transform);
        transform.Rotate(0f, yInfluence, 0f);
        camPos.Rotate(xInfluence, 0f, 0f);
    }

    void Jump() {
        playerRB.AddForce(transform.up * jumpHeight, ForceMode.VelocityChange);
    }
}
