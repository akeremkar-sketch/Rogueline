using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class SliderHandleAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale  = 1.15f;
    [SerializeField] private float pressScale  = 1.35f;
    [SerializeField] private float animSpeed   = 12f;

    [Header("Sounds")]
    [SerializeField] private AudioClip grabSound;
    [SerializeField] private AudioClip releaseSound;

    private bool        _pressed;
    private bool        _hovering;
    private float       _targetScale = 1f;
    private AudioSource _audio;
    private Coroutine   _soundCoroutine;

    private void Awake()
    {
        _audio              = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake  = false;
        _audio.spatialBlend = 0f;
    }

    private void Update()
    {
        float current = transform.localScale.x;
        float next    = Mathf.Lerp(current, _targetScale, Time.unscaledDeltaTime * animSpeed);
        transform.localScale = Vector3.one * next;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        _hovering = true;
        if (!_pressed) _targetScale = hoverScale;
    }

    public void OnPointerExit(PointerEventData e)
    {
        _hovering = false;
        if (!_pressed) _targetScale = 1f;
    }

    public void OnPointerDown(PointerEventData e)
    {
        _pressed     = true;
        _targetScale = pressScale;
        PlayClipped(grabSound);
    }

    public void OnPointerUp(PointerEventData e)
    {
        _pressed     = false;
        _targetScale = _hovering ? hoverScale : 1f;
        PlayClipped(releaseSound);
    }

    private void PlayClipped(AudioClip clip)
    {
        if (clip == null) return;
        if (_soundCoroutine != null) StopCoroutine(_soundCoroutine);
        _soundCoroutine = StartCoroutine(PlayRoutine(clip));
    }

    private IEnumerator PlayRoutine(AudioClip clip)
    {
        const float duration = 0.2f;
        const float fadeOut  = 0.06f;

        float vol = AudioManager.Instance != null ? AudioManager.Instance.EffectiveUI : 1f;
        _audio.volume = vol;
        _audio.clip   = clip;
        _audio.Play();

        float playTime = Mathf.Max(0f, duration - fadeOut);
        if (playTime > 0f)
            yield return new WaitForSecondsRealtime(playTime);

        float elapsed = 0f;
        while (elapsed < fadeOut)
        {
            elapsed       += Time.unscaledDeltaTime;
            _audio.volume  = Mathf.Lerp(vol, 0f, elapsed / fadeOut);
            yield return null;
        }

        _audio.Stop();
        _audio.volume   = 1f;
        _soundCoroutine = null;
    }
}
