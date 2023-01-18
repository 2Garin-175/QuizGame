using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Linq;

public class GameManager : MonoBehaviour
{
    const int POINTS_FOR_ANSWER = 1;
    const int ANSWER_BLINKS_COUNT = 5;
    const string TIME_SAVE_KEY = "BestTime";

    //[SerializeField] поля для заполнения в редакторе
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

    //основная логика игры
    #region MainLogic

    private List<Question> quaternionsPool;
    private Question currentQuestion;

    private Coroutine timer;
    private int time;

    private int mistakeCount;
    private int rightAnswerCount;

    private void Start() //загрузка времени при старте игры
    {
        if(PlayerPrefs.HasKey(TIME_SAVE_KEY))
            menuBestTimeText.text = ParceTimeToString(PlayerPrefs.GetInt(TIME_SAVE_KEY));
    }

    private void StartGameoLogic(bool randomMode) //запуск игры, если рандом мод то перемешиваем массив
    {
        mistakeCount = 0;
        foreach (GameObject mistak in mistakes) //сброс макеров ошибок
        {
            mistak.SetActive(false);
        }

        time = 0;
        timer = StartCoroutine(Timer());

        quaternionsPool = questions.ToList(); //загружаем вопросы из массива вопросов
        
        if (randomMode) //перемешиваем масив
        {
            for (int i = 0; i < quaternionsPool.Count; i++)
            {
                Question temp = quaternionsPool[i];
                int randomIndex =UnityEngine.Random.Range(i, quaternionsPool.Count);
                quaternionsPool[i] = quaternionsPool[randomIndex];
                quaternionsPool[randomIndex] = temp;
            }
        }

        LoadNextQuestion(); //загружаем первый вопрос
    }

    private void LoadNextQuestion() //загрузка следующего вопроса, подгрущаем всегда 0 вопрос, тк массив уже перемешан
    {
        if (quaternionsPool.Count == 0) //проверяем остались ли в листе вопросы
        {
            FinishGame();
        }
        else
        {
            currentQuestion = quaternionsPool[0];
            quaternionsPool.Remove(currentQuestion); //удаляем вопрос который загрузили

            questionsBoardText.text = currentQuestion.questionText;

            string[] temp = new string[4] { currentQuestion.answer_A, currentQuestion.answer_B, currentQuestion.answer_C, currentQuestion.answer_D };
            for(int i = 0;i< answers.Length;i++)
            {
                answers[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = temp[i]; //выставляем ответы, для вопроса
            }

            //answers[0].transform.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answer_A; //  <-- второй вариант, выглядит неочень)
            //answers[1].transform.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answer_B;
            //answers[2].transform.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answer_C;
            //answers[3].transform.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answer_D;
        }
    }

    private void CheckAnswer(int answerID) //проверка правельный ли ответ
    {
        if (answerID == (int)currentQuestion.correctAnswer)
            StartCoroutine(MakeRightAnswer(answerID));
        else
            StartCoroutine(MakeMistake(answerID));
    }

    IEnumerator MakeRightAnswer(int answerID) //правильный ответ, и анимация мерцания правильного ответа
    {
        rightAnswerCount++;

        Image tempImage = answers[answerID].GetComponent<Image>();

        for (int i = 0; i < ANSWER_BLINKS_COUNT; i++)
        {
            tempImage.color = Color.green;
            yield return new WaitForSeconds(0.2f);
            tempImage.color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }

        LoadNextQuestion();
    }

    IEnumerator MakeMistake(int answerID) //не правильный ответ, отображаем правильный ответ, и анимация выбраного ответа
    {
        mistakes[mistakeCount].SetActive(true);
        mistakeCount++;

        Image tempRightImage = answers[(int)currentQuestion.correctAnswer].GetComponent<Image>();
        Image tempMistakeImage = answers[answerID].GetComponent<Image>();

        tempRightImage.color = Color.green;

        for (int i = 0; i < ANSWER_BLINKS_COUNT; i++)
        {
            tempMistakeImage.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            tempMistakeImage.color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }

        tempRightImage.color = Color.white;

        if(mistakeCount == mistakes.Length)
            FinishGame(true);
        else
            LoadNextQuestion();
    }

    private void FinishGame(bool mistakeFinish = false) //завершение игры, и если мы прошли все вопросы то записываем время 
    {
        StopCoroutine(timer); 

        finishMenu.SetActive(true);

        finishTimeText.text = ParceTimeToString(time);
        
        finishScoreText.text = rightAnswerCount.ToString();
        // finishScoreText.text = ((rightAnswerCount * POINTS_FOR_ANSWER) + (mistakeCount * -POINTS_FOR_ANSWER)).ToString(); // <-- вариант, если хотим отображать счет, а не кол-во ответов

        if (!mistakeFinish)
            UpdateTimeData();
    }

    private void UpdateTimeData() //сохранение и обновление рекорда времени
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

    private string ParceTimeToString(int time) //секунды в формат мин:сек
    {
        string sec = time % 60 > 10 ? $"{time % 60}" : $"0{time % 60}";
        return $"{time / 60}:{sec}";
    }

    IEnumerator Timer() //просто таймер
    {
        while(true)
        {
            yield return new WaitForSecondsRealtime(1);
            time++;
        }
    }
    #endregion

    //методы кнопок
    #region Buttons
    public void StartGame(bool randomMode) //кнопка старта игры, и формат порядка вопросов
    {
        mainMenu.SetActive(false);
        gameScren.SetActive(true);
        StartGameoLogic(randomMode);
    }

    public void Answer(int answerID) //кнопки ответов
    {
        CheckAnswer(answerID);
    }

    public void BackToMenu() //кнопка на финишном скрине
    {
        gameScren.SetActive(false);
        finishMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void Exit() //кнопка выхода из игры
    {
        Application.Quit();
    }
    #endregion
}

[Serializable]
class Question //сам вопрос
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