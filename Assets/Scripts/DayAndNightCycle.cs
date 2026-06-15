using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

public class DayAndNightCycle : MonoBehaviour
{
    [Header("Cycle Settings")]
    [SerializeField] private float _cycleLenght = 300f;

    [Header("Player Vision")]
    [SerializeField] private Light2D _playerVisionLight;
    [SerializeField] private float _dayOuterRadius = 8f;
    [SerializeField] private float _nightOuterRadius = 5f;
    [SerializeField] private float _dayInnerRadius = 5f;
    [SerializeField] private float _nightInnerRadius = 1f;

    [Header("End Night UI")]
    [SerializeField] private Image _flashImage;
    [SerializeField] private TMP_Text _endText;
    [SerializeField] private float _flashDuration = 2f;

    [Header("Day Manager")]
    [SerializeField] private DayManager _dayManager;

    // Contrôle interne — indépendant de GameState
    private float _currentCycleTime = 0f;
    private bool _nightEnded = false;
    private bool _paused = true; // démarre en pause, ResumeTimer() le lance

    void Start()
    {
        ResetUI();
        // Le timer ne démarre pas au Start — c'est DayManager qui appelle ResumeTimer()
        Debug.Log("[DayNight] Prêt — en attente de ResumeTimer()");
    }

    void Update()
    {
        if (_paused || _nightEnded) return;

        if (_playerVisionLight != null)
        {
            float timePercent = Mathf.Clamp01(_currentCycleTime / _cycleLenght);
            _playerVisionLight.pointLightOuterRadius = Mathf.Lerp(_dayOuterRadius, _nightOuterRadius, timePercent);
            _playerVisionLight.pointLightInnerRadius = Mathf.Lerp(_dayInnerRadius, _nightInnerRadius, timePercent);
        }

        _currentCycleTime += Time.deltaTime;

        if (_currentCycleTime >= _cycleLenght)
        {
            _nightEnded = true;
            _paused = true;
            Debug.Log("[DayNight] Timer expiré !");
            StartCoroutine(EndNightSequence());
        }
    }

    // Appelé par DayManager quand le joueur entre dans la hutte
    public void ResetCycle()
    {
        _paused = true;
        _nightEnded = false;
        _currentCycleTime = 0f;
        ResetUI();
        Debug.Log("[DayNight] Cycle reset — timer à 0, en pause");
    }

    // Appelé par DayManager quand le joueur repart dans le monde
    public void ResumeTimer()
    {
        _paused = false;
        _nightEnded = false;
        _currentCycleTime = 0f; // double sécurité
        Debug.Log("[DayNight] Timer démarré pour le jour " + GameState.currentDay);
    }

    void ResetUI()
    {
        if (_flashImage != null) { Color c = _flashImage.color; c.a = 0f; _flashImage.color = c; }
        if (_endText   != null) { Color c = _endText.color;    c.a = 0f; _endText.color    = c; }
    }

    IEnumerator EndNightSequence()
    {
        if (_flashImage != null && _endText != null)
        {
            _endText.text = "La nuit tombe... Vous rentrez à la hutte.";
            float timer = 0f;
            Color ic = _flashImage.color; Color tc = _endText.color;
            while (timer < _flashDuration)
            {
                timer += Time.deltaTime;
                float a = Mathf.Clamp01(timer / _flashDuration);
                ic.a = a; _flashImage.color = ic;
                tc.a = a; _endText.color    = tc;
                yield return null;
            }
        }

        DayManager dm = _dayManager != null ? _dayManager : FindObjectOfType<DayManager>();
        if (dm != null) dm.OnDayEnded();
        else Debug.LogWarning("[DayNight] DayManager introuvable !");

        if (_flashImage != null && _endText != null)
        {
            float timer = 0f;
            Color ic = _flashImage.color; Color tc = _endText.color;
            while (timer < _flashDuration)
            {
                timer += Time.deltaTime;
                float a = Mathf.Clamp01(1f - timer / _flashDuration);
                ic.a = a; _flashImage.color = ic;
                tc.a = a; _endText.color    = tc;
                yield return null;
            }
        }

        ResetUI();
    }
}