using UnityEngine;

public enum CharacterType { Knight, Archer, Mage, Vampire }

[System.Serializable]
public struct StatBonus
{
    public StatType stat;
    public float    points;
}

[CreateAssetMenu(fileName = "CharacterData", menuName = "Rogueline/Character Data")]
public class CharacterData : ScriptableObject
{
    public CharacterType              type;
    public string                     displayName;
    [TextArea] public string          description;
    public Sprite                     avatar;
    public WeaponType                 weaponType;
    public RuntimeAnimatorController  animatorController;
    public StatBonus[]                startingBonuses;
}
