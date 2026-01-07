using UnityEngine;

[CreateAssetMenu(fileName = "RandomDungeonData_", menuName = "PCGDungeon/RandomDungeonData", order = 1)]
public class SimpleRandomDungeonData : ScriptableObject
{
    public int iterations = 10;
    public int walkLength = 10;
    public bool startRandomly = true;
}
