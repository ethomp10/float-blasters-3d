using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour {

    public float minDistanceToPlayer = 100f;
    public float enemySpeed = 45f;

    private Transform player;
    private Vector3 enemyToPlayer;
    private Rigidbody enemyRB;
    public enum AI_STATE { IDLE, ATTACK };
    public AI_STATE stage = AI_STATE.IDLE;

    // Use this for initialization
    void Start () {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemyRB = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        Attack();
    }

    void Attack () {
        enemyToPlayer = player.position - transform.position;

        Quaternion targetRotation = Quaternion.LookRotation(enemyToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);

        if (enemyToPlayer.magnitude >= minDistanceToPlayer) {
            enemyRB.velocity = Vector3.Lerp(enemyRB.velocity, enemyToPlayer.normalized * enemySpeed, 0.5f * Time.deltaTime);
        } else {
            enemyRB.velocity = Vector3.Lerp(enemyRB.velocity, Vector3.zero, 0.5f * Time.deltaTime);
        }
    }
}
