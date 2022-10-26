using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI scoreLabel;

    [SerializeField]
    TextMeshProUGUI highScoreLabel;

    [SerializeField]
    GameObject score;

    [SerializeField]
    GameObject highScore;

    [SerializeField]
    GameObject startButton;

    [SerializeField]
    GameObject[] buttons;

    public int currentScore = 0;
    public int currentHighScore = 0;
    bool gameStarted = false;
    bool gameLoaded = false;
    bool showSequence = false;
    bool showingSequence = false;
    int currentIndex = 0;

    List<int> sequence = new List<int>();

    // array that replaces working array once workingarray size drops below 25% in size
    static int[] originalArray = { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3 };

    // List that fuels the random selection, however, as numbers are drawn they are removed
    // thereby reducing chances of that number being drawn again right away, offering more variety
    List<int> workingList = new List<int>(originalArray);

    public enum ButtonColor
    {
        Blue,
        Green,
        Red,
        Yellow
    }

    // Start is called before the first frame update
    void Start()
    {
        // make sure button array populated
        Debug.Assert(buttons.Length != 0);
        Debug.Assert(startButton != null);
        Debug.Assert(highScore != null);

        // restore colors to default brightness
        restoreColors();
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
            scoreLabel.text = currentScore.ToString();
            highScoreLabel.text = currentHighScore.ToString();

            // loop through list if user completed previous sequence
            if (showSequence)
            {
                showSequence = false;
                showingSequence = true;
                StartCoroutine(enlighten());
              
            }
          
        }
        
    }

    void lightenUp(int index)
    {
        float H, S, V;

        Color.RGBToHSV(buttons[index].
            GetComponent<UnityEngine.UI.Image>().color, out H, out S, out V);

        buttons[index].GetComponent<UnityEngine.UI.Image>().color =
            Color.HSVToRGB(H, S, 1);
        
    }

    IEnumerator enlighten()
    {
        //Print the time of when the function is first called.
        //Debug.Log("Started Coroutine at timestamp : " + Time.time);

        foreach (int x in sequence)
        {
            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(1);

            lightenUp(x);

            yield return new WaitForSeconds(1);
            restoreColors();
        }
        // once sequence completes, reset currentIndex
        // and re-enable button functioality
        showingSequence = false;
        currentIndex = 0;
        //After we have waited 5 seconds print the time again.
        //Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }

    void restoreColors()
    {
        for (int x = 0; x < buttons.Length; ++x)
        {
            float H, S, V;
            Color.RGBToHSV(buttons[x].GetComponent<UnityEngine.UI.Image>().color, out H, out S, out V);
            buttons[x].GetComponent<UnityEngine.UI.Image>().color =
                Color.HSVToRGB(H, S, 0.64f);
        }
    }

    int getNextButton() {

        // add to sequence
        int roll = Random.Range(0, workingList.Count);
        int selectedButton = workingList[roll];
        workingList.RemoveAt(roll);

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
            restoreColors();
            sequence.Add(getNextButton());
            showSequence = true;
            gameStarted = true;
        }
        else
        {
            startButton.SetActive(false);
            restoreColors();
            showSequence = true;
            gameStarted = true;
            gameLoaded = false;
        }
    }
}
