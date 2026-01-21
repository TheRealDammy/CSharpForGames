using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/// <summary>
/// A class to control the top-down character.
/// Implements the player controls for moving and shooting.
/// Updates the player animator so the character animates based on input.
/// </summary>
public class TopDownCharacterController : MonoBehaviour
{
    #region Framework Variables

    //The components that we need to edit to make the player move smoothly.
    private Animator animator;
    private Rigidbody2D m_rigidbody;
    
    //The direction that the player is moving in.
    public Vector2 playerDirection;
    private Vector2 lastDirection;
   

    [Header("Movement parameters")]
    //The speed at which the player moves
    [SerializeField] private float playerSpeed = 200f;
    //The maximum speed the player can move
    [SerializeField] private float playerMaxSpeed = 1000f;
    [SerializeField] private float sprintSpeed = 400f; 
    public bool isSprinting;

    #endregion

    [Header("Projectile parameters")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;

    [Header("Attack")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float range = 1.2f;
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private float cooldown = 0.35f;
    [SerializeField] private float attackLockTime = 0.45f;


    [Header("Targeting")]
    [SerializeField] private LayerMask hittableLayers; // Props + Enemies layers
    [SerializeField] private Transform attackOrigin;    // empty child transform in front of player

    [Header("Knockback (optional)")]
    [SerializeField] private float knockbackForce = 0f;

    private float lastAttackTime = -999f;
    private bool isAttacking;

    private Stamina staminaComponent;

    /// <summary>
    /// When the script first initialises this gets called.
    /// Use this for grabbing components and setting up input bindings.
    /// </summary>
    private void Awake()
    {

        //get components from Character game object so that we can use them later.
        animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        staminaComponent = GetComponent<Stamina>();
    }

    /// <summary>
    /// Called after Awake(), and is used to initialize variables e.g. set values on the player
    /// </summary>
    void Start()
    {
        //not currently used - left here for demonstration purposes.
    }

    /// <summary>
    /// When a fixed update loop is called, it runs at a constant rate, regardless of pc performance.
    /// This ensures that physics are calculated properly.
    /// </summary>
    private void FixedUpdate()
    {
        if (isAttacking)
        {
            m_rigidbody.linearVelocity = Vector2.zero;
            return;
        }

        //clamp the speed to the maximum speed for if the speed has been changed in code.
        float speed = playerSpeed > playerMaxSpeed ? playerMaxSpeed : playerSpeed;
        
        //apply the movement to the character using the clamped speed value.
        m_rigidbody.linearVelocity = playerDirection * (speed * Time.fixedDeltaTime);
    }
    
    /// <summary>
    /// When the update loop is called, it runs every frame.
    /// Therefore, this will run more or less frequently depending on performance.
    /// Used to catch changes in variables or input.
    /// </summary>
    void Update()
    {
        staminaComponent.UpdateStamina();
    }

    public void HandleMove(InputAction.CallbackContext ctx)
    {
        if (isAttacking)
        {
            playerDirection = Vector2.zero;
            return;
        }

        if (ctx.performed)
        {
            Vector2 input = ctx.ReadValue<Vector2>();

            // Clamp diagonals: allow only one axis
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                input.y = 0;
            else
                input.x = 0;

            playerDirection = input;

            // ~~ handle animator ~~
            // Update the animator speed to ensure that we revert to idle if the player doesn't move.
            animator.SetFloat("Speed", playerDirection.magnitude);

            // If there is movement, set the directional values to ensure the character is facing the way they are moving.
            if (playerDirection.magnitude > 0)
            {
                animator.SetFloat("Horizontal", playerDirection.x);
                animator.SetFloat("Vertical", playerDirection.y);

                lastDirection = playerDirection;
            }          
        }
        else if (ctx.canceled)
        {
            playerDirection = Vector2.zero;

            // ~~ handle animator ~~
            // Update the animator speed to ensure that we revert to idle if the player doesn't move.
            animator.SetFloat("Speed", playerDirection.magnitude);

            // If there is no movement, keep the last directional values to ensure the character is still facing the way they were moving.
            animator.SetFloat("Horizontal", lastDirection.x);
            animator.SetFloat("Vertical", lastDirection.y);   
        }
    }

    public void HandleSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (staminaComponent != null)
            {
                playerSpeed = sprintSpeed;
                isSprinting = true;                           
            }
        }
        else
        {
            if (staminaComponent != null)
            {
                playerSpeed = 200f;
                isSprinting = false;     
            }
        }   
    }

    public void HandleAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            TryAttack();
        }
    }

    public void HandleInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Interact();
        }
    }

    private void Fire()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mousePointOnScreen = Camera.main.ScreenToWorldPoint(mousePosition);

        Vector2 fireDirection = mousePointOnScreen;
        if (fireDirection == Vector2.zero)
        {
            fireDirection = Vector2.down; // Default direction if no movement
        }
        GameObject spawnedProjectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        Rigidbody2D projectileRB = spawnedProjectile.GetComponent<Rigidbody2D>();
        if (projectileRB != null)
        {
            projectileRB.AddForce(fireDirection.normalized * projectileSpeed, ForceMode2D.Impulse);
        }
    }

    public void Interact()
    {
        // Interaction logic can be implemented here
        Debug.Log("Interact method called.");
    }

    public void TryAttack()
    {
        if (Time.time < lastAttackTime + cooldown) return;
        lastAttackTime = Time.time;

        animator.SetTrigger("Attack");

        Vector2 origin = attackOrigin != null ? (Vector2)attackOrigin.position : (Vector2)transform.position;
        Vector2 center = origin + playerDirection.normalized * range;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, hittableLayers);

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];

            // damage
            var dmg = col.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                Vector2 hitPoint = col.ClosestPoint(center);
                dmg.TakeDamage(damage, hitPoint, playerDirection.normalized);
            }

            // optional knockback if it has a rigidbody
            if (knockbackForce > 0f)
            {
                var rb = col.attachedRigidbody;
                if (rb != null)
                    rb.AddForce(playerDirection.normalized * knockbackForce, ForceMode2D.Impulse);
            }
        }
        StartAttackLock(attackLockTime);
    }
    public void StartAttackLock(float duration)
    {
        if (attackLockRoutine != null)
            StopCoroutine(attackLockRoutine);

        attackLockRoutine = StartCoroutine(AttackLockRoutine(duration));
    }

    private Coroutine attackLockRoutine;

    private IEnumerator AttackLockRoutine(float duration)
    {
        isAttacking = true;
        playerDirection = Vector2.zero;
        m_rigidbody.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(duration);

        isAttacking = false;
    }

}
