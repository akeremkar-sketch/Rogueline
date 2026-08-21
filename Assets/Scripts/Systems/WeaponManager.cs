using UnityEngine;

public enum WeaponType { Melee, Bow, Flame, Scythe }

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [Header("Weapon Choice")]
    [SerializeField] private WeaponType startWeapon = WeaponType.Melee;

    [Header("Weapons")]
    [SerializeField] private MeleeWeapon  meleeWeapon;
    [SerializeField] private BowWeapon    bowWeapon;
    [SerializeField] private FlameWeapon  flameWeapon;
    [SerializeField] private ScytheWeapon scytheWeapon;

    public WeaponBase    ActiveWeapon     { get; private set; }
    public WeaponType    ActiveWeaponType { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        WeaponType weapon = CharacterManager.Instance != null
            ? CharacterManager.Instance.ActiveCharacter.weaponType
            : startWeapon;
        SelectWeapon(weapon);
    }

    public void SelectWeapon(WeaponType type)
    {
        SetEnabled(meleeWeapon,  false);
        SetEnabled(bowWeapon,    false);
        SetEnabled(flameWeapon,  false);
        SetEnabled(scytheWeapon, false);

        ActiveWeaponType = type;
        ActiveWeapon = type switch
        {
            WeaponType.Melee  => meleeWeapon,
            WeaponType.Bow    => bowWeapon,
            WeaponType.Flame  => flameWeapon,
            WeaponType.Scythe => scytheWeapon,
            _                 => meleeWeapon
        };

        SetEnabled(ActiveWeapon, true);
    }

    private void SetEnabled(WeaponBase w, bool state)
    {
        if (w != null) w.enabled = state;
    }
}
