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
    public Color defaultEngineColor = new Color(0f, 1f, 1f);
    public Color quantumEngineColor = new Color(0f, 1f, 0f);
    public Color uiActive = new Color(0f, 1f, 1f);
    public Color uiInactive = new Color(50 / 255f, 50 / 255f, 50 / 255f);

    public Text uiAsstOff;
    public Text uiAstro;
    public Text uiQuantum;
    public Image uiQuantumReady;

    public enum FLIGHT_STATE { ASST_OFF, ASTRO, QUANTUM };
    public FLIGHT_STATE stage = FLIGHT_STATE.ASTRO;

    // Private Variables
	private Vector3 netForce;
	private Vector3 netTorque;
    private float distanceToAttractor;
    private bool allowQuantum;

    private Rigidbody shipRB;
    private Transform position;
    private GravityBody shipGB;
    private GravityAttractor[] planets;
    private GameObject[] engineLights;
    private ParticleSystem quantumParticles;
    private SkinnedMeshRenderer shipMesh;
    private float currentEngineState;

    void Start() {
        Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

        // Set up ship
		netForce = Vector3.zero;
		netTorque = Vector3.zero;
        shipRB = GetComponent<Rigidbody>();
        shipRB.maxAngularVelocity = maxRotationSpeed;
        position = GetComponent<Transform>();

        shipGB = GetComponent<GravityBody>();
        planets = FindObjectsOfType<GravityAttractor>();

        engineLights = GameObject.FindGameObjectsWithTag("EngineGlow");
        quantumParticles = GetComponentInChildren<ParticleSystem>();
        shipMesh = GetComponentInChildren<SkinnedMeshRenderer>();

        SetStage(stage);
    }

    void Update() {
        // Flight Assist stuff
        if (Input.GetButtonDown("Toggle Flight Assist")) {
			if (stage == FLIGHT_STATE.ASTRO) {
                SetStage(FLIGHT_STATE.ASST_OFF);
			} else {
                SetStage(FLIGHT_STATE.ASTRO);
			}
        }

        if (Input.GetButtonDown("Astro Flight")) {
            SetStage(FLIGHT_STATE.ASTRO);
        }

        if (Input.GetButtonDown("Quantum Flight")) {
            if (allowQuantum) {
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

        if (GetThrust() == Vector3.zero) {
            if (stage == FLIGHT_STATE.ASTRO) {
                shipRB.drag = 1f;
            } else {
                shipRB.drag = 0f;
            }
        } 

        // Emergency drop
        for (int i = 0; i < planets.Length; i++) {
            distanceToAttractor = planets[i].GetDistanceToBody(position);
            if (distanceToAttractor <= 1000f) {
                allowQuantum = false;
                if (stage == ShipControl.FLIGHT_STATE.QUANTUM) {
                    SetStage(ShipControl.FLIGHT_STATE.ASTRO);
                    Debug.Log("Astro Flight engaged (saftey override)");
                }
                break;
            } else {
                allowQuantum = true;
            }
        }

        // Quantum Flight  light
        if (allowQuantum) {
            uiQuantumReady.color = Color.green;
        } else {
            uiQuantumReady.color = Color.red;
        }

        // Engine Glow
        if (stage == FLIGHT_STATE.QUANTUM) {
            foreach (GameObject light in engineLights) {
                light.GetComponent<Light>().intensity = Mathf.Lerp(light.GetComponent<Light>().intensity, 8f, 1f * Time.deltaTime);
                light.GetComponent<Light>().color = Color.Lerp(light.GetComponent<Light>().color, quantumEngineColor, 0.5f * Time.deltaTime);
            }
        } else {
            foreach (GameObject light in engineLights) {
                light.GetComponent<Light>().intensity = Mathf.Lerp(light.GetComponent<Light>().intensity, GetThrust().z * 3f + 1f, 0.5f * Time.deltaTime);
                light.GetComponent<Light>().color = Color.Lerp(light.GetComponent<Light>().color, defaultEngineColor, 0.5f * Time.deltaTime);
            }
        }

        // Quantum Partocles
        if (stage == FLIGHT_STATE.QUANTUM) {
            quantumParticles.Play();
        } else {
            quantumParticles.Stop();
        }

        // Engine throttle animation
        currentEngineState = shipMesh.GetBlendShapeWeight(0);

        if (stage == FLIGHT_STATE.QUANTUM) {
            shipMesh.SetBlendShapeWeight(0, Mathf.Lerp(currentEngineState, 100f, 1f * Time.deltaTime));
        } else {
            shipMesh.SetBlendShapeWeight(0, Mathf.Lerp(currentEngineState, GetThrust().z * 100f, 1f * Time.deltaTime));
        }


        
    }

    // Physics
    void FixedUpdate() {

		// Get forces from input
		netForce = GetThrust() * enginePower;
		netForce.z *= 2f; // Give primary thrusters more power than the rest
		netTorque = GetTorque() * boosterPower;

        if (stage > 0 && netTorque == Vector3.zero) {
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
                maxSpeed = quantumSpeed;
                enginePower = 200f;
                boosterPower = 100f;
                shipGB.enabled = true;
                uiAsstOff.color = uiActive;
                uiAstro.color = uiInactive;
                uiQuantum.color = uiInactive;
                Debug.Log("Flight Assist off");
                break;
            }
            case FLIGHT_STATE.ASTRO: {
                maxSpeed = 50f;
                enginePower = 200f;
                boosterPower = 100f;
                shipGB.enabled = false;
                uiAsstOff.color = uiInactive;
                uiAstro.color = uiActive;
                uiQuantum.color = uiInactive;
                Debug.Log("Astro Flight engaged");
                break;
            }
            case FLIGHT_STATE.QUANTUM: {
                maxSpeed = quantumSpeed;
                enginePower = 0f;
                boosterPower = 30f;
                shipGB.enabled = false;
                uiAsstOff.color = uiInactive;
                uiAstro.color = uiInactive;
                uiQuantum.color = uiActive;
                Debug.Log("Quantum Flight engaged");
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
