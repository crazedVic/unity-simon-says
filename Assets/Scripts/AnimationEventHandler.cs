using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public static Action OnScoreAdd;
    public static Action OnHighScoreChangeEvent;

    public void onScoreChangeEvent()
    {
        OnScoreAdd?.Invoke();
    }

    public void onHighScoreChangeEvent()
    {
        OnHighScoreChangeEvent?.Invoke();
    }
}
