using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShipControl : MonoBehaviour {

    public float enginePower = 200f; // Affects net force
    public float boosterPower = 100f; // Affects net torque
    public float maxSpeed = 50f;
    public float quantumSpeed = 2000f;
    public float maxRotationSpeed = 20f;
    public bool flightAssist = true;

    public Text uiAsstOff;
    public Text uiAstro;
    public Text uiQuantum;

    public enum FLIGHT_STATE {ASST_OFF, ASTRO, QUANTUM};
    public FLIGHT_STATE stage;

    private Rigidbody shipRB;
    private GravityBody gravBody;
    private GameObject[] engineLights;
    
    void Start() {
        Cursor.visible = false;

        // Set up ship
        shipRB = GetComponent<Rigidbody>();
        shipRB.maxAngularVelocity = maxRotationSpeed / 10f;
        gravBody = GetComponent<GravityBody>();
        SetStage(FLIGHT_STATE.ASST_OFF);
        engineLights = GameObject.FindGameObjectsWithTag("EngineGlow");
    }

    void Update() {

        // Flight Assist stuff
        if (Input.GetButtonDown("Toggle Flight Assist")) {
            if (flightAssist) {
                SetStage(FLIGHT_STATE.ASST_OFF);
            } else {
                SetStage(FLIGHT_STATE.ASTRO);
            }
        }

        if (flightAssist && Input.GetButtonDown("Flight Stage 1")) {
            SetStage(FLIGHT_STATE.ASTRO);
        }

        if (flightAssist && Input.GetButtonDown("Flight Stage 2")) {
            if (gravBody.allowQuantum) {
                SetStage(FLIGHT_STATE.QUANTUM);
            } else {
                Debug.Log("Too close for Quantum Flight");
            }
        }

        if (GetSpeed() > maxSpeed) {
            shipRB.velocity = Vector3.Slerp(shipRB.velocity, shipRB.velocity.normalized * maxSpeed, 5f * Time.deltaTime);
        }

        if (stage == FLIGHT_STATE.QUANTUM) {
            shipRB.velocity = Vector3.Slerp(shipRB.velocity, transform.forward * quantumSpeed, 0.5f * Time.deltaTime);
        }

        if (flightAssist && GetThrust() == Vector3.zero) {
            if (stage == FLIGHT_STATE.ASTRO) {
                shipRB.drag = 1f;
            } else if (stage == FLIGHT_STATE.QUANTUM) {
                shipRB.drag = 0f;
            }
        } else {
            shipRB.drag = 0f;
        }

        // Engine Glow
        if (stage == FLIGHT_STATE.QUANTUM) {
            foreach (GameObject light in engineLights) {
                light.GetComponent<Light>().intensity = Mathf.Lerp (light.GetComponent<Light>().intensity, 8f, 0.5f * Time.deltaTime);
            }
        } else {
            foreach (GameObject light in engineLights) {
                light.GetComponent<Light>().intensity = GetThrust().z * 3f;
            }
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
                Debug.Log("Flight Assist off");
                flightAssist = false;
                maxSpeed = quantumSpeed;
                enginePower = 200f;
                uiAsstOff.color = new Color(0f, 1f, 1f);
                uiAstro.color = new Color(50 / 255f, 50 / 255f, 50 / 255f);
                uiQuantum.color = new Color(50 / 255f, 50 / 255f, 50 / 255f);
                break;
            }
            case FLIGHT_STATE.ASTRO: {
                Debug.Log("Astro Flight engaged");
                flightAssist = true;
                maxSpeed = 50f;
                enginePower = 200f;
                uiAsstOff.color = new Color(50 / 255f, 50 / 255f, 50 / 255f);
                uiAstro.color = new Color(0f, 1f, 1f);
                uiQuantum.color = new Color(50 / 255f, 50 / 255f, 50 / 255f);
                break;
            }
            case FLIGHT_STATE.QUANTUM: {
                Debug.Log("Quantum Flight engaged");
                flightAssist = true;
                maxSpeed = quantumSpeed;
                enginePower = 0f;
                uiAsstOff.color = new Color(50 / 255f, 50 / 255f, 50 / 255f);
                uiAstro.color = new Color(50 / 255f, 50 / 255f, 50 / 255f);
                uiQuantum.color = new Color(0f, 1f, 1f);
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
