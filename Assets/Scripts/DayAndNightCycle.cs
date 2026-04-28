using System.Collections;
using System.Collections.Generic;   
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayAndNightCycle : MonoBehaviour
{
    [System.Serializable]
    public class DayAndNighMark
    {
        public float timeRatio;
        public Color color;
        public float intensity;
    }

    [SerializeField] private DayAndNighMark[] _marks;
    [SerializeField] private float _cycleLenght = 300; // in seconds
    [SerializeField] private Light2D _light;

    // PLAYER VISION
    [Header("Player Vision")]
    [SerializeField] private Light2D _playerVisionLight;

    [SerializeField] private float _dayOuterRadius = 8f;
    [SerializeField] private float _nightOuterRadius = 5f;

    [SerializeField] private float _dayInnerRadius = 5f;
    [SerializeField] private float _nightInnerRadius = 1f;

    // DEBUG
    [SerializeField] private bool _debugLogs = true;

    private const float _TIME_CHECK_EPSILON = 0.1f;
    
    private float _currentCycleTime;
    private int _currentMarkIndex, _nextMarkIndex;
    private float _currentMarkTime, _nextMarkTime;


    void Start()
    {
        _currentMarkIndex = -1;
        _CycleMarks();

        if (_debugLogs)
        {
            Debug.Log("[DayNight] Cycle started");
        }
    }

    void Update()
    {
        // Safety check - prevent NullReferenceException
        if (_marks == null || _marks.Length == 0 || _light == null)
        {
            if (_debugLogs)
            {
                Debug.LogWarning("[DayNight] Missing references! Assign _marks and _light in inspector.");
            }
            return;
        }

        _currentCycleTime = (_currentCycleTime + Time.deltaTime) % _cycleLenght;

        // NORMALIZED TIME (0 -> 1)
        float timePercent = _currentCycleTime / _cycleLenght;

        // PLAYER VISION EVOLUTION
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
            Debug.Log($"[DayNight] Current Time : {_currentCycleTime:F2} / {_cycleLenght}");
        }

        // Passed a mark ?
        if (Mathf.Abs(_currentCycleTime - _nextMarkTime) < _TIME_CHECK_EPSILON)
        {
            DayAndNighMark next = _marks[_currentMarkIndex];

            _light.color = next.color;
            _light.intensity = next.intensity;

            // DEBUG
            if (_debugLogs)
            {
                Debug.Log(
                    $"[DayNight] Mark reached -> " +
                    $"Index : {_currentMarkIndex}, " +
                    $"Next Time : {_nextMarkTime:F2}, " +
                    $"Intensity : {next.intensity}"
                );
            }
            
            _CycleMarks();
        }
    }

    private void _CycleMarks()
    {
        // Safety check
        if (_marks == null || _marks.Length == 0)
            return;

        _currentMarkIndex = (_currentMarkIndex + 1) % _marks.Length;
        _nextMarkIndex = (_currentMarkIndex + 1) % _marks.Length;
        _nextMarkTime = _marks[_nextMarkIndex].timeRatio * _cycleLenght;

        // DEBUG
        if (_debugLogs)
        {
            Debug.Log(
                $"[DayNight] New target mark : {_nextMarkIndex} " +
                $"at {_nextMarkTime:F2}s"
            );
        }
    }
}