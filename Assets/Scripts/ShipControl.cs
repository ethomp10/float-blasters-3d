using UnityEngine;
using System.Collections;

public class ShipControl : MonoBehaviour {

    public float enginePower = 200f; // Affects net force
    public float boosterPower = 100f; // Affects net torque
    public float maxSpeed = 50f;
    public float maxRotationSpeed = 20f;
    public bool flightAssist = true;

    public enum FLIGHT_STATE {ASST_OFF, STAGE_1, STAGE_2};
    public FLIGHT_STATE stage; 

    private Rigidbody shipRB;
    private GravityBody gravBody;
    private GameObject[] engineLights;
    
    void Start() {
        Cursor.visible = false;
        shipRB = GetComponent<Rigidbody>();
        shipRB.maxAngularVelocity = maxRotationSpeed / 10f;
        gravBody = GetComponent<GravityBody>();
        SetStage(FLIGHT_STATE.STAGE_1);
        engineLights = GameObject.FindGameObjectsWithTag("EngineLight");
    }

    void Update() {

        // Flight Assist stuff
        if (Input.GetButtonDown("Toggle Flight Assist")) {
            if (flightAssist) {
                SetStage(FLIGHT_STATE.ASST_OFF);
            } else {
                SetStage(FLIGHT_STATE.STAGE_1);
            }
        }

        if (flightAssist && Input.GetButtonDown("Flight Stage 1")) {
            SetStage(FLIGHT_STATE.STAGE_1);
        }

        if (flightAssist && Input.GetButtonDown("Flight Stage 2")) {
            if (gravBody.allowQuantumDrive) {
                SetStage(FLIGHT_STATE.STAGE_2);
            }
        }

        if (GetSpeed() > maxSpeed) {
            shipRB.velocity = Vector3.Slerp(shipRB.velocity, shipRB.velocity.normalized * maxSpeed, 5f * Time.deltaTime);
        }

        // Chester is awesome!! 

        if (stage == FLIGHT_STATE.STAGE_2) {
            shipRB.velocity = Vector3.Slerp(shipRB.velocity, transform.forward * 1000f, 0.5f * Time.deltaTime);
        }

        if (flightAssist && GetThrust() == Vector3.zero) {
            if (stage == FLIGHT_STATE.STAGE_1) {
                shipRB.drag = 1f;
            } else if (stage >= FLIGHT_STATE.STAGE_2) {
                shipRB.drag = 0.1f;
            }
        } else {
            shipRB.drag = 0f;
        }

        // Engine Lights
        foreach (GameObject light in engineLights) {
            light.GetComponent<Light>().intensity = GetThrust().z * 5f;
        }
    }

    // Physics
    void FixedUpdate() {

        Vector3 netForce = GetThrust() * enginePower;
        netForce.z *= 2f; // Give primary thrusters more power than the rest
        Vector3 netTorque = GetTorque() * boosterPower;

        if (flightAssist && netTorque == Vector3.zero) {
            shipRB.angularDrag = 2f;
        } else {
            shipRB.angularDrag = 0f;
        }

        shipRB.AddRelativeForce(netForce);
        shipRB.AddRelativeTorque(netTorque);
    }

    public void SetStage(FLIGHT_STATE stage) {
        switch (stage) {
            case FLIGHT_STATE.ASST_OFF: {
                Debug.Log("Flight Assist Off");
                flightAssist = false;
                maxSpeed = 1000000f;
                enginePower = 200f;
                break;
            }
            case FLIGHT_STATE.STAGE_1: {
                Debug.Log("Flight Stage 1");
                flightAssist = true;
                maxSpeed = 50f;
                enginePower = 200f;
                break;
            }
            case FLIGHT_STATE.STAGE_2: {
                Debug.Log("Flight Stage 2");
                maxSpeed = 1000000f;
                enginePower = 0f;
                break;
            }
            default:
            break;
        }

        this.stage = stage;
    }
    
    public Vector3 GetThrust() {
        float primaryThrust = Input.GetAxis("Primary Thrust");
        float verticalThrust = Input.GetAxis("Vertical Thrust");
        float horizontalThrust = Input.GetAxis("Horizontal Thrust");

        return new Vector3(horizontalThrust, verticalThrust, primaryThrust);
    }
    
    public Vector3 GetTorque() {
        float yaw = Input.GetAxis("Yaw");
        float roll = Input.GetAxis("Roll");
        float pitch = Input.GetAxis("Pitch");

        return new Vector3(pitch, yaw, roll);
    }

    public float GetSpeed() {
        return shipRB.velocity.magnitude;
    }
}
