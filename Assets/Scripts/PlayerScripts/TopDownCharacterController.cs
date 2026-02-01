using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownCharacterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed = 6f;
    [SerializeField] private float baseSprintMultiplier = 1.6f;
    [SerializeField] private float acceleration = 20f;

    [Header("Stamina")]
    [SerializeField] private float baseMaxStamina = 100f;
    [SerializeField] private float baseStaminaRegen = 20f;
    [SerializeField] private float sprintDrain = 25f;
    [SerializeField] private Image staminaBar;

    [Header("UI")]
    [SerializeField] private GameObject inventoryUI;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerStats stats;

    private Vector2 moveInput;
    private Vector2 facingDirection = Vector2.down;
    private Vector2 currentVelocity;

    private float maxStamina;
    private float staminaRegen;
    private float currentStamina;

    private float moveSpeed;
    private float sprintMultiplier;

    private bool isSprinting;
    private bool inventoryOpen;

    // =========================
    // UNITY
    // =========================
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        if (stats != null)
            stats.OnStatChanged += OnStatChanged;
    }

    private void OnDisable()
    {
        if (stats != null)
            stats.OnStatChanged -= OnStatChanged;
    }

    private void Start()
    {
        ApplyStats();
        currentStamina = maxStamina;
        UpdateStaminaUI();
    }

    private void Update()
    {
        HandleStamina();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = moveInput * GetCurrentSpeed();
        currentVelocity = Vector2.Lerp(
            currentVelocity,
            targetVelocity,
            acceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = currentVelocity;
    }

    // =========================
    // INPUT
    // =========================
    public void HandleMove(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();

        // Lock to cardinal directions
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            input.y = 0;
        else
            input.x = 0;

        moveInput = input;

        if (input != Vector2.zero)
            facingDirection = input.normalized;
    }

    public void HandleSprint(InputAction.CallbackContext ctx)
    {
        isSprinting = ctx.performed;
    }

    public void HandleOpenInventory(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        inventoryOpen = !inventoryOpen;
        inventoryUI.SetActive(inventoryOpen);
    }

    // =========================
    // STATS
    // =========================
    public void ApplyStats()
    {
        moveSpeed = baseMoveSpeed;
        sprintMultiplier = baseSprintMultiplier;

        maxStamina = baseMaxStamina + stats.GetStatLevel(PlayerStatType.Stamina) * 15f;
        staminaRegen = baseStaminaRegen + stats.GetStatLevel(PlayerStatType.Stamina) * 2f;

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        UpdateStaminaUI();
    }

    private void OnStatChanged(PlayerStatType type)
    {
        if (type != PlayerStatType.Stamina) return;

        maxStamina = baseMaxStamina + stats.GetStatLevel(PlayerStatType.Stamina) * 15f;
        staminaRegen = baseStaminaRegen + stats.GetStatLevel(PlayerStatType.Stamina) * 2f;

        currentStamina = Mathf.Min(currentStamina, maxStamina);
        UpdateStaminaUI();
    }

    // =========================
    // STAMINA
    // =========================
    private void HandleStamina()
    {
        bool draining = isSprinting && moveInput.sqrMagnitude > 0.1f && currentStamina > 0f;

        if (draining)
        {
            currentStamina -= sprintDrain * Time.deltaTime;
        }
        else
        {
            currentStamina += staminaRegen * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        UpdateStaminaUI();
    }

    public bool HasStamina(float amount)
    {
        return currentStamina >= amount;
    }

    public void ConsumeStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina - amount, 0, maxStamina);
        UpdateStaminaUI();
    }

    public float GetCurrentSpeed()
    {
        if (isSprinting && currentStamina > 0)
            return moveSpeed * sprintMultiplier;

        return moveSpeed;
    }

    public float GetMaxStamina()
    {
        return maxStamina;
    }

    // =========================
    // GETTERS (USED BY COMBAT)
    // =========================
    public Vector2 GetFacingDirection()
    {
        return facingDirection;
    }

    // =========================
    // UI / ANIMATION
    // =========================
    private void UpdateStaminaUI()
    {
        if (staminaBar)
            staminaBar.fillAmount = currentStamina / maxStamina;
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("Speed", moveInput.magnitude);
        animator.SetFloat("Horizontal", facingDirection.x);
        animator.SetFloat("Vertical", facingDirection.y);
    }

    public void ResetSpeed()
    {
        moveSpeed = baseMoveSpeed;
        sprintMultiplier = baseSprintMultiplier;
    }
    public void ModifySpeed(float multiplier)
    {
        moveSpeed = baseMoveSpeed * multiplier;
    }
}
