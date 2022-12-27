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
    // PROVISIONAL
    private InputAction hurtAction;
    // FIN PROVISIONAL
    private Rigidbody2D rb;
    private Animator animator;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;
    private float direction = 1.0f;
    [SerializeField] private float attackRange = 0.5f;
    private bool grounded;
    private int health = 3;

    [SerializeField] float speed = 10;
    [SerializeField] float dashSpeed = 50;
    [SerializeField] float jumpForce = 200;

    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
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
            }
            else if (horizontal > 0.0f){
                direction = 1.0f;
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
            rb.AddForce(Vector2.up * jumpForce);
        }
    }

    private void Attack(){
        if (attackAction.triggered){
            animator.SetTrigger("attack");
            Collider2D [] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        
            foreach(Collider2D enemy in hitEnemies){
                Debug.Log("Hit");
            }
        }
    }

    private void Hit(){
        health--;
        animator.SetTrigger("hurt");

        if (health <= 0){
            animator.SetTrigger("dead");
        }
    }

    private void Dash(){
        if (dashAction.triggered){
            animator.SetTrigger("dash");
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
