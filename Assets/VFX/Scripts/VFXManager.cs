using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    [SerializeField] private GameObject explosionPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Multiple instances of VFXManager detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Creates an explosion effect at the specified position.
    /// </summary>
    /// <param name="position">The position where the explosion should occur.</param>
    /// <param name="destroyAfter">The number of seconds to pass before destroying the explosion</param>
    /// <returns>A refrence to the explosion gameObject that has spawned</returns>
    
    public static GameObject CreateExplosion(Vector2 position, float destroyAfter = 2f)
    {
        if (Instance == null || Instance.explosionPrefab == null)
        {
            Debug.LogError("VFXManager instance or explosionPrefab is not set.");
            return null;
        }

        GameObject explosion = Instantiate(Instance.explosionPrefab, position, Quaternion.identity);
        Destroy(explosion, destroyAfter);
        return explosion;
    }
}
