using UnityEngine;

public enum EnemyVariant { Variant1, Variant2, Variant3 }

[CreateAssetMenu(menuName = "Enemies/Enemy Type")]
public class EnemyTypeSO : ScriptableObject
{
    [Header("Identity")]
    public string enemyName;
    public Sprite defaultSprite;

    [Header("Base Stats")]
    public int baseHP = 10;
    public int baseDamage = 2;
    public float moveSpeed = 2f;

    [Header("Ranges")]
    public float aggroRange = 6f;
    public float attackRange = 0.8f;
    public float attackCooldown = 1.2f;

    [Header("Spawn")]
    [Min(0f)] public float spawnWeight = 1f;

    [Header("Pack / Group")]
    public bool prefersGroups = true;
    [Range(1, 8)] public int groupMin = 2;
    [Range(1, 12)] public int groupMax = 5;
    [Range(0f, 1f)] public float groupChance = 0.6f;


    [Header("Variants")]
    public EnemyVariantData[] variants = new EnemyVariantData[0];

}

[System.Serializable]
public class EnemyVariantData
{
    [Range(0.1f, 5f)] public float hpMultiplier = 1f;
    [Range(0.1f, 5f)] public float damageMultiplier = 1f;
    [Range(0.1f, 5f)] public float speedMultiplier = 1f;
    [Min(0f)] public float spawnWeight = 1f;

    public float scaleMultiplier = 1f;
    public Sprite spriteOverride;
    public AnimatorOverrideController animatorOverride;
}
