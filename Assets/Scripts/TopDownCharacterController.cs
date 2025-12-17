using System;
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

    //The inputs that we need to retrieve from the input system.
    private InputAction m_moveAction;
    private InputAction m_attackAction;
    private InputAction m_sprintAction;
    private InputAction m_rollAction;

    //The components that we need to edit to make the player move smoothly.
    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    
    //The direction that the player is moving in.
    private Vector2 m_playerDirection;
    private Vector2 m_lastDirection;
   

    [Header("Movement parameters")]
    //The speed at which the player moves
    [SerializeField] private float m_playerSpeed = 200f;
    //The maximum speed the player can move
    [SerializeField] private float m_playerMaxSpeed = 1000f;
    [SerializeField] private float m_sprintSpeed = 400f;

    #endregion

    private float minStamina = 0f;
    private float maxStamina = 100f;
    private float stamina = 100f;
    private float staminaRegen = 20f;
    private float sprintCost = 30f;
    private bool canSprint = false;

    [Header("Projectile parameters")]
    [SerializeField] private GameObject m_projectilePrefab;
    [SerializeField] private Transform m_projectileSpawnPoint;
    [SerializeField] private float m_projectileSpeed;
    [SerializeField] private float m_fireRate;
    private float m_nextFireTime = 0f;

    Vector3 mousePos = Input.mousePosition;

    float mouseX = Input.GetAxis("Mouse X");
    float mouseY = Input.GetAxis("Mouse Y");

    /// <summary>
    /// When the script first initialises this gets called.
    /// Use this for grabbing components and setting up input bindings.
    /// </summary>
    private void Awake()
    {
        //bind movement inputs to variables
        m_moveAction = InputSystem.actions.FindAction("Move");
        m_attackAction = InputSystem.actions.FindAction("Attack");
        m_sprintAction = InputSystem.actions.FindAction("Sprint");
        m_rollAction = InputSystem.actions.FindAction("Roll");

        //get components from Character game object so that we can use them later.
        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
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
        //clamp the speed to the maximum speed for if the speed has been changed in code.
        float speed = m_playerSpeed > m_playerMaxSpeed ? m_playerMaxSpeed : m_playerSpeed;
        
        //apply the movement to the character using the clamped speed value.
        m_rigidbody.linearVelocity = m_playerDirection * (speed * Time.fixedDeltaTime);
    }
    
    /// <summary>
    /// When the update loop is called, it runs every frame.
    /// Therefore, this will run more or less frequently depending on performance.
    /// Used to catch changes in variables or input.
    /// </summary>
    void Update()
    {
        

        // store any movement inputs into m_playerDirection - this will be used in FixedUpdate to move the player.
        m_playerDirection = m_moveAction.ReadValue<Vector2>();
        
        // ~~ handle animator ~~
        // Update the animator speed to ensure that we revert to idle if the player doesn't move.
        m_animator.SetFloat("Speed", m_playerDirection.magnitude);
        
        // If there is movement, set the directional values to ensure the character is facing the way they are moving.
        if (m_playerDirection.magnitude > 0)
        {
            m_animator.SetFloat("Horizontal", m_playerDirection.x);
            m_animator.SetFloat("Vertical", m_playerDirection.y);
            canSprint = true;

            m_lastDirection = m_playerDirection;
        }
        else 
        {
            canSprint = false;
        }

        if (m_rollAction.WasPressedThisFrame())
        {
            m_animator.SetTrigger("Roll");

            Debug.Log("Roll!");
        }

        if (canSprint == true)
        {
            if (m_sprintAction.IsPressed())
            {
                
                stamina -= sprintCost * Time.deltaTime;
                stamina = Mathf.Clamp(stamina, minStamina, maxStamina);
                if (stamina <= 0f)
                {
                    canSprint = false;
                    Debug.Log("Out of stamina!");
                    m_playerSpeed = 200f;
                }
                else
                {
                    Debug.Log("Stamina: " + stamina);
                    m_playerSpeed = m_sprintSpeed;
                }
            }
        }
        else
        {
            if (stamina < 100f)
            {
                stamina += staminaRegen * Time.deltaTime;
                Debug.Log("Stamina: " + stamina);
            }
            m_playerSpeed = 200f;
        }
        // check if an attack has been triggered.
        if (m_attackAction.IsPressed() && Time.time > m_nextFireTime)
        {
            m_nextFireTime = Time.time + m_fireRate;
            Fire();
            Debug.Log("Attack!");
        }
    }

    private void Fire()
    {
        Vector2 MousePosition = Mouse.current.position.ReadValue();
        Vector3 mousePointOnScreen = Camera.main.ScreenToWorldPoint(MousePosition);

        Vector2 fireDirection = mousePointOnScreen;
        if (fireDirection == Vector2.zero)
        {
            fireDirection = Vector2.down; // Default direction if no movement
        }
        GameObject spawnedProjectile = Instantiate(m_projectilePrefab, m_projectileSpawnPoint.position, Quaternion.identity);

        Rigidbody2D projectileRB = spawnedProjectile.GetComponent<Rigidbody2D>();
        if (projectileRB != null)
        {
            projectileRB.AddForce(fireDirection.normalized * m_projectileSpeed, ForceMode2D.Impulse);
        }
    }
}
