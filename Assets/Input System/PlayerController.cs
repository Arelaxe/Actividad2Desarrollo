using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction walkAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction dashAction;
    private Collider2D ownCollider;
    private AudioSource audioSource;
    // PROVISIONAL
    private InputAction hurtAction;
    // FIN PROVISIONAL
    private Rigidbody2D rb;
    private Animator animator;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private LayerMask healingLayer;
    private float direction = 1.0f;
    [SerializeField] private float attackRange = 0.5f;
    private bool grounded;
    private int health = 3;

    [SerializeField] private float speed = 10;
    [SerializeField] private float dashSpeed = 50;
    [SerializeField] private float jumpForce = 200;
    [SerializeField] private float attackDelay = 1.0f;
    [SerializeField] private AudioClip swordSound;
    [SerializeField] private AudioClip jumpSound;
    private AudioSource playerSoundController;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip gameOverSound;

    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        ownCollider = GetComponent<Collider2D>();
        audioSource = Camera.main.GetComponent<AudioSource>();
        playerSoundController = GetComponent<AudioSource>();
        walkAction = playerInput.actions["Andar"];
        jumpAction = playerInput.actions["Saltar"];
        attackAction = playerInput.actions["Atacar"];
        dashAction = playerInput.actions["Dash"];
        // PROVISIONAL
        hurtAction = playerInput.actions["Hurt (provisional)"];
        // FIN PROVISIONAL
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Walk();
        Jump();
        Attack();
        Dash();
        if (hurtAction.triggered){
            Hit();
        }
    }

    private void FixedUpdate() {
        WalkPhysics();
        CheckIsGrounded();
        DashPhysics();
    }

    private void WalkPhysics() {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")){
            rb.velocity = new Vector2(walkAction.ReadValue<Vector2>().x * speed, rb.velocity.y); 
        }
        else{
            rb.velocity = new Vector2(0f, 0f);
        }
    }

    private void DashPhysics() {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dash")){
            rb.velocity = new Vector2(direction * dashSpeed, rb.velocity.y); 
        }
    }

    private void CheckIsGrounded() {
        grounded = Physics2D.Raycast(transform.position, Vector3.down, 1.5f);
    }

    private void Walk(){
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")){
            float horizontal = walkAction.ReadValue<Vector2>().x;

            if (horizontal < 0.0f){
                direction = -1.0f;
                playerSoundController.enabled = true;
            }
            else if (horizontal > 0.0f){
                direction = 1.0f;
                playerSoundController.enabled = true;
            }
            else{
                playerSoundController.enabled = false;
            }

            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
            
            if (horizontal == 0.0f){
                animator.SetBool("isWalking", false);
            }
            else{
                animator.SetBool("isWalking", true);
            }

        }
        
    }

    private void Jump(){
        if (grounded && jumpAction.triggered){
            animator.SetTrigger("jump");
            audioSource.PlayOneShot(jumpSound);
            rb.AddForce(Vector2.up * jumpForce);
        }
    }

    private void Attack(){
        if (attackAction.triggered){
            animator.SetTrigger("attack");
            StartCoroutine(ExecuteAttack());
            
            audioSource.PlayOneShot(swordSound);
        }
    }

    IEnumerator ExecuteAttack()
    {
        yield return new WaitForSeconds(attackDelay);

        Collider2D [] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
       
       foreach(Collider2D enemy in hitEnemies){
            Destroy(enemy.gameObject);
            Debug.Log("Hit");
        }
    }

    private void Hit(){
        health--;
        animator.SetTrigger("hurt");
        audioSource.PlayOneShot(hurtSound);

        if (health <= 0){
            animator.SetTrigger("dead");
            audioSource.PlayOneShot(gameOverSound);
        }
    }

    private void Dash(){
        if (dashAction.triggered){
            animator.SetTrigger("dash");
            audioSource.PlayOneShot(dashSound);
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
