using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{

    [SerializeField]
    GameManager gameManager;

    private void Start()
    {
        Debug.Assert(gameManager != null);
    }

    public void onScoreChangeEvent()
    {
        gameManager.currentScore++;
    }

    public void onHighScoreChangeEvent()
    {
        gameManager.currentHighScore = gameManager.currentScore;
        gameManager.currentScore = 0;
    }
}
