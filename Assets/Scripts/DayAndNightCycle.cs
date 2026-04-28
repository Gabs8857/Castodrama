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

    private const float TIME_CHECK_EPSILON = 0.1f;
    
    private float _currentCycleTime;
    private int _currentMarkIndex, _nextMarkIndex;
    private float _currentMarkTime, _nextMarkTime;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _currentMarkIndex = -1;
        _CycleMarks();
    }

    // Update is called once per frame
    void Update()
    {
       _currentCycleTime = (_currentCycleTime + Time.deltaTime) % _cycleLenght;

    // Passed a mark ?
    if (Mathf.Abs(_currentCycleTime - _nextMarkTime) < TIME_CHECK_EPSILON)
    {
        _CycleMarks();

    }
    }
    private void _CycleMarks()
    {
        _currentMarkIndex = (_currentMarkIndex + 1) % _marks.Length;
        _nextMarkIndex = (_currentMarkIndex + 1) % _marks.Length;
        _nextMarkTime = _marks[_nextMarkIndex].timeRatio * _cycleLenght;
    }

}
