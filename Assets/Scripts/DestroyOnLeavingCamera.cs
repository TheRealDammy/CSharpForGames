using UnityEngine;

public class DestroyOnLeavingCamera : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 2.0f);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
