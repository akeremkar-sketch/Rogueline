using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Linq;

public static class BloodFXAnimationCreator
{
    private const string SourceRoot = "Assets/Sprites/Bloodfx";
    private const string OutputRoot = "Assets/Animations/BloodFX";
    private const float  Fps        = 12f;

    [MenuItem("Tools/Create BloodFX Prefabs")]
    public static void CreatePrefabs()
    {
        const string prefabRoot = "Assets/Prefabs/BloodFX";
        if (!AssetDatabase.IsValidFolder(prefabRoot))
            AssetDatabase.CreateFolder("Assets/Prefabs", "BloodFX");

        int created = 0;
        for (int i = 1; i <= 9; i++)
        {
            string controllerPath = $"{OutputRoot}/BloodFX_{i}.controller";
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (controller == null)
            {
                Debug.LogWarning($"[BloodFXAnimationCreator] Controller bulunamadı: {controllerPath} — önce 'Create BloodFX Animations' çalıştır.");
                continue;
            }

            string prefabPath = $"{prefabRoot}/BloodFX_{i}.prefab";

            var go   = new GameObject($"BloodFX_{i}");
            var sr   = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder     = 0;
            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
            go.AddComponent<BloodFXAutoDestroy>();

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[BloodFXAnimationCreator] {created}/9 BloodFX prefab oluşturuldu: {prefabRoot}");
    }

    [MenuItem("Tools/Create BloodFX Animations")]
    public static void CreateAll()
    {
        if (!AssetDatabase.IsValidFolder(OutputRoot))
            AssetDatabase.CreateFolder("Assets/Animations", "BloodFX");

        int created = 0;
        for (int i = 1; i <= 9; i++)
        {
            AnimationClip clip = CreateClip(i);
            if (clip != null)
            {
                CreateController(clip, i);
                created++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[BloodFXAnimationCreator] Tamamlandı — {created}/9 clip + controller oluşturuldu: {OutputRoot}");
    }

    private static AnimationClip CreateClip(int index)
    {
        string folderPath = $"{SourceRoot}/{index}";

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogWarning($"[BloodFXAnimationCreator] Klasör bulunamadı: {folderPath}");
            return null;
        }

        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
        if (guids.Length == 0)
        {
            Debug.LogWarning($"[BloodFXAnimationCreator] {folderPath} içinde sprite yok — önce import et.");
            return null;
        }

        Sprite[] sprites = guids
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .OrderBy(p => Path.GetFileNameWithoutExtension(p))
            .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p))
            .Where(s => s != null)
            .ToArray();

        if (sprites.Length == 0) return null;

        AnimationClip clip = new AnimationClip();
        clip.frameRate = Fps;

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type         = typeof(SpriteRenderer),
            path         = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
        for (int k = 0; k < sprites.Length; k++)
            keyframes[k] = new ObjectReferenceKeyframe { time = k / Fps, value = sprites[k] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string clipPath = $"{OutputRoot}/BloodFX_{index}.anim";
        AssetDatabase.CreateAsset(clip, clipPath);
        Debug.Log($"[BloodFXAnimationCreator] {clipPath} ({sprites.Length} frame @ {Fps}fps)");
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
    }

    private static void CreateController(AnimationClip clip, int index)
    {
        string path = $"{OutputRoot}/BloodFX_{index}.controller";
        AssetDatabase.DeleteAsset(path);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        var sm         = controller.layers[0].stateMachine;

        var state            = sm.AddState("Play");
        state.motion         = clip;
        state.writeDefaultValues = false;
        sm.defaultState      = state;

        Debug.Log($"[BloodFXAnimationCreator] {path} controller oluşturuldu");
    }
}
