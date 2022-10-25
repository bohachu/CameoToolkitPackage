using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class TimerCountdown : MonoBehaviour
{
    public Image CounterBar;
    public Text CountdownText;
    public int TimeLimit;
    public UnityEvent OnTimeEnd;
    public bool AutoStart = true;
    private int CountdownTime;
    private int StartTime;

    bool stop=true;
    int countingTime = 0;
    
    void Start()
    {
        if(AutoStart)
        {
            StartTimmer(TimeLimit, null);
        }
    }
    public void StartTimmer(int Duration, UnityAction OnEnd)
    {
        ResetTime(Duration);
        updateTimmerUI();
        if (OnEnd != null)
            OnTimeEnd.AddListener(OnEnd);
        PauseResumeCountDown(false);
    }
    public void PauseResumeCountDown(bool isPause)
    {
        if(stop&&!isPause)
        {
            StartTime = (int)Time.time-countingTime;
        }
        stop = isPause;
    }
    public void StopCount(){
        stop = true;
        Debug.Log("total time: "+ countingTime);
    }public void ResetTime(int Seconds){
        TimeLimit =  Seconds;
        CountdownTime = Seconds;
        StartTime = (int) Time.time;
        updateTimmerUI();
    }

    public int GetWorkTime(){
        return countingTime;
    }
    void updateTimmerUI()
    {
        CountdownText.text = CountdownTime.ToString();
        CounterBar.fillAmount = (float)CountdownTime / TimeLimit;
    }
    // Update is called once per frame
    void Update()
    {
        if (stop) return;
        if ((CountdownTime > 0)){
            CountdownTime = TimeLimit + StartTime - (int) Time.time;
            countingTime=(int) Time.time - StartTime;
        }

        if (CountdownTime <= 0){
            StopCount();
            OnTimeEnd.Invoke();
            CountdownTime = 0;
        }
        updateTimmerUI();
    }
}
