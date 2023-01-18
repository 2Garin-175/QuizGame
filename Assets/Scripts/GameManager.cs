using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Linq;

public class GameManager : MonoBehaviour
{
    const int POINTS_FOR_ANSWER = 100;
    const string TIME_SAVE_KEY = "BestTime";

    //[SerializeField] ���� ��� ���������� � ���������
    #region Unity

    [SerializeField] private TextMeshProUGUI menuBestTimeText;

    [Header("MENUS")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject gameScren;
    [SerializeField] private GameObject finishMenu;
    
    [Header("GAME")]
    [SerializeField] private Image questionsBoardFrame;
    [SerializeField] private TextMeshProUGUI questionsBoardText;
    [SerializeField] private TextMeshProUGUI finishTimeText;
    [SerializeField] private TextMeshProUGUI finishScoreText;
    [SerializeField] private GameObject[] mistakes;

    [Header("ANSWER_BUTTONS")]
    [SerializeField] private GameObject[] answers;

    [Header("QUESTIONS")]
    [SerializeField] private Question[] questions;

    #endregion

    //�������� ������ ����
    #region MainLogic

    private List<Question> quaternionsPool;
    private Question currentQuestion;

    private Coroutine timer;
    private int time;

    private int mistakeCount;
    private int rightAnswerCount;

    private void Start() //�������� ������� ��� ������ ����
    {
        if(PlayerPrefs.HasKey(TIME_SAVE_KEY))
            menuBestTimeText.text = ParceTimeToString(PlayerPrefs.GetInt(TIME_SAVE_KEY));
    }

    private void StartGameoLogic(bool randomMode) //������ ����, ���� ������ ��� �� ������������ ������
    {
        mistakeCount = 0;
        foreach (GameObject mistak in mistakes) //����� ������� ������
        {
            mistak.SetActive(false);
        }

        time = 0;
        timer = StartCoroutine(Timer());

        quaternionsPool = questions.ToList(); //��������� ������� �� ������� ��������
        
        if (randomMode) //������������ �����
        {
            for (int i = 0; i < quaternionsPool.Count; i++)
            {
                Question temp = quaternionsPool[i];
                int randomIndex =UnityEngine.Random.Range(i, quaternionsPool.Count);
                quaternionsPool[i] = quaternionsPool[randomIndex];
                quaternionsPool[randomIndex] = temp;
            }
        }

        LoadNextQuestion(); //��������� ������ ������
    }

    private void LoadNextQuestion() //�������� ���������� �������, ���������� ������ 0 ������, �� ������ ��� ���������
    {
        if (quaternionsPool.Count == 0) //��������� �������� �� � ����� �������
        {
            FinishGame();
        }
        else
        {
            currentQuestion = quaternionsPool[0];
            quaternionsPool.Remove(currentQuestion); //������� ������ ������� ���������

            questionsBoardText.text = currentQuestion.questionText;
            answers[0].transform.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answer_A;
            answers[1].transform.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answer_B;
            answers[2].transform.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answer_C;
            answers[3].transform.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answer_D;
        }
    }

    private void CheckAnswer(int answerID) //�������� ���������� �� �����
    {
        if (answerID == (int)currentQuestion.correctAnswer)
            StartCoroutine(MakeRightAnswer(answerID));
        else
            StartCoroutine(MakeMistake(answerID));
    }

    IEnumerator MakeRightAnswer(int answerID) //���������� �����, � �������� �������� ����������� ������
    {
        rightAnswerCount++;

        Image tempImage = answers[answerID].GetComponent<Image>();

        for (int i = 0; i < 5; i++)
        {
            tempImage.color = Color.green;
            yield return new WaitForSeconds(0.2f);
            tempImage.color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }

        LoadNextQuestion();
    }

    IEnumerator MakeMistake(int answerID) //�� ���������� �����, ���������� ���������� �����, � �������� ��������� ������
    {
        mistakes[mistakeCount].SetActive(true);
        mistakeCount++;

        Image tempRightImage = answers[(int)currentQuestion.correctAnswer].GetComponent<Image>();
        Image tempMistakeImage = answers[answerID].GetComponent<Image>();

        tempRightImage.color = Color.green;

        for (int i = 0; i < 5; i++)
        {
            tempMistakeImage.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            tempMistakeImage.color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }

        tempRightImage.color = Color.white;

        if(mistakeCount == 3)
            FinishGame(true);
        else
            LoadNextQuestion();
    }

    private void FinishGame(bool mistakeFinish = false) //���������� ����, � ���� �� ������ ��� ������� �� ���������� ����� 
    {
        StopCoroutine(timer); 

        finishMenu.SetActive(true);

        finishTimeText.text = ParceTimeToString(time);
        finishScoreText.text = ((rightAnswerCount * POINTS_FOR_ANSWER) + (mistakeCount * -POINTS_FOR_ANSWER)).ToString();

        if(!mistakeFinish)
            UpdateTimeData();
    }

    private void UpdateTimeData() //���������� � ���������� ������� �������
    {
        if (PlayerPrefs.HasKey(TIME_SAVE_KEY))
        {
            if (PlayerPrefs.GetInt(TIME_SAVE_KEY) > time)
            {
                PlayerPrefs.SetInt(TIME_SAVE_KEY, time);
                menuBestTimeText.text = ParceTimeToString(time);
            }
        }
        else
        {
            PlayerPrefs.SetInt(TIME_SAVE_KEY, time);
            menuBestTimeText.text = ParceTimeToString(time);
        }
    }

    private string ParceTimeToString(int time) //������� � ������ ���:���
    {
        string sec = time % 60 > 10 ? $"{time % 60}" : $"0{time % 60}";
        return $"{time / 60}:{sec}";
    }

    IEnumerator Timer() //������ ������
    {
        while(true)
        {
            yield return new WaitForSecondsRealtime(1);
            time++;
        }
    }
    #endregion

    //������ ������
    #region Buttons
    public void StartGame(bool randomMode) //������ ������ ����, � ������ ������� ��������
    {
        mainMenu.SetActive(false);
        gameScren.SetActive(true);
        StartGameoLogic(randomMode);
    }

    public void Answer(int answerID) //������ �������
    {
        CheckAnswer(answerID);
    }

    public void BackToMenu() //������ �� �������� ������
    {
        gameScren.SetActive(false);
        finishMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void Exit() //������ ������ �� ����
    {
        Application.Quit();
    }
    #endregion
}

[Serializable]
class Question //��� ������
{
    public enum CorrectAnswer
    {
        A, B, C, D,
    }

    [TextArea(3,8)]
    public string questionText;

    public string answer_A;
    public string answer_B;
    public string answer_C;
    public string answer_D;

    public CorrectAnswer correctAnswer = (CorrectAnswer)0;
}