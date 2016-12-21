using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ShipControl : MonoBehaviour {

    // Public Variables
    public float enginePower = 2500;
    public float boosterPower = 1000f;
    public float astroSpeed = 200f;
    public float quantumSpeed = 2000f;
    public float maxRotationSpeed = 1.5f;

    public Light[] engineLights;
    public Color defaultEngineColour = new Color(0f, 1f, 1f);
    public Color quantumEngineColour = new Color(0f, 1f, 0f);

    public CanvasGroup healthPanel;
    public CanvasGroup throttlePanel;
    public CanvasGroup flightModePanel;
    public Color uiActive = new Color(0f, 1f, 1f);
    public Color uiInactive = new Color(50 / 255f, 50 / 255f, 50 / 255f);
    public Text uiAsstOff;
    public Text uiHover;
    public Text uiAstro;
    public Text uiQuantum;
    public Image uiQuantumLED;
    public Image uiHealth;
    public Image uiThrottle;

    public enum FLIGHT_MODE { ASST_OFF, HOVER, ASTRO, QUANTUM };
    public FLIGHT_MODE currentMode = FLIGHT_MODE.ASST_OFF;

    // Private Variables
    private float currentEnginePower; // Affects net force
    private float currentBoosterPower; // Affects net torque
    private float speedCap;
    private float capRate = 1f;
    private float throttle;

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
    private Coroutine healthFade = null;
    private Coroutine throttleFade = null;
    private Coroutine flightModeFade = null;

    void Start() {

        // TODO: Remove this later
        /////////////////////////////////////////
        Cursor.lockState = CursorLockMode.Locked;
        /////////////////////////////////////////

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

        healthFade = StartCoroutine(ShowHealth());
    }

    void Update() {
        // Input
        netForce = GetForce() * currentEnginePower;
        netTorque = GetTorque() * currentBoosterPower;
        netTorque.y *= 0.33f; // Slow down yaw

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
        if (GetSpeed() > speedCap) {
            shipRB.velocity = Vector3.Lerp(shipRB.velocity, shipRB.velocity.normalized, capRate * Time.deltaTime);
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
        netForce.z *= 2f;

        SyncEngineLights(3f, 1f, defaultEngineColour);
        SyncEngineAperture(GetForce().z * 100f);
    }

    void Hover() {
        SyncEngineLights(1f, 1f, defaultEngineColour);
        SyncEngineAperture(GetForce().z * 50f);
    }

    void Astro() {
        netForce.z = 0f;
        throttle = speedCap / astroSpeed;

        // Increase/decrease throttle
        if (GetForce().z > 0f) {
            // Show throttle UI
            if (throttleFade != null) {
                StopCoroutine(throttleFade);
            }
            throttleFade = StartCoroutine(ShowThrottle());
            // Adjust speed
            if (speedCap < astroSpeed) {
                speedCap += GetForce().z;
            } else {
                speedCap = astroSpeed;
            }
        } else if (GetForce().z < 0f) {
            // Show throttle UI
            if (throttleFade != null) {
                StopCoroutine(throttleFade);
            }
            throttleFade = StartCoroutine(ShowThrottle());
            // Adjust speed
            if (speedCap > astroSpeed * -0.5f) {
                speedCap += GetForce().z;
            } else {
                speedCap = astroSpeed * -0.5f;
            }
        }
        shipRB.velocity = Vector3.Lerp(shipRB.velocity, transform.forward * speedCap, 0.8f * Time.deltaTime);

        // Update visual effects
        SyncEngineLights(0f, throttle * 3f + 1, defaultEngineColour);
        SyncEngineAperture(throttle * 100f);

        // Update UI
        if (throttle > 0) {
            uiThrottle.color = Color.green;
            uiThrottle.fillAmount = throttle;
        } else {
            uiThrottle.color = Color.red;
            uiThrottle.fillAmount = -throttle;
        }
    }

    void Quantum() {
        // Emergency drop
        if (!allowQuantum) {
            SetStage(FLIGHT_MODE.HOVER);
        }

        shipRB.velocity = Vector3.Lerp(shipRB.velocity, transform.forward * quantumSpeed, 0.5f * Time.deltaTime);

        // Update visual effects
        SyncEngineLights(0f, 8f, quantumEngineColour);
        SyncEngineAperture(100f);
    }

    // Physics
    void FixedUpdate() {
        shipRB.AddRelativeForce(netForce);
        shipRB.AddRelativeTorque(netTorque);
    }

    void SetStage(FLIGHT_MODE nextMode) {
        // Show flight mode panel
        if (flightModeFade != null) {
            StopCoroutine(flightModeFade);
        }
        flightModeFade = StartCoroutine(ShowFlightMode());

        switch (nextMode) {
            case FLIGHT_MODE.ASST_OFF:
                speedCap = quantumSpeed;
                currentEnginePower = enginePower;
                currentBoosterPower = boosterPower;

                shipGB.enabled = true;
                shipRB.drag = 0f;
                shipRB.angularDrag = 0f;

                quantumParticles.Stop();
                uiAsstOff.color = uiActive;
                uiHover.color = uiInactive;
                uiAstro.color = uiInactive;
                uiQuantum.color = uiInactive;
                throttlePanel.alpha = 0f;

                Debug.Log("Flight Assist off");
                break;
            case FLIGHT_MODE.HOVER:
                speedCap = 10f;
                if (capRate != 1f) StartCoroutine(DropCapRate());
                currentEnginePower = enginePower * 0.5f;
                currentBoosterPower = boosterPower;

                shipGB.enabled = false;
                shipRB.drag = 1f;
                shipRB.angularDrag = 2f;

                quantumParticles.Stop();
                uiAsstOff.color = uiInactive;
                uiHover.color = uiActive;
                uiAstro.color = uiInactive;
                uiQuantum.color = uiInactive;
                throttlePanel.alpha = 0f;

                Debug.Log("Hover Flight engaged");
                break;
            case FLIGHT_MODE.ASTRO:
                speedCap = 50f;
                if (capRate != 1f) StartCoroutine(DropCapRate());
                currentEnginePower = enginePower;
                currentBoosterPower = boosterPower;

                shipGB.enabled = false;
                shipRB.drag = 0f;
                shipRB.angularDrag = 2f;

                quantumParticles.Stop();
                uiAsstOff.color = uiInactive;
                uiHover.color = uiInactive;
                uiAstro.color = uiActive;
                uiQuantum.color = uiInactive;
                if (throttleFade != null) {
                    StopCoroutine(throttleFade);
                }
                throttleFade = StartCoroutine(ShowThrottle());

                Debug.Log("Astro Flight engaged");
                break;
            case FLIGHT_MODE.QUANTUM:
                speedCap = quantumSpeed;
                capRate = 5f;
                currentEnginePower = 0f;
                currentBoosterPower = boosterPower * 0.33f;

                shipGB.enabled = false;
                shipRB.drag = 0f;
                shipRB.angularDrag = 2f;

                quantumParticles.Play();
                uiAsstOff.color = uiInactive;
                uiHover.color = uiInactive;
                uiAstro.color = uiInactive;
                uiQuantum.color = uiActive;
                throttlePanel.alpha = 0f;

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
    void SyncEngineLights(float thrustIntensity, float offset, Color colour) {
        foreach (Light engineLight in engineLights) {
            engineLight.intensity = Mathf.Lerp(engineLight.intensity, GetForce().z * thrustIntensity + offset, 0.5f * Time.deltaTime);
            engineLight.color = Color.Lerp(engineLight.color, colour, 0.5f * Time.deltaTime);
        }
    }

    // Engine aperture
    void SyncEngineAperture(float size) {
        currentEngineAperture = engineMesh.GetBlendShapeWeight(0);
        engineMesh.SetBlendShapeWeight(0, Mathf.Lerp(currentEngineAperture, size, 1f * Time.deltaTime));
    }

    IEnumerator ShowHealth() {
        healthPanel.alpha = 1f;
        yield return new WaitForSeconds(3f);

        while (healthPanel.alpha > 0f) {
            healthPanel.alpha -= 0.8f * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ShowThrottle() {
        throttlePanel.alpha = 1f;
        yield return new WaitForSeconds(3f);

        while (throttlePanel.alpha > 0f) {
            throttlePanel.alpha -= 0.8f * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ShowFlightMode() {
        flightModePanel.alpha = 1f;
        yield return new WaitForSeconds(3f);

        while (flightModePanel.alpha > 0f) {
            flightModePanel.alpha -= 0.8f * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator DropCapRate() {
        yield return new WaitForSeconds(1f);
        capRate = 1f;
    }
}
