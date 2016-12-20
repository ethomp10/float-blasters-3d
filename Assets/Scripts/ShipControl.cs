using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShipControl : MonoBehaviour {

    // Public Variables
    public float defaultPower = 250f;
    public float hoverPower = 100f;
    public float astroSpeed = 200f;
    public float quantumSpeed = 2000f;
    public float maxRotationSpeed = 1.5f;

    public Light[] engineLights;
    public Color defaultEngineColour = new Color(0f, 1f, 1f);
    public Color quantumEngineColour = new Color(0f, 1f, 0f);

    public Color uiActive = new Color(0f, 1f, 1f);
    public Color uiInactive = new Color(50 / 255f, 50 / 255f, 50 / 255f);
    public Text uiAsstOff;
    public Text uiHover;
    public Text uiAstro;
    public Text uiQuantum;
    public Image uiQuantumLED;

    public enum FLIGHT_MODE { ASST_OFF, HOVER, ASTRO, QUANTUM };
    public FLIGHT_MODE currentMode = FLIGHT_MODE.ASST_OFF;

    // Private Variables
    private float enginePower; // Affects net force
    private float boosterPower; // Affects net torque
    private float maxSpeed;

    private Vector3 netForce;
    private Vector3 netTorque;
    private float distanceToAttractor;
    private bool allowQuantum;

    private Transform position;
    private Rigidbody shipRB;
    private GravityBody shipGB;
    private GravityAttractor[] planets;
    private ParticleSystem quantumParticles;
    private SkinnedMeshRenderer engineMesh;
    private float currentEngineAperture;

    private FLIGHT_MODE activeMode;

    void Start() {
        // Set up ship
        shipRB = GetComponent<Rigidbody>();
        shipRB.maxAngularVelocity = maxRotationSpeed;
        position = GetComponent<Transform>();

        shipGB = GetComponent<GravityBody>();
        planets = FindObjectsOfType<GravityAttractor>();

        quantumParticles = GetComponentInChildren<ParticleSystem>();
        engineMesh = GetComponentInChildren<SkinnedMeshRenderer>();

        SetStage(currentMode);
        if (currentMode == FLIGHT_MODE.ASST_OFF) {
            activeMode = FLIGHT_MODE.HOVER;
        } else {
            activeMode = currentMode;
        }
    }

    void Update() {
        // Input
        netForce = GetForce() * enginePower;
        netTorque = GetTorque() * boosterPower;

        if (Input.GetButtonDown("Toggle Flight Assist")) {
            if (currentMode == FLIGHT_MODE.ASST_OFF) {
                SetStage(activeMode);
            } else if (currentMode == FLIGHT_MODE.QUANTUM) {
                SetStage(FLIGHT_MODE.HOVER);
            } else {
                SetStage(FLIGHT_MODE.ASST_OFF);
            }
        }

        if (Input.GetButtonDown("Hover Mode")) {
            SetStage(FLIGHT_MODE.HOVER);
        }

        if (Input.GetButtonDown("Astro Flight")) {
            SetStage(FLIGHT_MODE.ASTRO);
        }

        if (Input.GetButtonDown("Quantum Flight")) {
            if (allowQuantum) {
                SetStage(FLIGHT_MODE.QUANTUM);
            } else {
                Debug.Log("Too close for Quantum Flight");
            }
        }

        // Flight assist
        switch (currentMode) {
            case FLIGHT_MODE.ASST_OFF:
                AssistOff();
                break;
            case FLIGHT_MODE.HOVER:
                Hover();
                break;
            case FLIGHT_MODE.ASTRO:
                Astro();
                break;
            case FLIGHT_MODE.QUANTUM:
                Quantum();
                break;
            default:
                break;
        }

        // Cap velocity
        if (GetSpeed() > maxSpeed) {
            shipRB.velocity = Vector3.Lerp(shipRB.velocity, shipRB.velocity.normalized, Time.deltaTime);
        }

        // Check distance to planets
        for (int i = 0; i < planets.Length; i++) {
            distanceToAttractor = planets[i].GetDistanceToBody(position);
            if (distanceToAttractor <= 1000f) {
                allowQuantum = false;
                uiQuantumLED.color = Color.red;
                break;
            } else {
                allowQuantum = true;
                uiQuantumLED.color = Color.green;
            }
        }
    }

    void AssistOff() {
        SyncThrottleLights(3f, 1f, defaultEngineColour);
        SyncThrottleAperture(GetForce().z * 100f);
        netForce.z *= 2f;
    }

    void Hover() {
        SyncThrottleLights(1f, 1f, defaultEngineColour);
        SyncThrottleAperture(GetForce().z * 50f);
    }

    void Astro() {
        SyncThrottleLights(3f, 3f, defaultEngineColour);
        SyncThrottleAperture(maxSpeed / astroSpeed * 100f);

        // Increase/decrease throttle
        if (GetForce().z > 0f) {
            if (maxSpeed < astroSpeed) {
                maxSpeed += GetForce().z;
            } else {
                maxSpeed = astroSpeed;
            }
        } else if (GetForce().z < 0f) {
            if (maxSpeed > 10f) {
                maxSpeed += GetForce().z;
            } else {
                maxSpeed = 10f;
            }
        }

        shipRB.velocity = Vector3.Lerp(shipRB.velocity, transform.forward * maxSpeed, 0.8f * Time.deltaTime);
    }

    void Quantum() {
        SyncThrottleLights(0f, 8f, quantumEngineColour);
        shipRB.velocity = Vector3.Lerp(shipRB.velocity, transform.forward * quantumSpeed, 0.5f * Time.deltaTime);

        if (!allowQuantum) {
            SetStage(FLIGHT_MODE.HOVER); // Emergency drop
        }
    }

    // Physics
    void FixedUpdate() {
        shipRB.AddRelativeForce(netForce);
        shipRB.AddRelativeTorque(netTorque);
    }

    void SetStage(FLIGHT_MODE nextMode) {
        switch (nextMode) {
            case FLIGHT_MODE.ASST_OFF:
                maxSpeed = quantumSpeed;
                enginePower = defaultPower;
                boosterPower = 100f;

                shipGB.enabled = true;
                shipRB.drag = 0f;
                shipRB.angularDrag = 0f;

                quantumParticles.Stop();
                uiAsstOff.color = uiActive;
                uiHover.color = uiInactive;
                uiAstro.color = uiInactive;
                uiQuantum.color = uiInactive;

                Debug.Log("Flight Assist off");
                break;
            case FLIGHT_MODE.HOVER:
                maxSpeed = 10f;
                enginePower = hoverPower;
                boosterPower = 100f;

                shipGB.enabled = false;
                shipRB.drag = 1f;
                shipRB.angularDrag = 2f;

                quantumParticles.Stop();
                uiAsstOff.color = uiInactive;
                uiHover.color = uiActive;
                uiAstro.color = uiInactive;
                uiQuantum.color = uiInactive;

                Debug.Log("Hover Flight engaged");
                break;
            case FLIGHT_MODE.ASTRO:
                maxSpeed = 10f;
                enginePower = 0f;
                boosterPower = 100f;

                shipGB.enabled = false;
                shipRB.drag = 0f;
                shipRB.angularDrag = 2f;

                quantumParticles.Stop();
                uiAsstOff.color = uiInactive;
                uiHover.color = uiInactive;
                uiAstro.color = uiActive;
                uiQuantum.color = uiInactive;

                Debug.Log("Astro Flight engaged");
                break;
            case FLIGHT_MODE.QUANTUM:
                maxSpeed = quantumSpeed;
                enginePower = 0f;
                boosterPower = 30f;

                shipGB.enabled = false;
                shipRB.drag = 0f;
                shipRB.angularDrag = 2f;

                quantumParticles.Play();
                uiAsstOff.color = uiInactive;
                uiHover.color = uiInactive;
                uiAstro.color = uiInactive;
                uiQuantum.color = uiActive;

                Debug.Log("Quantum Flight engaged");
                break;
            default:
                break;
        }

        currentMode = nextMode;
        if (nextMode != FLIGHT_MODE.ASST_OFF) {
            activeMode = nextMode;
        }
    }

    Vector3 GetForce() {
        float horizontalThrust = Input.GetAxis("Horizontal Thrust");
        float verticalThrust = Input.GetAxis("Vertical Thrust");
        float primaryThrust = Input.GetAxis("Primary Thrust");

        return new Vector3(horizontalThrust, verticalThrust, primaryThrust);
    }

    Vector3 GetTorque() {
        float pitch = Input.GetAxis("Pitch");
        float yaw = Input.GetAxis("Yaw");
        float roll = Input.GetAxis("Roll");

        return new Vector3(pitch, yaw, roll);
    }

    public float GetSpeed() {
        return shipRB.velocity.magnitude;
    }

    // Engine lights
    void SyncThrottleLights(float intensity, float idle, Color colour) {
        foreach (Light engineLight in engineLights) {
            engineLight.intensity = Mathf.Lerp(engineLight.intensity, GetForce().z * intensity + idle, 0.5f * Time.deltaTime);
            engineLight.color = Color.Lerp(engineLight.color, defaultEngineColour, 0.5f * Time.deltaTime);
        }
    }

    // Engine aperture
    void SyncThrottleAperture(float size) {
        currentEngineAperture = engineMesh.GetBlendShapeWeight(0);
        engineMesh.SetBlendShapeWeight(0, Mathf.Lerp(currentEngineAperture, size, 1f * Time.deltaTime));
    }
}
