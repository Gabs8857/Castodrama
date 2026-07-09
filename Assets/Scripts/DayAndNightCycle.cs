using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Ink.Runtime;

public class DayAndNightCycle : MonoBehaviour
{
    [Header("⏳ Cycle Settings")]
    [SerializeField] private float _cycleLength = 300f; // Durée du cycle jour/nuit

    [Header("👁️ Player Vision")]
    [SerializeField] private Light2D _playerVisionLight;
    [SerializeField] private float _dayOuterRadius = 8f;
    [SerializeField] private float _nightOuterRadius = 5f;
    [SerializeField] private float _dayInnerRadius = 5f;
    [SerializeField] private float _nightInnerRadius = 1f;

    [Header("⏰ Dialogue avant fin (10%)")]
    [SerializeField] private TextAsset _startDayInk; // Ton fichier Ink avec le Knot "Start"
    [SerializeField] private string _startDayKnot = "Start";
    [SerializeField] [Range(0f, 1f)] private float _dialogueTriggerPercent = 0.1f; // 10% avant la fin

    [Header("🌃 Fin de nuit")]
    [SerializeField] private Image _flashImage;
    [SerializeField] private TMP_Text _endText;
    [SerializeField] private float _flashDuration = 2f;

    [Header("🔄 Changement de scène")]
    [SerializeField] private string _nextSceneName = "HutteScene";

    private float _currentCycleTime = 0f;
    private bool _nightEnded = false;
    private bool _paused = true;
    private bool _dialogueLaunched = false; // Pour éviter de lancer plusieurs fois

    // =========================================================================
    void Start()
    {
        ResetUI();
        Debug.Log("[DayNight] Prêt — en attente de ResumeTimer()");
    }

    // =========================================================================
    void Update()
    {
        if (_paused || _nightEnded) return;

        // Mise à jour de la lumière du joueur
        if (_playerVisionLight != null)
        {
            float timePercent = Mathf.Clamp01(_currentCycleTime / _cycleLength);
            _playerVisionLight.pointLightOuterRadius = Mathf.Lerp(_dayOuterRadius, _nightOuterRadius, timePercent);
            _playerVisionLight.pointLightInnerRadius = Mathf.Lerp(_dayInnerRadius, _nightInnerRadius, timePercent);
        }

        _currentCycleTime += Time.deltaTime;

        // ✅ NOUVEAU : Lancer le dialogue 10% avant la fin
        float timeRemainingPercent = 1f - (_currentCycleTime / _cycleLength);
        if (timeRemainingPercent <= _dialogueTriggerPercent && !_dialogueLaunched)
        {
            _dialogueLaunched = true;
            Debug.Log($"[DayNight] Lancement du dialogue (il reste {timeRemainingPercent * 100f:F0}% du cycle)");

            if (_startDayInk != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(_startDayInk, _startDayKnot);
            }
        }

        // Fin du cycle
        if (_currentCycleTime >= _cycleLength)
        {
            _nightEnded = true;
            _paused = true;
            _dialogueLaunched = false; // Réinitialiser pour le prochain jour
            Debug.Log($"[DayNight] Fin du jour {GameState.currentDay} !");
            GameState.currentDay++;
            StartCoroutine(EndNightSequence());
        }
    }

    // =========================================================================
    public void ResetCycle()
    {
        _paused = true;
        _nightEnded = false;
        _currentCycleTime = 0f;
        _dialogueLaunched = false; // Réinitialiser
        ResetUI();
        Debug.Log("[DayNight] Cycle réinitialisé");
    }

    // =========================================================================
    public void ResumeTimer()
    {
        _paused = false;
        _nightEnded = false;
        _dialogueLaunched = false; // Réinitialiser
        Debug.Log($"[DayNight] Timer démarré pour le jour {GameState.currentDay}");
    }

    // =========================================================================
    void ResetUI()
    {
        if (_flashImage != null) { Color c = _flashImage.color; c.a = 0f; _flashImage.color = c; }
        if (_endText != null) { Color c = _endText.color; c.a = 0f; _endText.color = c; }
    }

    // =========================================================================
    IEnumerator EndNightSequence()
    {
        if (_flashImage != null && _endText != null)
        {
            _endText.text = "La nuit tombe... Rentrez à la hutte !";
            float timer = 0f;
            Color ic = _flashImage.color;
            Color tc = _endText.color;

            while (timer < _flashDuration)
            {
                timer += Time.deltaTime;
                float a = Mathf.Clamp01(timer / _flashDuration);
                ic.a = a;
                tc.a = a;
                _flashImage.color = ic;
                _endText.color = tc;
                yield return null;
            }
        }

        yield return new WaitForSeconds(1f);

        if (_flashImage != null && _endText != null)
        {
            float timer = 0f;
            Color ic = _flashImage.color;
            Color tc = _endText.color;

            while (timer < _flashDuration)
            {
                timer += Time.deltaTime;
                float a = Mathf.Clamp01(1f - timer / _flashDuration);
                ic.a = a;
                tc.a = a;
                _flashImage.color = ic;
                _endText.color = tc;
                yield return null;
            }
        }

        if (!string.IsNullOrEmpty(_nextSceneName))
        {
            SceneManager.LoadScene(_nextSceneName);
        }

        ResetUI();
    }
}