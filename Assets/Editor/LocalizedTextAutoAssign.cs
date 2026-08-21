#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LocalizedTextAutoAssign
{
    private static readonly Dictionary<string, string> _textToKey = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
    {
        { "Take",      "take" },
        { "No Space",  "no_space" },
        { "Press [E]", "press_e" },
        { "READY",             "status_ready" },
        { "NO BLOOD",          "status_no_blood" },
        { "DODGE!",            "status_dodge" },
        { "SLOWED!",           "status_slowed" },
        { "Charge Rush",       "ability_charge_rush" },
        { "Dash",              "ability_dash" },
        { "Blink",             "ability_blink" },
        { "Bat Form",          "ability_bat_form" },
        { "Move Speed",        "stat_move_speed" },
        { "Max Health",        "stat_max_health" },
        { "Damage",            "stat_damage" },
        { "Attack Speed",      "stat_attack_speed" },
        { "Attack Count",      "stat_attack_count" },
        { "Pickup Radius",     "stat_pickup_radius" },
        { "XP Gain",           "stat_xp_gain" },
        { "Coin Gain",         "stat_coin_gain" },
        { "Defense",           "stat_defense" },
        { "Luck",              "stat_luck" },
        { "Life Steal",        "stat_life_steal" },
        { "Life Regen",        "stat_life_regen" },
        { "Dodge",             "stat_dodge" },
        { "Knockback",         "stat_knockback" },
        { "Crit Chance",       "stat_crit_chance" },
        { "Difficulty",        "stat_difficulty" },
        { "Max",               "hud_xp_max" },
        { "XP",                "hud_xp_label" },
        { "YOU DIED",          "death_title" },
        { "Restart",   "restart" },
        { "Main Menu", "main_menu" },
        { "Resume",    "resume" },
        { "Continue",  "continue" },
        { "Quit",      "quit" },
        { "Settings",  "settings" },
        { "Play",      "play" },
        { "Back",      "back" },
        { "Sell",      "sell" },
        { "Buy",       "buy" },
        { "Boss I",            "boss_1" },
        { "Boss II",           "boss_2" },
        { "Boss III",          "boss_3" },
        { "PAUSED",            "pause_title" },
    };

    [MenuItem("Tools/Localization/Auto-Assign LocalizedText (Current Scene)")]
    public static void AutoAssignInScene()
    {
        int added = 0, skipped = 0;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;
            foreach (GameObject root in scene.GetRootGameObjects())
                ProcessHierarchy(root.transform, ref added, ref skipped);
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("LocalizedText Auto-Assign", $"Added: {added}\nSkipped: {skipped}", "OK");
    }

    private static void ProcessHierarchy(Transform t, ref int added, ref int skipped)
    {
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            string text = tmp.text?.Trim();
            if (!string.IsNullOrEmpty(text) && _textToKey.TryGetValue(text, out string key))
            {
                if (t.GetComponent<LocalizedText>() != null) { skipped++; }
                else
                {
                    Undo.RecordObject(t.gameObject, "Add LocalizedText");
                    var lt = Undo.AddComponent<LocalizedText>(t.gameObject);
                    var so = new SerializedObject(lt);
                    so.FindProperty("key").stringValue = key;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(t.gameObject);
                    added++;
                }
            }
            else skipped++;
        }

        foreach (Transform child in t)
            ProcessHierarchy(child, ref added, ref skipped);
    }
}
#endif
