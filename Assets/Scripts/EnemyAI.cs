using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour {

    // Public
    public float totalHitPoints = 100f;
    public Canvas HUD;
    public Image healthBar;

    public float minTrackingDistance = 100f;
    public float maxTrackingDistance = 1000f;
    public float moveSpeed = 45f;

    public enum AI_STATE {
        IDLE,
        ATTACK,
        DEAD
    };

    public AI_STATE state = AI_STATE.IDLE;

    // Private
    private float hitPoints;
    private GravityBody enemyGB;
    private Transform player;
    private Vector3 enemyToPlayer;
    private float distanceToPlayer;
    private Rigidbody enemyRB;
    private Light[] lights;

    // Use this for initialization
    void Start() {
        hitPoints = totalHitPoints;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemyRB = GetComponent<Rigidbody>();
        enemyGB = GetComponent<GravityBody>();
        lights = GetComponentsInChildren<Light>();
    }

    // Update is called once per frame
    void Update() {
        enemyToPlayer = player.position - transform.position;
        distanceToPlayer = enemyToPlayer.magnitude;

        if (state != AI_STATE.DEAD) {
            if (distanceToPlayer <= maxTrackingDistance) {
                SetState(AI_STATE.ATTACK);
            } else {
                SetState(AI_STATE.IDLE);
            }
        }

        if (state == AI_STATE.ATTACK) {
            Attack();
        }

        HUD.transform.rotation = player.transform.rotation;
    }

    void Attack() {
        Quaternion targetRotation = Quaternion.LookRotation(enemyToPlayer);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.5f * Time.deltaTime);

        if (distanceToPlayer >= minTrackingDistance) {
            enemyRB.velocity = Vector3.Lerp(enemyRB.velocity, transform.forward * moveSpeed, 0.5f * Time.deltaTime);
        } else { // If the player is close, stop moving towards them
            enemyRB.velocity = Vector3.Lerp(enemyRB.velocity, Vector3.zero, 0.5f * Time.deltaTime);
        }
    }

    void Die() {
        // Turn on gravity
        HUD.enabled = false;
        GetComponent<AudioSource>().Stop();
        enemyGB.enabled = true;

        // Turn off all ship lights
        foreach (Light light in lights) {
            light.intensity = 0f;
        }

        float spinForce = Random.Range(-80f, 80f);

        enemyRB.AddRelativeTorque(new Vector3(0f, 0f, spinForce), ForceMode.Impulse);

        Destroy(gameObject, 10f);
    }

    void SetState(AI_STATE nextState) {
        switch (nextState) {
            case AI_STATE.IDLE:
                state = nextState;
                break;
            case AI_STATE.ATTACK:
                state = nextState;
                break;
            case AI_STATE.DEAD:
                Die();
                state = nextState;
                break;
        }
    }

    public void Damage(float damage) {
        if (hitPoints > 0f) {
            if (hitPoints - damage <= 0f) {
                hitPoints = 0f;
                SetState(AI_STATE.DEAD);
            } else {
                hitPoints -= damage;
            }
        }

        healthBar.fillAmount = hitPoints / totalHitPoints;
    }
}
