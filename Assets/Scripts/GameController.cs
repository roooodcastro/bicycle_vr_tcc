using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(HighScores))]
public class GameController : MonoBehaviour {
    public GameObject player;
    public GameObject pauseMenu;
    public GameObject finishScreen;
    public GameObject startCamera;
    public GameObject playerDashboard;
    public GameObject finishNoHighscore;
    public GameObject finishNewHighscore;
    
    public AudioSource countdown;
    public InputField nameInput;
    
    public Text distanceMeter;
    public Text stopwatch;
    public Text speedometer;
    public Text startText;
    public Text finishTimeText;

    private PlayerProgress playerProgress;
    private PlayerBicycleController playerController;
    private HighScores highScores;
    
    private bool paused;
    private float finishTime;
    private float timeChangedState;
    private GameState gameState;

    enum GameState {
        PREGAME,
        STARTUP,
        GAME,
        PAUSED,
        FINISHED
    }

    void Awake() {
        playerProgress = player.GetComponent<PlayerProgress>();
        playerController = player.GetComponent<PlayerBicycleController>();
        highScores = GetComponent<HighScores>();
        paused = false;
        finishTime = 0;
        AudioListener.pause = false;
        ChangeState(GameState.PREGAME);
    }

    // Update is called once per frame
    void Update() {
        ReadEscapeKey();
        
        switch (gameState) {
            case GameState.PREGAME:
                break;
            case GameState.STARTUP:
                UpdateDashboards();
                DoStartCountdown();
                break;
            case GameState.GAME:
                UpdateDashboards();
                if (Time.timeSinceLevelLoad - timeChangedState >= 1 && startText.gameObject.activeInHierarchy) {
                    startText.gameObject.SetActive(false);
                }
                
                finishTime = Time.timeSinceLevelLoad - timeChangedState;
                if (playerProgress.isFinished()) {
                    ChangeState(GameState.FINISHED);
                }
                break;
            case GameState.PAUSED:
                break;
            case GameState.FINISHED:
                if (!finishScreen.activeInHierarchy) {
                    playerController.userHasControl = false;
                    ShowFinishScreen();
                }
                break;
        }
    }

    public void ButtonResumeClick() {
        TogglePause();
    }

    public void ButtonRestartClick() {
        if (paused)
            TogglePause();
        SceneManager.LoadScene("Game");
    }

    public void ButtonHighscoresClick() {
        highScores.Show();
    }

    public void ButtonQuitClick() {
        Application.Quit();
    }

    public void ButtonStartClick() {
        ChangeState(GameState.STARTUP);
    }

    public void ButtonSaveHighscoreClick() {
        if (nameInput.text.Length >= 3) {
            highScores.WriteScore(nameInput.text, finishTime);
            SceneManager.LoadScene("Game");
        }
    }

    public void UseSensorsChanged(bool value) {
        player.GetComponent<PlayerBicycleController>().SetSensors(value);
    }

    private void ReadEscapeKey() {
        if (Input.GetButtonDown("Pause") && CanPause()) {
            if (highScores.isOverlayActive()) {
                highScores.Hide();
            }
            else {
                TogglePause();
            }
        }
    }

    private bool CanPause() {
        return gameState == GameState.STARTUP || gameState == GameState.GAME;
    }

    private void TogglePause() {
        paused = !paused;
        Time.timeScale = paused ? 0 : 1;
        pauseMenu.SetActive(paused);
        AudioListener.pause = paused;
    }

    private void ShowFinishScreen() {
        finishScreen.SetActive(true);
        finishTimeText.text = "Your time was " + FormatGameTime(finishTime);
        if (highScores.ScoreIsHighscore(finishTime)) {
            finishNewHighscore.SetActive(true);
            finishNoHighscore.SetActive(false);
        }
        else {
            finishNewHighscore.SetActive(false);
            finishNoHighscore.SetActive(true);
        }
    }

    private void UpdateDashboards() {
        speedometer.text = FormatSpeed();
        distanceMeter.text = FormatProgress();
        if (gameState == GameState.GAME) {
            stopwatch.text = FormatGameTime(Time.timeSinceLevelLoad - timeChangedState);
        }
    }

    private void DoStartCountdown() {
        float timeSinceChangedState = Time.timeSinceLevelLoad - timeChangedState;
        if (!startText.gameObject.activeInHierarchy) {
            startText.gameObject.SetActive(true);
            player.SetActive(true);
            startCamera.SetActive(false);
            playerDashboard.SetActive(true);
        }
        
        if (timeSinceChangedState >= 2 && timeSinceChangedState < 3) {
            if (!countdown.isPlaying) {
                countdown.Play();
            }
            startText.text = "3";
        }
        else if (timeSinceChangedState >= 3 && timeSinceChangedState < 4) {
            startText.text = "2";
        }
        else if (timeSinceChangedState >= 4 && timeSinceChangedState < 5) {
            startText.text = "1";
        }
        else if (timeSinceChangedState >= 5) {
            startText.text = "GO!";
            playerController.userHasControl = true;
            ChangeState(GameState.GAME);
        }
    }

    private String FormatGameTime(float seconds) {
        float millis = seconds - Mathf.Floor(seconds);
        string strMillis = ((int) Mathf.Floor(millis * 1000)).ToString().PadLeft(3, '0');
        string strSeconds = ((int) Mathf.Floor(seconds % 60)).ToString().PadLeft(2, '0');
        string strMinutes = ((int) Mathf.Floor(seconds / 60)).ToString().PadLeft(2, '0');
        return strMinutes + ":" + strSeconds + ":" + strMillis;
    }

    protected string FormatSpeed() {
        float speed = player.GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
        return speed.ToString("F0").PadLeft(3, '0') + " kph";
    }

    protected string FormatProgress() {
        float remaining = player.GetComponent<PlayerProgress>().RemainingDistance();
        return remaining.ToString("F1").PadLeft(5, '0') + " m";
    }

    private void ChangeState(GameState newState) {
        gameState = newState;
        timeChangedState = Time.timeSinceLevelLoad;
    }
}