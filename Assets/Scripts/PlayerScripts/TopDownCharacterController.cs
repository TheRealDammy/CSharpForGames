using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownCharacterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed = 200f;
    [SerializeField] private float baseSprintSpeed = 400f;
    [SerializeField] private float maxSpeed = 1000f;

    [Header("Stamina")]
    [SerializeField] private float baseMaxStamina = 100f;
    [SerializeField] private float baseStaminaRegen = 20f;
    [SerializeField] private float sprintCost = 30f;
    [SerializeField] private Image staminaBar;

    [Header("Attack")]
    [SerializeField] private int baseDamage = 1;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private float attackCooldown = 0.35f;
    [SerializeField] private float attackLockTime = 0.45f;
    [SerializeField] private LayerMask hittableLayers;
    [SerializeField] private Transform attackOrigin;

    [Header("UI")]
    [SerializeField] private GameObject inventoryUI;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerStats stats;

    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.down;

    private bool isSprinting;
    private bool isAttacking;
    private bool inventoryOpen;

    private float lastAttackTime;
    private Coroutine attackLockRoutine;

    private float currentStamina;
    private float maxStamina;
    private float staminaRegen;
    private float moveSpeed;
    private float sprintSpeed;
    private int damage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        ApplyStats();
        currentStamina = maxStamina;
        UpdateStaminaUI();
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


    // =========================
    // STAT APPLICATION
    // =========================
    public void ApplyStats()
    {
        moveSpeed = baseMoveSpeed;
        sprintSpeed = baseSprintSpeed;

        maxStamina = baseMaxStamina + stats.GetStatLevel(PlayerStatType.Stamina) * 15f;
        staminaRegen = baseStaminaRegen + stats.GetStatLevel(PlayerStatType.Stamina) * 2f;

        damage = baseDamage + stats.GetStatLevel(PlayerStatType.Strength) * 2;

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        UpdateStaminaUI();
    }

    private void OnStatChanged(PlayerStatType type)
    {
        if (type == PlayerStatType.Stamina)
        {
            maxStamina = 100f + stats.GetStatLevel(PlayerStatType.Stamina) * 15f;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }

        if (type == PlayerStatType.Strength)
        {
            damage = 1 + stats.GetStatLevel(PlayerStatType.Strength);
        }
    }


    // =========================
    // MOVEMENT
    // =========================
    private void FixedUpdate()
    {
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float speed = isSprinting && currentStamina > 0 ? sprintSpeed : moveSpeed;
        speed = Mathf.Min(speed, maxSpeed);

        rb.linearVelocity = moveInput * speed * Time.fixedDeltaTime;
    }

    public void HandleMove(InputAction.CallbackContext ctx)
    {
        if (isAttacking) return;

        if (ctx.performed)
        {
            moveInput = ctx.ReadValue<Vector2>();

            // Lock to cardinal directions
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
                moveInput.y = 0;
            else
                moveInput.x = 0;

            if (moveInput != Vector2.zero)
                lastDirection = moveInput.normalized;

            UpdateAnimator();
        }
        else if (ctx.canceled)
        {
            moveInput = Vector2.zero;
            UpdateAnimator();
        }
    }

    // =========================
    // SPRINT / STAMINA
    // =========================
    public void HandleSprint(InputAction.CallbackContext ctx)
    {
        isSprinting = ctx.performed;
    }

    private void Update()
    {
        HandleStamina();
    }

    private void HandleStamina()
    {
        if (isSprinting && moveInput.magnitude > 0 && currentStamina > 0)
        {
            currentStamina -= sprintCost * Time.deltaTime;
        }
        else
        {
            currentStamina += staminaRegen * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {
        if (staminaBar)
            staminaBar.fillAmount = currentStamina / maxStamina;
    }

    // =========================
    // ATTACK
    // =========================
    public void HandleAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            TryAttack();
    }

    private void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");

        Vector2 origin = attackOrigin ? (Vector2)attackOrigin.position : (Vector2)transform.position;
        Vector2 center = origin + lastDirection * attackRange;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRadius, hittableLayers);

        foreach (var hit in hits)
        {
            IDamageable dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                Vector2 hitPoint = hit.ClosestPoint(center);
                dmg.TakeDamage(damage, hitPoint, lastDirection);
            }
        }

        StartAttackLock();
    }

    private void StartAttackLock()
    {
        if (attackLockRoutine != null)
            StopCoroutine(attackLockRoutine);

        attackLockRoutine = StartCoroutine(AttackLock());
    }

    private IEnumerator AttackLock()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(attackLockTime);
        isAttacking = false;
    }

    // =========================
    // INVENTORY
    // =========================
    public void HandleOpenInventory(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (inventoryOpen)
        {
            inventoryUI.SetActive(false);
            inventoryOpen = false;
        }
        else
        {
            inventoryUI.SetActive(true);
            inventoryOpen = true;
        }
    }

    // =========================
    // ANIMATION
    // =========================
    private void UpdateAnimator()
    {
        animator.SetFloat("Speed", moveInput.magnitude);
        animator.SetFloat("Horizontal", lastDirection.x);
        animator.SetFloat("Vertical", lastDirection.y);
    }
}
