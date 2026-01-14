using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    [SerializeField] private Image staminaBar;


    private float minStamina = 0f;
    [SerializeField] private float maxStamina = 100f;
    private float currentStamina;
    [SerializeField] private float staminaRegen = 20f;
    [SerializeField] private float sprintCost = 30f;
    public bool canSprint = false;

    private TopDownCharacterController playerController;
    void Start()
    {
        currentStamina = maxStamina;
        playerController = GetComponent<TopDownCharacterController>();
    }

    public bool UseStamina()
    {
        if (currentStamina > minStamina)
        {
            currentStamina -= sprintCost * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, minStamina, maxStamina);
            canSprint = true;
            Debug.Log("Stamina: " + currentStamina);         
        }
        else if (currentStamina <= minStamina)
        {
            canSprint = false;
        }
        return canSprint;
    }

    public void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegen * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, minStamina, maxStamina);
            Debug.Log("Stamina: " + currentStamina);
        }
    }

    public void UpdateStamina()
    {
        if (canSprint)
        {
            UseStamina();
        }
        else         
        {
            RegenerateStamina();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.playerDirection.magnitude > 0 && playerController.sprintAction.IsPressed())
        {
            canSprint = true;
        }
        else
        {
            canSprint = false;
        }

        staminaBar.fillAmount = currentStamina / maxStamina;
    }
}
