using UnityEngine;


public class TutorialTrigger : MonoBehaviour
{
    public TutorialUI tutorialUI; // drag Player here in Inspector
    public float timerDuration = 5f; // 5 seconds
    private float timer;
    public bool timerRunning = false;
    [TextArea] public string tutorialMessage;
    public Sprite tutorialSprite;

    private bool triggered = false;

    void Start()
    {
        tutorialUI.HideTutorial();
    }
    void Update()
    {
        if (timerRunning)
        {
            timer -= Time.deltaTime;
            

            if (timer <= 0f)
            {
                timerRunning = false;
                timer = 0f;
                TimerFinished();
            }
        }
    }
    public void StartTimer()
    {
        timer = timerDuration;
        timerRunning = true;
    }
    private void TimerFinished()
    {
        Debug.Log("5 seconds have passed!");
        tutorialUI.HideTutorial();
        // Place any logic here to trigger after 5 seconds
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided");
        if (!triggered)
        {
            tutorialUI.ShowTutorial(tutorialMessage, tutorialSprite);
            Debug.Log("BEGIN TRIGGER");
            StartTimer();
                
            

            triggered = true;
        }
    }
}