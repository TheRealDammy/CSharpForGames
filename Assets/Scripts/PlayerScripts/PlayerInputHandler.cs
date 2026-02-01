using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool AttackPressed { get; private set; }
    private float attackBufferTimer;

    public void HandleMove(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();
    }

    public void HandleAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        // Cast to CombatController to access the protected method via a derived class
        CombatController combatController = GetComponentInChildren<CombatController>();
        if (combatController != null)
        {
            combatController.TryAttack();
        }
    }

    private void Update()
    {
        if (attackBufferTimer > 0)
            attackBufferTimer -= Time.deltaTime;
        else
            AttackPressed = false;
    }
}
