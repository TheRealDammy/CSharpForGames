using UnityEngine;

public class RunStats : MonoBehaviour
{
    public static RunStats Instance;

    public int enemiesKilled;
    public float runTime;
    public int damageDealt;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        runTime += Time.deltaTime;
    }

    public void RegisterKill() => enemiesKilled++;
    public void RegisterDamage(int amount) => damageDealt += amount;
}
