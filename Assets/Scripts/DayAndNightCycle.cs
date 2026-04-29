using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

public class DayAndNightCycle : MonoBehaviour
{
    [Header("Cycle Settings")]
    [SerializeField] private float _cycleLenght = 24f; // In seconds

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

    [Header("Debug")]
    [SerializeField] private bool _debugLogs = true;

    private float _currentCycleTime;
    private bool _nightEnded;

    void Start()
    {
        // Reset UI
        if (_flashImage != null)
        {
            Color imageColor = _flashImage.color;
            imageColor.a = 0f;
            _flashImage.color = imageColor;
        }

        if (_endText != null)
        {
            Color textColor = _endText.color;
            textColor.a = 0f;
            _endText.color = textColor;
        }

        if (_debugLogs)
        {
            Debug.Log("[DayNight] Cycle started");
        }
    }

    void Update()
    {
        if (_playerVisionLight == null)
            return;

        // TIMER
        _currentCycleTime += Time.deltaTime;

        // NORMALIZED TIME (0 -> 1)
        float timePercent = Mathf.Clamp01(_currentCycleTime / _cycleLenght);

        // VISION EVOLUTION
        float currentOuterRadius = Mathf.Lerp(
            _dayOuterRadius,
            _nightOuterRadius,
            timePercent
        );

        float currentInnerRadius = Mathf.Lerp(
            _dayInnerRadius,
            _nightInnerRadius,
            timePercent
        );

        _playerVisionLight.pointLightOuterRadius = currentOuterRadius;
        _playerVisionLight.pointLightInnerRadius = currentInnerRadius;

        // DEBUG
        if (_debugLogs)
        {
            Debug.Log(
                $"[DayNight] Vision : " +
                $"Outer={currentOuterRadius:F2} | " +
                $"Inner={currentInnerRadius:F2}"
            );
        }

        // END OF NIGHT
        if (!_nightEnded && _currentCycleTime >= _cycleLenght)
        {
            _nightEnded = true;
            StartCoroutine(_EndNightFlash());
        }
    }

    private IEnumerator _EndNightFlash()
    {
        if (_flashImage == null || _endText == null)
            yield break;

        float timer = 0f;

        Color imageColor = _flashImage.color;
        Color textColor = _endText.color;

        _endText.text = "La nuit est terminée, vous rentrez vous coucher.";

        while (timer < _flashDuration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.PingPong(timer * 2f, 1f);

            imageColor.a = alpha;
            textColor.a = alpha;

            _flashImage.color = imageColor;
            _endText.color = textColor;

            yield return null;
        }

        // Final state
        imageColor.a = 1f;
        textColor.a = 1f;

        _flashImage.color = imageColor;
        _endText.color = textColor;

        if (_debugLogs)
        {
            Debug.Log("[DayNight] Night ended");
        }
    }
}