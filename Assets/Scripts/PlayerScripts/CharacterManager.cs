using UnityEngine;

/// <summary>
/// Applies the selected character: starting stats, animator controller, weapon and movement ability.
/// Ready in Awake so WeaponManager.Start() can read from it.
/// </summary>
public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [SerializeField] private CharacterData[] characters; // 0=Knight, 1=Archer, 2=Mage

    public CharacterData ActiveCharacter { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        int idx = PlayerPrefs.GetInt("SelectedCharacter", 0);
        ActiveCharacter = characters[Mathf.Clamp(idx, 0, characters.Length - 1)];
    }

    private void Start()
    {
        // Apply starting stat bonuses
        if (PlayerStats.Instance != null)
            foreach (var bonus in ActiveCharacter.startingBonuses)
                PlayerStats.Instance.ModifyStat(bonus.stat, bonus.points);

        // Re-initialize HP with correct MaxHealth (Start() order is not guaranteed)
        PlayerHealth.Instance?.ReinitializeHealth();

        // Animator
        var animator = PlayerController.Instance?.GetComponent<Animator>();
        if (animator != null && ActiveCharacter.animatorController != null)
            animator.runtimeAnimatorController = ActiveCharacter.animatorController;

        // Add character-specific movement ability
        AddAbility();
    }

    private void AddAbility()
    {
        if (PlayerController.Instance == null) return;
        GameObject player = PlayerController.Instance.gameObject;

        // Disable all abilities first, then enable only the active character's
        foreach (var a in player.GetComponents<MovementAbilityBase>())
            a.enabled = false;

        MovementAbilityBase ability = ActiveCharacter.type switch
        {
            CharacterType.Knight  => player.GetComponent<KnightAbility>(),
            CharacterType.Archer  => player.GetComponent<ArcherAbility>(),
            CharacterType.Mage    => player.GetComponent<MageAbility>(),
            CharacterType.Vampire => player.GetComponent<VampireAbility>(),
            _                     => null
        };

        if (ability != null)
            ability.enabled = true;
        else
            Debug.LogWarning($"[CharacterManager] No ability component found for {ActiveCharacter.type}. Add all three ability scripts to the Player GameObject.");
    }
}
