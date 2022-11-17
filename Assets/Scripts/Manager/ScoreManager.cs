using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ScoreManager : MonoBehaviour
{

    public static ScoreManager instance;

    public float TimeLimit = 120f;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI EndingTotalScoreText;

    public Animator StartPanel;
    public Animator EndingPanel;

    bool _isStartPanelShowed;
    bool _isEndingPanelShowed;

    int _totalScore;

    WeaponController _playerWeaponController;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _playerWeaponController = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<WeaponController>();
        _totalScore = 0;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ShowStartPanel();
    }

    private void Update()
    {
        TimeCounting();
        UpdateScore();
        HandlePanelInput();
    }

    void HandlePanelInput()
    {
        if (_isStartPanelShowed)
        {
            if (Input.GetKey("return"))
            {
                HideStartPanel();
                _isStartPanelShowed = false;
            }
        }

        if (_isEndingPanelShowed)
        {
            if (Input.GetKey("return"))
            {
                Restart();
                _isEndingPanelShowed = false;
            }
        }
    }

    public void ShowStartPanel()
    {

        Time.timeScale = 0;
        StartPanel.SetTrigger("show");

        _isStartPanelShowed = true;
    }

    public void HideStartPanel()
    {

        StartPanel.SetTrigger("hide");
        Time.timeScale = 1;
    }

    public void ShowEndingPanel()
    {

        EndingTotalScoreText.text = _totalScore.ToString();

        EndingPanel.SetTrigger("show");
        Time.timeScale = 0;

        _isEndingPanelShowed = true;
    }

    public void Restart()
    {
        EndingPanel.SetTrigger("hide");
        Time.timeScale = 1;

        SceneManager.LoadScene("MainScene");
    }

    public void AddScore(int score)
    {
        _totalScore += score;
    }

    void UpdateScore()
    {
        ScoreText.text = "Score: " + _totalScore.ToString();
    }

    void TimeCounting()
    {
        TimeLimit -= Time.deltaTime;
        TimeText.text = "Time: " + Mathf.FloorToInt(TimeLimit).ToString() + "s";

        if (TimeLimit <= 0)
            ShowEndingPanel();
    }

    public void AmmoReward(int ammoAmount)
    {
        _playerWeaponController.MaxAmmo += ammoAmount;
    }

}
