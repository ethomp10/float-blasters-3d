using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShipControl : MonoBehaviour {

    // Public Variables
    public float enginePower = 200f; // Affects net force
    public float boosterPower = 100f; // Affects net torque
    public float maxSpeed = 50f;
    public float quantumSpeed = 2000f;
    public float maxRotationSpeed = 1.5f;

    public Light[] engineLights;
    public Color defaultEngineColor = new Color(0f, 1f, 1f);
    public Color quantumEngineColor = new Color(0f, 1f, 0f);

    public Color uiActive = new Color(0f, 1f, 1f);
    public Color uiInactive = new Color(50 / 255f, 50 / 255f, 50 / 255f);
    public Text uiAsstOff;
    public Text uiAstro;
    public Text uiQuantum;
    public Image uiQuantumReady;

    public enum FLIGHT_MODE { ASST_OFF, ASTRO, QUANTUM };
    public FLIGHT_MODE activeMode = FLIGHT_MODE.ASTRO;

    // Private Variables
	private Vector3 netForce;
	private Vector3 netTorque;
    private float distanceToAttractor;
    private bool allowQuantum;

    private Rigidbody shipRB;
    private Transform position;
    private GravityBody shipGB;
    private GravityAttractor[] planets;
    private ParticleSystem quantumParticles;
    private SkinnedMeshRenderer engineMesh;
    private float currentEngineState;

    void Start() {
        // Set up ship
		netForce = Vector3.zero;
		netTorque = Vector3.zero;
        shipRB = GetComponent<Rigidbody>();
        shipRB.maxAngularVelocity = maxRotationSpeed;
        position = GetComponent<Transform>();

        shipGB = GetComponent<GravityBody>();
        planets = FindObjectsOfType<GravityAttractor>();
        
        quantumParticles = GetComponentInChildren<ParticleSystem>();
        engineMesh = GetComponentInChildren<SkinnedMeshRenderer>();

        SetStage(activeMode);
    }

    void Update() {
        // Flight Assist stuff
        if (Input.GetButtonDown("Toggle Flight Assist")) {
			if (activeMode == FLIGHT_MODE.ASTRO) {
                SetStage(FLIGHT_MODE.ASST_OFF);
			} else {
                SetStage(FLIGHT_MODE.ASTRO);
			}
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
        
        if (activeMode == FLIGHT_MODE.QUANTUM) {
            shipRB.velocity = Vector3.Lerp(shipRB.velocity, transform.forward * quantumSpeed, 0.5f * Time.deltaTime);
        }

        // Cap velocity
        if (GetSpeed() > maxSpeed) {
            shipRB.velocity = Vector3.Lerp(shipRB.velocity, shipRB.velocity.normalized * maxSpeed, 5f * Time.deltaTime);
        }

        // Emergency drop
        for (int i = 0; i < planets.Length; i++) {
            distanceToAttractor = planets[i].GetDistanceToBody(position);
            if (distanceToAttractor <= 1000f) {
                allowQuantum = false;
                if (activeMode == FLIGHT_MODE.QUANTUM) {
                    SetStage(FLIGHT_MODE.ASTRO);
                    Debug.Log("Astro Flight engaged (saftey override)");
                }
                break;
            } else {
                allowQuantum = true;
            }
        }

        // Quantum Flight indicator light
        if (allowQuantum) {
            uiQuantumReady.color = Color.green;
        } else {
            uiQuantumReady.color = Color.red;
        }

        // Engine Glow
        if (activeMode == FLIGHT_MODE.QUANTUM) {
            foreach (Light engineLight in engineLights) {
                engineLight.intensity = Mathf.Lerp(engineLight.intensity, 8f, 1f * Time.deltaTime);
                engineLight.color = Color.Lerp(engineLight.color, quantumEngineColor, 0.5f * Time.deltaTime);
            }
        } else {
            foreach (Light engineLight in engineLights) {
                engineLight.intensity = Mathf.Lerp(engineLight.intensity, GetThrust().z * 3f + 1f, 0.5f * Time.deltaTime);
                engineLight.color = Color.Lerp(engineLight.color, defaultEngineColor, 0.5f * Time.deltaTime);
            }
        }

        // Quantum Partocles
        if (activeMode == FLIGHT_MODE.QUANTUM) {
            quantumParticles.Play();
        } else {
            quantumParticles.Stop();
        }

        // Engine throttle animation
        currentEngineState = engineMesh.GetBlendShapeWeight(0);

        if (activeMode == FLIGHT_MODE.QUANTUM) {
            engineMesh.SetBlendShapeWeight(0, Mathf.Lerp(currentEngineState, 100f, 1f * Time.deltaTime));
        } else {
            engineMesh.SetBlendShapeWeight(0, Mathf.Lerp(currentEngineState, GetThrust().z * 100f, 1f * Time.deltaTime));
        }
    }

    // Physics
    void FixedUpdate() {

		// Get forces from input
		netForce = GetThrust() * enginePower;
		netForce.z *= 2f; // Give primary thrusters more power than the rest
		netTorque = GetTorque() * boosterPower;

        shipRB.AddRelativeForce(netForce);
        shipRB.AddRelativeTorque(netTorque);
    }

    public void SetStage(FLIGHT_MODE activeMode) {
        switch (activeMode) {
            case FLIGHT_MODE.ASST_OFF: {
                maxSpeed = quantumSpeed;
                enginePower = 250f;
                boosterPower = 100f;
                shipGB.enabled = true;
                shipRB.drag = 0f;
                shipRB.angularDrag = 0f;
                uiAsstOff.color = uiActive;
                uiAstro.color = uiInactive;
                uiQuantum.color = uiInactive;
                Debug.Log("Flight Assist off");
                break;
            }
            case FLIGHT_MODE.ASTRO: {
                maxSpeed = 10f;
                enginePower = 100f;
                boosterPower = 100f;
                shipGB.enabled = false;
                shipRB.drag = 0.5f;
                shipRB.angularDrag = 2f;
                uiAsstOff.color = uiInactive;
                uiAstro.color = uiActive;
                uiQuantum.color = uiInactive;
                Debug.Log("Astro Flight engaged");
                break;
            }
            case FLIGHT_MODE.QUANTUM: {
                maxSpeed = quantumSpeed;
                enginePower = 0f;
                boosterPower = 30f;
                shipGB.enabled = false;
                shipRB.drag = 0f;
                shipRB.angularDrag = 2f;
                uiAsstOff.color = uiInactive;
                uiAstro.color = uiInactive;
                uiQuantum.color = uiActive;
                Debug.Log("Quantum Flight engaged");
                break;
            }
            default:
            break;
        }

        this.activeMode = activeMode;
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
