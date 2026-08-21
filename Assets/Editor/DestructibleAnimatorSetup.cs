using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Unity Editor tool that generates AnimationClips and AnimatorController
/// for destructible world objects from a sliced sprite sheet.
///
/// Open via: Tools > Destructible Animator Setup
///
/// Workflow:
///   1. Assign the sprite sheet (must be set to Multiple sprite mode + sliced).
///   2. Set the frame index ranges for Idle and Break animations.
///   3. Set FPS and output folder.
///   4. Click Generate — creates {Name}_Idle.anim, {Name}_Break.anim, {Name}.controller.
///   5. Drag the .controller onto your prefab's Animator component.
///   6. Set destroyDelay on DestructibleObject >= (breakFrameCount / fps).
/// </summary>
public class DestructibleAnimatorSetup : EditorWindow
{
    private Texture2D _spriteSheet;
    private string    _objectName  = "Barrel";
    private int       _idleStart   = 0;
    private int       _idleEnd     = 2;
    private int       _breakStart  = 3;
    private int       _breakEnd    = 6;
    private float     _fps         = 8f;
    private string    _outputPath  = "Assets/Animations/Destructibles";

    private List<Sprite> _cachedSprites;
    private string       _cachedSheetPath;

    [MenuItem("Tools/Destructible Animator Setup")]
    public static void ShowWindow() =>
        GetWindow<DestructibleAnimatorSetup>("Destructible Animator");

    private void OnGUI()
    {
        GUILayout.Label("Destructible Animator Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        EditorGUI.BeginChangeCheck();
        _spriteSheet = (Texture2D)EditorGUILayout.ObjectField(
            "Sprite Sheet", _spriteSheet, typeof(Texture2D), false);
        if (EditorGUI.EndChangeCheck()) _cachedSprites = null;

        _objectName = EditorGUILayout.TextField("Object Name", _objectName);
        _fps        = EditorGUILayout.FloatField("FPS", _fps);
        _outputPath = EditorGUILayout.TextField("Output Path", _outputPath);

        EditorGUILayout.Space(6);

        // Show sprite info
        if (_spriteSheet != null)
        {
            List<Sprite> sprites = GetSprites();
            EditorGUILayout.HelpBox($"Sprite sheet has {sprites.Count} slices (index 0 – {sprites.Count - 1})", MessageType.Info);
        }

        EditorGUILayout.Space(4);
        GUILayout.Label("Idle Animation Frames", EditorStyles.boldLabel);
        _idleStart = EditorGUILayout.IntField("  Start Index", _idleStart);
        _idleEnd   = EditorGUILayout.IntField("  End Index",   _idleEnd);

        EditorGUILayout.Space(4);
        GUILayout.Label("Break Animation Frames", EditorStyles.boldLabel);
        _breakStart = EditorGUILayout.IntField("  Start Index", _breakStart);
        _breakEnd   = EditorGUILayout.IntField("  End Index",   _breakEnd);

        // Show suggested destroy delay
        if (_fps > 0f)
        {
            int breakFrames = _breakEnd - _breakStart + 1;
            float suggestedDelay = breakFrames / _fps + 0.05f;
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                $"Suggested destroyDelay on DestructibleObject: {suggestedDelay:F2}s  " +
                $"({breakFrames} frames @ {_fps} fps)",
                MessageType.None);
        }

        EditorGUILayout.Space(8);

        if (_spriteSheet == null)
        {
            EditorGUILayout.HelpBox("Assign a sprite sheet to continue.", MessageType.Warning);
            return;
        }

        GUI.enabled = !string.IsNullOrEmpty(_objectName);
        if (GUILayout.Button("Generate Animator", GUILayout.Height(30)))
            Generate();
        GUI.enabled = true;
    }

    private void Generate()
    {
        List<Sprite> sprites = GetSprites();

        if (sprites.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No sprites found. Is the texture set to Sprite Mode: Multiple and sliced?", "OK");
            return;
        }

        int maxIndex = sprites.Count - 1;
        if (_idleStart  < 0 || _idleEnd  > maxIndex ||
            _breakStart < 0 || _breakEnd > maxIndex ||
            _idleStart  > _idleEnd || _breakStart > _breakEnd)
        {
            EditorUtility.DisplayDialog("Error",
                $"Frame indices out of range. Sheet has {sprites.Count} sprites (0 – {maxIndex}).", "OK");
            return;
        }

        // Ensure output folder exists
        if (!AssetDatabase.IsValidFolder(_outputPath))
        {
            Directory.CreateDirectory(_outputPath);
            AssetDatabase.Refresh();
        }

        // Create clips
        AnimationClip idleClip  = BuildClip(sprites, _idleStart,  _idleEnd,  _fps, loop: true);
        AnimationClip breakClip = BuildClip(sprites, _breakStart, _breakEnd, _fps, loop: false);

        string idlePath  = $"{_outputPath}/{_objectName}_Idle.anim";
        string breakPath = $"{_outputPath}/{_objectName}_Break.anim";
        string ctrlPath  = $"{_outputPath}/{_objectName}.controller";

        SaveOrReplace(idleClip,  idlePath);
        SaveOrReplace(breakClip, breakPath);

        // Reload clips from disk so the controller references them correctly
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        idleClip  = AssetDatabase.LoadAssetAtPath<AnimationClip>(idlePath);
        breakClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(breakPath);

        // Build AnimatorController
        AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
        if (ctrl != null) AssetDatabase.DeleteAsset(ctrlPath);

        ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;

        ctrl.AddParameter("Break", AnimatorControllerParameterType.Trigger);

        AnimatorState idleState  = sm.AddState("Idle");
        idleState.motion         = idleClip;
        sm.defaultState          = idleState;

        AnimatorState breakState = sm.AddState("Break");
        breakState.motion        = breakClip;

        // Idle → Break on trigger (instant, no exit time)
        AnimatorStateTransition t = idleState.AddTransition(breakState);
        t.AddCondition(AnimatorConditionMode.If, 0f, "Break");
        t.hasExitTime = false;
        t.duration    = 0f;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[DestructibleAnimator] Generated: {ctrlPath}");
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = ctrl;
    }

    // --- Helpers ---

    private List<Sprite> GetSprites()
    {
        string path = AssetDatabase.GetAssetPath(_spriteSheet);
        if (_cachedSprites != null && _cachedSheetPath == path)
            return _cachedSprites;

        _cachedSheetPath = path;
        _cachedSprites   = new List<Sprite>();

        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            if (asset is Sprite s) _cachedSprites.Add(s);

        // Numeric sort: SpriteName_0, SpriteName_1, ..., SpriteName_10, ...
        _cachedSprites.Sort((a, b) =>
        {
            int nA = ExtractTrailingNumber(a.name);
            int nB = ExtractTrailingNumber(b.name);
            return nA.CompareTo(nB);
        });

        return _cachedSprites;
    }

    private static int ExtractTrailingNumber(string name)
    {
        int i = name.LastIndexOf('_');
        if (i >= 0 && int.TryParse(name.Substring(i + 1), out int n)) return n;
        return 0;
    }

    private static AnimationClip BuildClip(List<Sprite> sprites, int start, int end, float fps, bool loop)
    {
        AnimationClip clip = new AnimationClip { frameRate = fps };

        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");

        int frameCount = end - start + 1;
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time  = i / fps,
                value = sprites[start + i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        return clip;
    }

    private static void SaveOrReplace(AnimationClip clip, string path)
    {
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(path) != null)
            AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
    }
}
