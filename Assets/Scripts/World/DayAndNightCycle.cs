using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DayAndNightCycle : MonoBehaviour
{
    [Range(0, 1)] [SerializeField] private float _dayTime;
    [SerializeField] private float _dayDuration;
    [SerializeField] private int _totalDaysPassed;

    [SerializeField] private int _morningStartHour;
    [SerializeField] private int _dayStartHour;
    [SerializeField] private int _eveningStartHour;
    [SerializeField] private int _nightStartHour;

    [SerializeField] private AnimationCurve _sunCurve;
    [SerializeField] private AnimationCurve _moonCurve;
    [SerializeField] private AnimationCurve _skyboxCurve;

    [SerializeField] private float _updateDelay;

    [SerializeField] private RainEvent _rainEvent;
    [SerializeField] private Material _rainyDaySkybox;
    
    [SerializeField] private Material _nightSkybox;

    [SerializeField] private Material _currentDaySkybox;

    [SerializeField] private Light _sun;
    [SerializeField] private Light _moon;

    [SerializeField] private float _timeMultiplier;
    [SerializeField] private float _nightTimeMultiplier;
    
    [SerializeField] private float _startHour;

    [SerializeField] private PlayerSleep _playerSleep;

    [SerializeField] private CargoShipArrive _cargoShipArrive;

    private DateTime _currentTime;

    private TimeSpan _newDayTime;

    private bool _newDayAdded;

    private float _sunIntensity;
    private float _moonIntensity;
    
    private string _currentDayPartText;
    
    public enum DayPart
    {
        Morning,
        Day,
        Evening,
        Night
    }
    public event UnityAction<DateTime> TimeWasChanged;
    public event UnityAction<int> PassedDaysAmountChanged;
    
    public event UnityAction<string> CurrentDayPartChanged;

    public DayPart CurrentDayPart;
    
    private void Start()
    {
        _sunIntensity = _sun.intensity;
        _moonIntensity = _moon.intensity;
        
        _currentTime = DateTime.Now.Date + TimeSpan.FromHours(_startHour);
        
        _timeMultiplier = (60 * 60) / (_dayDuration / 24); //1 hour for second in real time
        
        _newDayTime = TimeSpan.FromHours(0);
        
        StartCoroutine(Delay());

        _currentDayPartText = DayPart.Morning.ToString();

        _newDayAdded = false;

        LoadDateTimeInfo();
        PassedDaysAmountChanged?.Invoke(_totalDaysPassed);
    }

    private void Update()
    {
        _dayTime += Time.deltaTime / _dayDuration;
        
        if (_dayTime >= 1)
        {
            _dayTime = 0;
            _newDayAdded = false;
        }
        
        _sun.transform.localRotation = Quaternion.Euler(_dayTime * 360f, 0, 0);
        _moon.transform.localRotation = Quaternion.Euler(_dayTime * 360f + 180f, 0, 0);
        
        _currentTime = _currentTime.AddSeconds(Time.deltaTime  * _timeMultiplier);
        
        TimeWasChanged?.Invoke(_currentTime);

        if (_currentTime.Hour == _newDayTime.Hours && _newDayAdded == false)
        {
            _totalDaysPassed++;
            PassedDaysAmountChanged?.Invoke(_totalDaysPassed);
            _newDayAdded = true;
        }
        
        CheckDayPart(_morningStartHour, _dayStartHour, DayPart.Morning);
        CheckDayPart(_dayStartHour, _eveningStartHour, DayPart.Day);
        CheckDayPart(_eveningStartHour, _nightStartHour, DayPart.Evening);
        CheckDayPart(_nightStartHour, _morningStartHour, DayPart.Night);
    }

    private IEnumerator Delay()
    {
        while (true)
        {
            if (_dayTime >= 0.5f)
            {
                _moon.enabled = true;
                _sun.enabled = false;
            }
            else
            {
                _moon.enabled = false;
                _sun.enabled = true;
            }

            if (_rainEvent.IsRainStarted == true)
            {
                _currentDaySkybox = _rainyDaySkybox;
            }
            RenderSettings.skybox.Lerp(_nightSkybox, _currentDaySkybox, _skyboxCurve.Evaluate(_dayTime));
            RenderSettings.sun = _skyboxCurve.Evaluate(_dayTime) > 0.1f ? _sun : _moon;
            DynamicGI.UpdateEnvironment();
            _sun.intensity = _sunIntensity * _sunCurve.Evaluate(_dayTime);
            _moon.intensity = _moonIntensity * _moonCurve.Evaluate(_dayTime);
            yield return new WaitForSeconds(_updateDelay);
        }
    }

    private void SetNewDay()
    {
        _cargoShipArrive.SetTimeBetweenChances(_timeMultiplier / _nightTimeMultiplier);

        _timeMultiplier = _nightTimeMultiplier;

        StartCoroutine(CheckDayPart());
    }

    private void OnEnable()
    {
        _playerSleep.PlayerStartsSleep += SetNewDay;
    }
    private void OnDisable()
    {
        _playerSleep.PlayerStartsSleep -= SetNewDay;
    }

    private void CheckDayPart(int firstTimeBorder, int secondTimeBorder, DayPart dayPart)
    {
        if (dayPart == DayPart.Night)
        {
            if (_currentTime.Hour >= firstTimeBorder)
            {
                _currentDayPartText = dayPart.ToString();
                CurrentDayPart = dayPart;
                CurrentDayPartChanged?.Invoke(_currentDayPartText);
                return;
            }
        }
        
        if(_currentTime.Hour >= firstTimeBorder && _currentTime.Hour < secondTimeBorder && _currentDayPartText != dayPart.ToString())
        {
            CurrentDayPart = dayPart;
            _currentDayPartText = dayPart.ToString();
            CurrentDayPartChanged?.Invoke(_currentDayPartText);
        }
    }

    private IEnumerator CheckDayPart()
    {
        while (true)
        {
            var currentTime = _currentTime.Hour >= _nightStartHour ? 24 - _currentTime.Hour : _currentTime.Hour;

            if (currentTime >= _morningStartHour)
            {
                _cargoShipArrive.SetTimeBetweenChances(3);
                _timeMultiplier = 360;
                _dayTime = 0;
                break;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
#if UNITY_ANDROID && !UNITY_EDITOR
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveDateTimeInfo();
        } 
}
#endif
    private void OnApplicationQuit()
    {
        SaveDateTimeInfo();
    }

    private void SaveDateTimeInfo()
    {
        PlayerPrefs.SetFloat("DayTime", _dayTime);
        PlayerPrefs.SetInt("TotalDaysPassed", _totalDaysPassed);
        PlayerPrefs.SetInt("CurrentHours", _currentTime.Hour);
        PlayerPrefs.SetInt("CurrentMinutes", _currentTime.Minute);
    }
    private void LoadDateTimeInfo()
    {
        _dayTime = PlayerPrefs.GetFloat("DayTime", 0);
        _totalDaysPassed = PlayerPrefs.GetInt("TotalDaysPassed", 0);

        int hours = PlayerPrefs.GetInt("CurrentHours", 8);
        int minutes = PlayerPrefs.GetInt("CurrentMinutes", 0);

        _currentTime = DateTime.Now.Date + TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);
    }
}

