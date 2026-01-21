using UnityEngine;

public class InteractableProps : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Interacted with prop: " + gameObject.name);
        // Add interaction logic here (e.g., open a door, pick up an item, etc.)
    }
}
