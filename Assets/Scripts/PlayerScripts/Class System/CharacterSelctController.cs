using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectController : MonoBehaviour
{
    public static CharacterSelectController Instance { get; private set; }

    public CharacterClass SelectedClass { get; private set; }

    public CharacterClassData selectedClassData;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectSwordsman() => Select(CharacterClass.Swordsman);
    public void SelectArcher() => Select(CharacterClass.Archer);
    public void SelectMage() => Select(CharacterClass.Mage);

    public void SelectRandom()
    {
        var classes = (CharacterClass[])System.Enum.GetValues(typeof(CharacterClass));
        int index = Random.Range(0, classes.Length);
        Select(classes[index]);
    }

    private void Select(CharacterClass chosen)
    {
        SelectedClass = chosen;
        SceneManager.LoadScene("Dungeon");
    }
}
