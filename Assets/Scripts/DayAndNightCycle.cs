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
    [SerializeField] private float _cycleLenght = 24; // in seconds
    [SerializeField] private Light2D _light;

    private const float _TIME_CHECK_EPSILON = 0.1f;
    
    private float _currentCycleTime;
    private int _currentMarkIndex, _nextMarkIndex;
    private float _currentMarkTime, _nextMarkTime;

    private bool IsConfigured => _marks != null && _marks.Length > 0 && _cycleLenght > 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_light == null)
        {
            _light = FindFirstObjectByType<Light2D>();
        }

        if (!IsConfigured)
        {
            enabled = false;
            return;
        }

        _currentMarkIndex = -1;
        _CycleMarks();
    }

    // Update is called once per frame
    void Update()
    {
       if (_light == null)
       {
           return;
       }

       _currentCycleTime = (_currentCycleTime + Time.deltaTime) % _cycleLenght;

        // Passed a mark ?
        if (Mathf.Abs(_currentCycleTime - _nextMarkTime) < _TIME_CHECK_EPSILON)
        {
            DayAndNighMark next = _marks[_nextMarkIndex];

            _light.color = next.color;
            _light.intensity = next.intensity;
            
            _CycleMarks();
        }
    }

    private void _CycleMarks()
    {
        if (!IsConfigured)
        {
            return;
        }

        _currentMarkIndex = (_currentMarkIndex + 1) % _marks.Length;
        _nextMarkIndex = (_currentMarkIndex + 1) % _marks.Length;
        _currentMarkTime = _marks[_currentMarkIndex].timeRatio * _cycleLenght;
        _nextMarkTime = _marks[_nextMarkIndex].timeRatio * _cycleLenght;
    }
}