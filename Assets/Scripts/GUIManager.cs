using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GUIManager : MonoBehaviour
{
    public static GUIManager instance;

    public GameObject gamePanel;
    public GameObject gameOverPanel;
    public GameObject pausePanel;

    public TextMeshProUGUI scoreTxt;
    public TextMeshProUGUI highScoreTxt;

    public TextMeshProUGUI GOscoreTxt;
    public TextMeshProUGUI GOhighScoreTxt;

    public TextMeshProUGUI timerTxt;
    private int score;
    [SerializeField]private float timeRemaining;
    public bool timerIsRunning;

    private void Awake()
    {
        gamePanel.SetActive(true);
        gameOverPanel.SetActive(false);

        timerIsRunning = false;
        DisplayTime(timeRemaining);

        instance = GetComponent<GUIManager>();
        score = 0;
        if (!PlayerPrefs.HasKey("Highscore"))
        {
            PlayerPrefs.SetInt("Highscore", 0);
        }
        else
            highScoreTxt.text = "Highscore: " + PlayerPrefs.GetInt("Highscore").ToString();
        scoreTxt.text = "Score: " + score.ToString();
    }

    private void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining >= 0) {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else {
                timeRemaining = 0;
                DisplayTime(timeRemaining);
                timerIsRunning = false;
                GameManager.instance.gameOver = true;
                StartCoroutine(WaitForShifting());
                //Debug.Log("Timer ended");
            }
        }
    }

    public void OnPauseButtonClick()
    {
        GameManager.instance.PauseGame();
        pausePanel.SetActive(true);
        gamePanel.SetActive(false);
    }

    public void OnBackButtonClick()
    {
        GameManager.instance.UnPauseGame();
        pausePanel.SetActive(false);
        gamePanel.SetActive(true);
    }

    private void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float second = Mathf.FloorToInt(timeRemaining % 60);
        float minute = Mathf.FloorToInt(timeRemaining / 60);
        timerTxt.text = string.Format("{0:00}:{1:00}", minute, second);
    }

    public int Score
    {
        get { return score; }
        set {
            score = value;
            scoreTxt.text = "Score: " + score.ToString();
            if (score > PlayerPrefs.GetInt("Highscore"))
            {
                PlayerPrefs.SetInt("Highscore", score);
                highScoreTxt.text = "New highscore: " + score.ToString();
            }
            else
                highScoreTxt.text = "Highscore: " + PlayerPrefs.GetInt("Highscore").ToString();
        }
    }

    private void GameOver()
    {
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);

        if (score > PlayerPrefs.GetInt("Highscore"))
        {
            PlayerPrefs.SetInt("Highscore", score);
            GOhighScoreTxt.text = "New highscore: " + score.ToString();
        }
        else
            GOhighScoreTxt.text = "Highscore: " + PlayerPrefs.GetInt("Highscore").ToString();
       
        GOscoreTxt.text = "Your score: " + score.ToString();
    }

    public IEnumerator WaitForShifting()
    {
        yield return new WaitUntil(() => !Grid.instance.IsDestroying);
        yield return new WaitForSeconds(.25f);

        GameManager.instance.StartCoroutine(GameManager.instance.FadeIn(
                GameManager.instance.faderObject, GameManager.instance.faderImg));

        GameManager.instance.StartCoroutine(GameManager.instance.FadeOut(
           GameManager.instance.faderObject, GameManager.instance.faderImg));

        GameOver();
    }

}
