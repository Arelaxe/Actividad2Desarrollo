using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
    Main character logic
*/
public class PlayerController : MonoBehaviour
{
    // Input Actions
    private PlayerInput playerInput;
    private InputAction walkAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction dashAction;

    // Components
    private Collider2D ownCollider;
    private Rigidbody2D rb;
    private Animator animator;

    // Values
    private float direction = 1.0f;
    private bool grounded;
    private bool isTakingDamage = false;

    // Audio Sources
    private AudioSource mainAudioSource;
    private AudioSource walkSoundController;
    
    // Serialized components
    [SerializeField] private Transform attackPoint;

    // Layers
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private LayerMask healingLayer;
    
    // Serialized values
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float speed = 10;
    [SerializeField] private int health = 3;
    [SerializeField] private float dashSpeed = 50;
    [SerializeField] private float jumpForce = 200;
    [SerializeField] private float attackValue = 2.0f;
    [SerializeField] private float attackDelay = 1.0f;
    [SerializeField] private float inmunityTime = 2.0f;
    
    // Audio clips
    [SerializeField] private AudioClip swordSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip gameOverSound;

    /*
        Unity functions
    */

    // Start is called before the first frame update
    void Start() {
        // Get components
        playerInput = GetComponent<PlayerInput>();
        ownCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        mainAudioSource = Camera.main.GetComponent<AudioSource>();
        walkSoundController = GetComponent<AudioSource>();

        // Get Input Actions
        walkAction = playerInput.actions["Andar"];
        jumpAction = playerInput.actions["Saltar"];
        attackAction = playerInput.actions["Atacar"];
        dashAction = playerInput.actions["Dash"];
    }

    // Update is called once per frame
    void Update() {
        Walk();

        if (grounded && jumpAction.triggered){
            Jump();
        }

        if (attackAction.triggered){
            Attack();
        }

        if (dashAction.triggered){
            Dash();
        }
    }

    // Frame-rate independent function for physics calculations
    private void FixedUpdate() {
        WalkPhysics();
        DashPhysics();
        CheckIsGrounded();
    }

    /*
        Actions
    */

    // Walk action
    private void Walk(){
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")){ // We don't want the character to move while attacking
            float horizontal = walkAction.ReadValue<Vector2>().x;

            // We don't make any sounds if the character it's not moving or if it's not executing the proper animation
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Run") || horizontal == 0.0f){
                walkSoundController.enabled = false;
            }
            else if (horizontal < 0.0f){ // We check if the character is moving left
                direction = -1.0f;
                walkSoundController.enabled = true;
            }
            else if (horizontal > 0.0f){ // We check if the character is moving right
                direction = 1.0f;
                walkSoundController.enabled = true;
            }

            // We rotate the character depending on the direction it is moving
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
            
            // We update the animation bool
            if (horizontal == 0.0f){
                animator.SetBool("isWalking", false);
            }
            else{
                animator.SetBool("isWalking", true);
            }
        }
        
    }

    // Jump action
    private void Jump(){
        // Update animations and sounds
        animator.SetTrigger("jump");
        mainAudioSource.PlayOneShot(jumpSound);

        // Add a force to jump
        rb.AddForce(Vector2.up * jumpForce);
    }

    // Attack action
    private void Attack(){
        // Update animations and sounds
        animator.SetTrigger("attack");
        mainAudioSource.PlayOneShot(swordSound);
        walkSoundController.enabled = false;

        // Start attack coroutine
        StartCoroutine(ExecuteAttack());
    }

    // Dash action
    private void Dash(){
        animator.SetTrigger("dash");
        mainAudioSource.PlayOneShot(dashSound);
    }

    /*
        Passive actions
    */

    // This function is called when they hit us
    public void Hit(){
        if (!isTakingDamage){
            isTakingDamage = true;
            
            // Reduce player's health
            health--;

            // Update animations and sounds
            animator.SetTrigger("hurt");
            mainAudioSource.PlayOneShot(hurtSound);

            // If our health reaches 0, we die and the game is over
            if (health <= 0){
                animator.SetTrigger("dead");
                mainAudioSource.PlayOneShot(gameOverSound);
            }

            StartCoroutine(Inmunity());
        }
    }

    /*
        Physics functions
    */

    // Physics for the walk action
    private void WalkPhysics() {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")){ 
            rb.velocity = new Vector2(walkAction.ReadValue<Vector2>().x * speed, rb.velocity.y); 
        }
        else{ // We stop the motion if we are attacking
            rb.velocity = new Vector2(0f, 0f);
        }
    }

    // Physics for the dash action
    private void DashPhysics() {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dash")){
            rb.velocity = new Vector2(direction * dashSpeed, rb.velocity.y); 
        }
    }

    // Checks whether we are grounded or not
    private void CheckIsGrounded() {
        Debug.DrawRay(transform.position, Vector3.down*1.5f, Color.green);
        grounded = Physics2D.Raycast(transform.position, Vector3.down, 1.5f);
    }

    /*
        Coroutines
    */

    // Coroutine for attack action
    IEnumerator ExecuteAttack() {
        // We need to add a certain delay because of the attack animation
        yield return new WaitForSeconds(attackDelay);

        // Update sounds
        walkSoundController.enabled = false;
        mainAudioSource.PlayOneShot(swordSound);

        // Get the enemies reached by player's attack
        Collider2D [] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        
        foreach(Collider2D enemy in hitEnemies){
            if (enemy.gameObject.GetComponent<NPC>() != null){
                StartCoroutine(enemy.gameObject.GetComponent<NPC>().TakeHit(gameObject, attackValue));
            }
        }
    }

    IEnumerator Inmunity() {
        yield return new WaitForSeconds(inmunityTime);
        isTakingDamage = false;
    }

    /*
        Debug functions
    */

    // Draws attack range sphere
    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
