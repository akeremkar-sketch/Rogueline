using System.Collections;
using UnityEngine;

public class BloodFXAutoDestroy : MonoBehaviour
{
    [SerializeField] private float playbackSpeed = 2f;

    private IEnumerator Start()
    {
        var anim = GetComponent<Animator>();
        if (anim != null) anim.speed = playbackSpeed;

        yield return null; // one frame for animator to initialize

        if (anim != null)
        {
            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
        }
        Destroy(gameObject);
    }
}
