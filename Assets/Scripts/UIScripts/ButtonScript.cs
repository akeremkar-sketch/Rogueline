using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float pressOffset = 4f;
    [SerializeField] private float hoverScale  = 1.05f;

    [Header("Sounds")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip errorSound;

    [Header("Locked Mode")]
    [Tooltip("When clicked, plays error sound instead of click sound.")]
    [SerializeField] private bool lockedMode = false;

    [Tooltip("If disabled, OnPointerDown won't play click sound — PlayClick/PlayError called externally (e.g. shop button).")]
    [SerializeField] private bool autoPlayClick = true;

    private RectTransform _rt;
    private Vector2       _originalPos;
    private AudioSource   _audioSrc;
    private Coroutine     _soundCoroutine;

    private void Awake()
    {
        _rt          = GetComponent<RectTransform>();
        _originalPos = _rt.anchoredPosition;

        _audioSrc              = gameObject.AddComponent<AudioSource>();
        _audioSrc.playOnAwake  = false;
        _audioSrc.spatialBlend = 0f;
        _audioSrc.volume       = 1f;
    }

    // ── Public API ───────────────────────────────────────────────────────────────

    public AudioClip ClickSoundClip => clickSound;
    public AudioClip ErrorSoundClip => errorSound;

    public void PlayClick() => PlayClipped(clickSound);
    public void PlayError() => PlayClipped(errorSound);

    /// Disable autoPlayClick for buttons controlled externally, like shop buttons.
    public void SetAutoPlayClick(bool auto) { autoPlayClick = auto; }

    // ── Internal ─────────────────────────────────────────────────────────────────

    private void PlayClipped(AudioClip clip)
    {
        if (clip == null) return;
        if (_soundCoroutine != null) StopCoroutine(_soundCoroutine);
        _soundCoroutine = StartCoroutine(PlayClippedRoutine(clip, 0.25f));
    }

    private IEnumerator PlayClippedRoutine(AudioClip clip, float duration)
    {
        const float fadeOut = 0.08f;
        float startVol = AudioManager.Instance != null ? AudioManager.Instance.EffectiveUI : 1f;

        _audioSrc.volume = startVol;
        _audioSrc.clip   = clip;
        _audioSrc.Play();

        float playTime = Mathf.Max(0f, duration - fadeOut);
        if (playTime > 0f)
            yield return new WaitForSecondsRealtime(playTime);

        // Smooth fade-out
        float elapsed = 0f;
        while (elapsed < fadeOut)
        {
            elapsed         += Time.unscaledDeltaTime;
            _audioSrc.volume = Mathf.Lerp(startVol, 0f, elapsed / fadeOut);
            yield return null;
        }

        _audioSrc.Stop();
        _audioSrc.volume = 1f;
        _soundCoroutine  = null;
    }

    // ── Pointer events ───────────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData e)
    {
        transform.localScale = Vector3.one * hoverScale;
        PlayClipped(hoverSound);
    }

    public void OnPointerExit(PointerEventData e)
    {
        transform.localScale = Vector3.one;
        _rt.anchoredPosition = _originalPos;
    }

    public void OnPointerDown(PointerEventData e)
    {
        transform.localScale = Vector3.one;
        _rt.anchoredPosition = _originalPos + Vector2.down * pressOffset;

        if (lockedMode)
            PlayClipped(errorSound);
        else if (autoPlayClick)
            PlayClipped(clickSound);
        // autoPlayClick=false: no sound, PlayClick/PlayError will be called externally
    }

    public void OnPointerUp(PointerEventData e)
    {
        _rt.anchoredPosition = _originalPos;
        transform.localScale = Vector3.one * hoverScale;
    }
}
