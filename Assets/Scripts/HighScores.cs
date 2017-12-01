using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class HighScores : MonoBehaviour {
    public GameObject highscoresOverlay;
    public Text namesText;
    public Text scoresText;

    private bool overlayActive;
    private List<Score> scores;

    // Use this for initialization
    void Awake() {
        ReadScores();
        FillHighscoresUi();
    }

    // Returns true if the given score is good enough to enter the top 10.
    public bool ScoreIsHighscore(float score) {
        float lowestScore = scores[9].score;
        return score < lowestScore;
    }
    
    public void WriteScore(string playerName, float score) {
        Score newScore = new Score(playerName, score);
        scores.Add(newScore);
        scores.Sort();
        scores.RemoveAt(10);
        SaveScores();
        ReadScores();
        FillHighscoresUi();
    }

    public void Show() {
        overlayActive = true;
        highscoresOverlay.SetActive(true);
    }

    public void Hide() {
        overlayActive = false;
        highscoresOverlay.SetActive(false);
    }

    public bool isOverlayActive() {
        return overlayActive;
    }

    public void ButtonCloseClick() {
        overlayActive = false;
        highscoresOverlay.SetActive(false);
    }

    private void SaveScores() {
        for (int i = 0; i < 10; i++) {
            SaveScore(i);
        }
    }

    private void SaveScore(int position) {
        string key = "scoreboard_" + position;
        PlayerPrefs.SetString(key, scores[position].Serialize());
    }

    private void ReadScores() {
        scores = new List<Score>();
        for (int i = 0; i < 10; i++) {
            scores.Add(ReadScore(i));
        }
        scores.Sort();
    }

    private Score ReadScore(int position) {
        string key = "scoreboard_" + position;
        string serialization = PlayerPrefs.GetString(key);
        if (!String.IsNullOrEmpty(serialization)) {
            return Score.Deserialize(serialization);
        }
        return new Score("CPU", GenerateStartHighscore(position));
    }

    private void FillHighscoresUi() {
        string[] names = new string[10];
        string[] values = new string[10];

        for (int i = 0; i < 10; i++) {
            names[i] = scores[i].playerName;
            values[i] = FormatGameTime(scores[i].score);
        }

        namesText.text = String.Join("\n", names);
        scoresText.text = String.Join("\n", values);
    }
    
    private String FormatGameTime(float seconds) {
        float millis = seconds - Mathf.Floor(seconds);
        string strMillis = ((int) Mathf.Floor(millis * 1000)).ToString().PadLeft(3, '0');
        string strSeconds = ((int) Mathf.Floor(seconds % 60)).ToString().PadLeft(2, '0');
        string strMinutes = ((int) Mathf.Floor(seconds / 60)).ToString().PadLeft(2, '0');
        return strMinutes + ":" + strSeconds + ":" + strMillis;
    }

    // Generates the default CPU highscores.
    // Assumes that first place will have a run at 35km/h and
    // following places will each run at 3km/h less than previous.
    private float GenerateStartHighscore(int position) {
        float speed = (35.0f - (position * 3)) / 3.6f; // In m/s
        float distance = 1116f; // In meters
        return distance / speed;
    }
}
