using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodFXManager : MonoBehaviour
{
    public static BloodFXManager Instance { get; private set; }

    [SerializeField] private GameObject bloodFXPrefab;
    [SerializeField] private int        poolSize = 10;

    private readonly Queue<GameObject> _pool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        if (bloodFXPrefab == null) { Debug.LogError("[BloodFXManager] bloodFXPrefab atanmamış!"); return; }

        for (int i = 0; i < poolSize; i++)
            _pool.Enqueue(CreateObject());
    }

    public void Spawn(Vector2 position, RuntimeAnimatorController controller, string sortingLayerName, int sortingOrder)
    {
        if (controller == null || bloodFXPrefab == null) return;

        GameObject go = _pool.Count > 0 ? _pool.Dequeue() : CreateObject();
        go.transform.position = position;
        go.SetActive(true);

        var sr   = go.GetComponent<SpriteRenderer>();
        var anim = go.GetComponent<Animator>();

        if (sr != null)
        {
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder     = sortingOrder + 1;
        }

        if (anim != null)
        {
            anim.runtimeAnimatorController = controller;
            anim.Play("Play", 0, 0f);
        }

        StartCoroutine(ReturnWhenDone(go, anim));
    }

    private IEnumerator ReturnWhenDone(GameObject go, Animator anim)
    {
        yield return null; // one frame for animator to initialize

        if (anim != null)
        {
            while (anim.isActiveAndEnabled &&
                   anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
        }

        if (go != null)
        {
            go.SetActive(false);
            _pool.Enqueue(go);
        }
    }

    private GameObject CreateObject()
    {
        GameObject go = Instantiate(bloodFXPrefab);
        go.name = "BloodFX";
        go.transform.SetParent(transform);
        go.SetActive(false);
        return go;
    }
}
