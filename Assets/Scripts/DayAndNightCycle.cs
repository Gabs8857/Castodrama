using System.Collections;
using System.Collections.Generic;   
using UnityEngine;

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
    if (_currentCycleTime >= _nextMarkTime)
    {
        _CycleMarks();

    }
    private void _CycleMarks()
    {
        _currentMarkIndex = (_currentMarkIndex + 1) % _marks.Length;
        _nextMarkIndex = (_currentMarkIndex + 1) % _marks.Length;
        _nextMarkTime = _marks[_nextMarkIndex].timeRatio * _cycleLenght;
    }
}
