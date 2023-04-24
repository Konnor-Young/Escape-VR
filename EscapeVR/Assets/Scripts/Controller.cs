using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Controller : MonoBehaviour
{
    public WSconnect audioRequests;
    public List<GameObject> r1Buttons;
    public List<GameObject> r2Buttons;
    public List<GameObject> r3Buttons;
    public UnityEvent onIntro = new UnityEvent();
    public UnityEvent onStart = new UnityEvent();
    public UnityEvent onGoodPress = new UnityEvent();
    public UnityEvent onBadPress = new UnityEvent();
    public UnityEvent onR2Bad = new UnityEvent();
    public UnityEvent onR3Bad = new UnityEvent();
    public UnityEvent roundOver = new UnityEvent();
    
    private enum GameState
    {
        r1,
        r2,
        r3,
        GameOver
    }

    private GameState current;
    private List<GameObject> currentButtons;
    private int buttonIndex;

    private void Start()
    {
        StartCoroutine(PlayIntro());
        current = GameState.r1;
        StartRound(current);
    }
    private void StartRound(GameState round)
    {
        this.current = round;
        buttonIndex = 0;
        switch (round)
        {
            case GameState.r1:
                onStart.Invoke();
                currentButtons = r1Buttons;
                break;
            case GameState.r2:
                onStart.Invoke();
                currentButtons = r2Buttons;
                break;
            case GameState.r3:
                onStart.Invoke();
                currentButtons = r3Buttons;
                break;
            case GameState.GameOver:
                audioRequests.StopAudio();
                break;
        }
    }
    public void OnButtonPress(GameObject button)
    {
       if(button == currentButtons[buttonIndex])
        {
            onGoodPress.Invoke();
            audioRequests.PlayCorrect();
            buttonIndex++;
            if(buttonIndex >= currentButtons.Count)
            {
                audioRequests.PlayAudio("wav/jumpscare.wav", false);
                roundOver.Invoke();
                current++;
                if(current == GameState.r3)
                {
                    audioRequests.PlayAudio("wav/heart.wav", true);
                }
                StartRound(current);
            }
        }
        else
        {
            if(current == GameState.r2)
            {
                onR2Bad.Invoke();
                audioRequests.PlayIncorrect();
            }
            else if(current == GameState.r3)
            {
                onR3Bad.Invoke();
                audioRequests.PlayIncorrect();
            }
            else
            {
                onBadPress.Invoke();
                audioRequests.PlayIncorrect();
            }
            StartRound(current);
        }
    }
    IEnumerator PlayIntro()
    {
        Debug.Log("Intro start");
        onIntro.Invoke();
        yield return new WaitForSeconds(20f);
        Debug.Log("Intro end");
    }
    void OnApplicationQuit()
    {
        audioRequests.StopAudio();
    }
}