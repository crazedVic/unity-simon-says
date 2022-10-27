using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static Action OnGameStart;
    
    [SerializeField]
    private TextMeshProUGUI scoreLabel;

    [SerializeField]
    private TextMeshProUGUI highScoreLabel;

    [SerializeField]
    private GameObject score;

    [SerializeField]
    private Slider speedSlider;

    [SerializeField]
    private GameObject highScore;

    [SerializeField]
    private GameObject startButton;

    [SerializeField]
    private Button[] buttons;

    public int currentScore = 0;
    public int currentHighScore = 0;
    private bool gameStarted = false;
    private bool gameLoaded = false;
    private bool showSequence = false;
    private bool showingSequence = false;
    private int currentIndex = 0;
    private float sequenceSpeed = 1.0f;

    private List<int> sequence = new List<int>();

    // array that replaces working array once workingarray size drops below 25% in size
    private static int[] originalArray = { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3 };

    // List that fuels the random selection, however, as numbers are drawn they are removed
    // thereby reducing chances of that number being drawn again right away, offering more variety
    private List<int> workingList = new List<int>(originalArray);

    public enum ButtonColor
    {
        Blue,
        Green,
        Red,
        Yellow
    }

    private void Awake()
    {
        AnimationEventHandler.OnScoreAdd += OnScoreAdd;
        AnimationEventHandler.OnHighScoreChangeEvent += OnHighScoreChangeEvent;
    }

    private void OnHighScoreChangeEvent()
    {
        currentHighScore = currentScore;
        currentScore = 0;
        highScoreLabel.text = currentHighScore.ToString();
    }

    private void OnScoreAdd()
    {
        currentScore++;
        scoreLabel.text = currentScore.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        // make sure button array populated
        Debug.Assert(buttons.Length != 0);
        Debug.Assert(startButton != null);
        Debug.Assert(highScore != null);

        // restore colors to default brightness
        RestoreColors();
        LoadGame();
    }

    void LoadGame()
    {
        Debug.Log("Loading Game");
        currentHighScore = PlayerPrefs.GetInt("highScore",0);
        currentScore = PlayerPrefs.GetInt("currentScore", 0);

        if(currentScore > 0) {
            string tempSequence = PlayerPrefs.GetString("currentSequence", "");
            Debug.Log(tempSequence);
            sequence.AddRange(Array.ConvertAll(
                tempSequence.ToCharArray(), c => (int)Char.GetNumericValue(c)));
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Tap to continue game.";
            gameLoaded = true;
        }


        Debug.Log(sequence);
        scoreLabel.text = currentScore.ToString();
        highScoreLabel.text = currentHighScore.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {

            // loop through list if user completed previous sequence
            if (showSequence)
            {
                StartCoroutine(enlighten());
            }
        }
    }

    void lightenUp(int index)
    {
        float H, S, V;

        Color.RGBToHSV(buttons[index].image.color, out H, out S, out V);

        buttons[index].image.color = Color.HSVToRGB(H, S, 1);
    }

    IEnumerator enlighten()
    {
        showSequence = false;
        showingSequence = true;
        //Print the time of when the function is first called.
        //Debug.Log("Started Coroutine at timestamp : " + Time.time);

        foreach (int x in sequence)
        {
            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(sequenceSpeed);

            lightenUp(x);

            yield return new WaitForSeconds(sequenceSpeed);
            RestoreColors();
        }
        // once sequence completes, reset currentIndex
        // and re-enable button functioality
        showingSequence = false;
        currentIndex = 0;
        //After we have waited 5 seconds print the time again.
        //Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }

    void RestoreColors()
    {
        for (int x = 0; x < buttons.Length; ++x)
        {
            float H, S, V;
            Color.RGBToHSV(buttons[x].image.color, out H, out S, out V);
            buttons[x].image.color = Color.HSVToRGB(H, S, 0.64f);
        }
    }

    int getNextButton() 
    {
        // add to sequence
        int roll = Random.Range(0, workingList.Count);
        int selectedButton = workingList[roll];
        workingList.RemoveAt(roll);

        // if we've used up most of the numbers in the worklist, reset it
        if (workingList.Count < 4)
        {
            workingList = new List<int>(originalArray);
        }

        return selectedButton;
    }


    public void OnClick(int index)
    { 
       
        // if sequence being shown ignore clicks
        // TODO: should probably disable buttons during this state.
        if (showingSequence)
            return;

        if (sequence[currentIndex] == index)
        {
            // see if this is the last one or not
            if (currentIndex + 1 == sequence.Count)
            {
                sequence.Add(getNextButton());
                showSequence = true;

                // animate and update score via animation event
                PlayerPrefs.SetInt("currentScore", currentScore + 1);
                string tempSequence = String.Join("", sequence);
                PlayerPrefs.SetString("currentSequence", tempSequence);
                score.GetComponent<Animator>().SetTrigger("updateScore");
            }
            else
            {
                currentIndex++;
            }
        }
        else
        {
            // game over
            GameOver();

        }
        //ButtonColor color = (ButtonColor)index;
        //switch (color)
        //{
        //    case ButtonColor.Blue:
        //        currentScore++;
        //        Debug.Log("Blue");
        //        break;
        //    case ButtonColor.Green:
        //        currentScore++;
        //        Debug.Log("Green");
        //        break;
        //    case ButtonColor.Red:
        //        currentScore++;
        //        Debug.Log("Red");
        //        break;
        //    case ButtonColor.Yellow:
        //        currentScore++;
        //        Debug.Log("Yellow");
        //        break;
        //}

        // if sequence never broken then get next color

    }

    public void onSliderChange()
    {
        sequenceSpeed = speedSlider.value;
    }

    public void GameOver()
    {
        if (currentScore > currentHighScore)
        {
            highScore.GetComponent<Animator>().SetTrigger("newhighscore");
            PlayerPrefs.SetInt("highScore", currentScore);
        }
        PlayerPrefs.SetInt("currentScore", 0);
        PlayerPrefs.SetString("currentSequence", "");
        workingList = new List<int>(originalArray);
        currentIndex = 0;
        sequence.Clear();
        startButton.SetActive(true);
        startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Game Over!<br>Tap to try again";
    }

    public void OnStartClick()
    {
        // click here to begin game
        if (!gameLoaded)
        {
            startButton.SetActive(false);
            currentIndex = 0;
            currentScore = 0;
            sequence.Add(getNextButton());
            
            OnGameStart?.Invoke();
        }
        else
        {
            startButton.SetActive(false);
            gameLoaded = false;
        }
        gameStarted = true;
        showSequence = true;
        RestoreColors();
    }
}
