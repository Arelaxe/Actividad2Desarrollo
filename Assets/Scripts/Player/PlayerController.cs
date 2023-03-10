using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    private InputAction pauseAction;

    // Components
    private Collider2D ownCollider;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer ownRenderer;

    // Values
    private float direction = 1.0f;
    private bool grounded;
    private bool isTakingDamage = false;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool canPressDash = true;
    private bool paused = false;
    private bool isDashing = false;
    
    // Audio Sources
    private AudioSource mainAudioSource;
    private AudioSource walkSoundController;
    
    // Serialized components
    [SerializeField] private Transform attackPoint;

    // Layers
    [SerializeField] private LayerMask enemyLayers;
    
    // Serialized values
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float speed = 10;
    [SerializeField] private float health;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float dashSpeed = 50;
    [SerializeField] private float jumpForce = 200;
    [SerializeField] private float attackValue = 2.0f;
    [SerializeField] private float attackDelay = 1.0f;
    [SerializeField] private float inmunityTime = 2.0f;
    [SerializeField] private float dashCoolDownTime = 2.0f;
    
    // HUD Objects
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private GameObject menuPause;

    // Audio clips
    [SerializeField] private AudioClip swordSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip gameOverSound;

    // For collisions
    private bool isCollided;
    private bool lastCollisionEnded = true;
    private GameObject lastObjectCollided;

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
        ownRenderer = GetComponent<SpriteRenderer>();
        mainAudioSource = Camera.main.GetComponent<AudioSource>();
        walkSoundController = GetComponent<AudioSource>();

        // Get Input Actions
        walkAction = playerInput.actions["Andar"];
        jumpAction = playerInput.actions["Saltar"];
        attackAction = playerInput.actions["Atacar"];
        dashAction = playerInput.actions["Dash"];
        pauseAction = playerInput.actions["Pause"];

        // Initialization of HUD values
        health = maxHealth;
        healthBar.InitializeHealthBar(health);
    }

    // Update is called once per frame
    void Update() {
        if (!isDead && !paused){
            Walk();

            if (grounded && jumpAction.triggered){
                Jump();
            }

            if (!isAttacking && attackAction.triggered){
                Attack();
            }

            if (dashAction.triggered && canPressDash){
                canPressDash = false;
                Dash();
            }
        }

        if(pauseAction.triggered && !isDead){
            Pause();
        }
    }

    // Frame-rate independent function for physics calculations
    private void FixedUpdate() {
        if (!isDead && !paused){
            WalkPhysics();
            DashPhysics();
            CheckIsGrounded();
        }
        else if (isDead){
            if (grounded){ // We destroy de RigidBody 2D when de body reaches the ground
                Destroy(rb);
            }
        }
    }

    // This function is called when the player enters a collisions
    private void OnCollisionEnter2D (Collision2D other) {
        if (lastCollisionEnded){
            isCollided = true;
            lastObjectCollided = other.gameObject;
            lastCollisionEnded = false;
        }
    }

    // This function is called when the player exits a collision
    void OnCollisionExit2D(Collision2D other) {
        if (lastObjectCollided == other.gameObject){
            isCollided = false;
            lastCollisionEnded = true;
        }
    }

    /*
        Getters and Setters
    */

    // Changes paused bool
    public void SetPaused(bool newPaused){
        paused = newPaused;
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
        if (!isDashing){
            // Update animations and sounds
            animator.SetTrigger("jump");
            mainAudioSource.PlayOneShot(jumpSound);

            // Add a force to jump
            rb.AddForce(Vector2.up * jumpForce);
        }
    }

    // Attack action
    private void Attack(){
        isAttacking = true;

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

    // Pause action
    private void Pause(){
        if(!paused){
            paused = true;
            Time.timeScale = 0f;
            menuPause.SetActive(true);
        }
        else{
            paused = false;
            Time.timeScale = 1.0f;
            menuPause.SetActive(false);
        }
    }

    /*
        Passive actions
    */

    // This function is called when they hit us
    public void Hit(){
        if (!isTakingDamage){
            // We set this trigger to apply inmunity after taking a hit
            isTakingDamage = true;
            
            // Reduce player's health
            health-= 20;

            // Update HUD values
            healthBar.ChangeActualHealth(health);

            // Update animations and sounds
            animator.SetTrigger("hurt");
            mainAudioSource.PlayOneShot(hurtSound);

            // If our health reaches 0, we die and the game is over
            if (health <= 0){
                isDead = true;
                animator.SetTrigger("dead");
                mainAudioSource.PlayOneShot(gameOverSound);
                //wait for a bit and call the GameOver scene
                Invoke("GameEnd", 2);
            }
            else{
                // We set inmunity for a time and we make the character blink
                StartCoroutine(Inmunity());
                StartCoroutine(DoBlinks(4, inmunityTime/12));
            }
        }
    }

    /*
        Physics functions
    */

    // Physics for the walk action
    private void WalkPhysics() {
        // This bool fixes a bug that happened when colliding with walls
        bool canMove = !isCollided || grounded;

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && canMove){
           rb.velocity = new Vector2(walkAction.ReadValue<Vector2>().x * speed, rb.velocity.y); 
        }
        else{ // We stop the motion if we are attacking
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    // Physics for the dash action
    private void DashPhysics() {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dash")){
            isDashing = true;
            rb.velocity = new Vector2(direction * dashSpeed, rb.velocity.y); 
            StartCoroutine(DashCoolDown());
        }
    }

    // Checks whether we are grounded or not
    private void CheckIsGrounded() {
        // We need three rays to avoid bugs with corners
        grounded = Physics2D.Raycast(transform.position, Vector3.down, 0.9f) || 
                   Physics2D.Raycast(new Vector2(transform.position.x + 0.25f, transform.position.y), Vector3.down, 0.9f) ||
                   Physics2D.Raycast(new Vector2(transform.position.x - 0.25f, transform.position.y), Vector3.down, 0.9f);

        // We draw those rays for debugging
        Debug.DrawRay(transform.position, Vector3.down * 0.9f, Color.green);
        Debug.DrawRay(new Vector2(transform.position.x + 0.25f, transform.position.y), Vector3.down * 0.9f, Color.green);
        Debug.DrawRay(new Vector2(transform.position.x - 0.25f, transform.position.y), Vector3.down * 0.9f, Color.green);
    }

    // Ends the game when we lose
    private void GameEnd() {
        SceneManager.LoadScene("GameOver");
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

        isAttacking = false;
    }

    // Coroutine for inmunity. We have a little inmunity time after taking a hit
    IEnumerator Inmunity() {
        yield return new WaitForSeconds(inmunityTime);
        isTakingDamage = false;
    }

    // Coroutine to make our character blink, it is used to indicate inmunity
    IEnumerator DoBlinks(int numBlinks, float seconds) {
        for (int i=0; i<numBlinks*3; i++) {
        
            //toggle renderer
            ownRenderer.enabled = !ownRenderer.enabled;
            
            //wait for a bit
            yield return new WaitForSeconds(seconds);
        }
        
        //make sure renderer is enabled when we exit
        ownRenderer.enabled = true;
    }

    // Coroutine for dash cool down
    IEnumerator DashCoolDown() {
        yield return new WaitForSeconds(dashCoolDownTime);
        canPressDash = true;
        isDashing = false;
    }

    /*
        Debug functions
    */

    // Draws attack range sphere
    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
