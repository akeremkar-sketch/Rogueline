using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Safe version of GraphicRaycaster.
///
/// Problem: If a destroyed TextMeshProUGUI (or other Graphic) stays in the m_RaycastableGraphics
/// list, GraphicRaycaster throws MissingReferenceException every frame and breaks all click detection.
///
/// Solution: Destroyed entries are removed from the list before base.Raycast() is called.
/// The try-catch approach loses all raycast results; this approach does not.
///
/// Setup: Auto-installed on scene load via [RuntimeInitializeOnLoadMethod].
/// No action required in the Editor.
/// </summary>
public class SafeGraphicRaycaster : GraphicRaycaster
{
    private Canvas _canvas;
    private readonly List<Graphic> _toRemove = new List<Graphic>();

    // Returns the Canvas this component is attached to (instead of accessing private base.canvas)
    private Canvas GetCanvas()
    {
        if (_canvas == null)
            _canvas = GetComponent<Canvas>();
        return _canvas;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        foreach (Canvas canvas in FindObjectsOfType<Canvas>(true))
        {
            // Skip if SafeGraphicRaycaster is already installed
            if (canvas.GetComponent<SafeGraphicRaycaster>() != null) continue;

            GraphicRaycaster existing = canvas.GetComponent<GraphicRaycaster>();

            if (existing != null)
            {
                bool ignoreReversed  = existing.ignoreReversedGraphics;
                var  blockingObjects = existing.blockingObjects;
                var  blockingMask    = existing.blockingMask;

                // Add safe version first — Canvas is never left without a raycaster
                SafeGraphicRaycaster safe = canvas.gameObject.AddComponent<SafeGraphicRaycaster>();
                safe.ignoreReversedGraphics = ignoreReversed;
                safe.blockingObjects        = blockingObjects;
                safe.blockingMask           = blockingMask;

                // Then remove the old one (end of frame — Destroy, not DestroyImmediate)
                Destroy(existing);
            }
            else
            {
                // No raycaster on this Canvas — add one
                canvas.gameObject.AddComponent<SafeGraphicRaycaster>();
            }
        }
    }

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        // Remove destroyed Graphics from the raycastable list.
        // Without this, base.Raycast() iterates over them and GetComponent() inside
        // Graphic.get_canvasRenderer() accesses a destroyed object → MissingReferenceException.
        // Catching the exception loses all raycast results; this approach does not.
        var list = GraphicRegistry.GetRaycastableGraphicsForCanvas(GetCanvas());
        if (list != null && list.Count > 0)
        {
            _toRemove.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                // Unity's == operator returns null for destroyed objects
                if (list[i] == null)
                    _toRemove.Add(list[i]);
            }
            // Each Remove call removes one destroyed entry from the list.
            // All _toRemove entries are destroyed (== null), so
            // N calls remove N destroyed entries.
            foreach (var g in _toRemove)
                list.Remove(g);
        }

        base.Raycast(eventData, resultAppendList);
    }
}
