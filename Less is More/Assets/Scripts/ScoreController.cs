using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    public GameObject ScorePanelParent;
    public Text NamesText;
    public Text FinesText;
    public Text RoundsText;

    private bool _showingPanel;
    private bool _outOfDate; // To avoid updating panel

    private class ScoreEntry : IComparable
    {
        public string Name;
        public int Score;
        public int Rounds;

        public ScoreEntry(string name, int score, int rounds)
        {
            this.Name = name;
            this.Score = score;
            this.Rounds = rounds;
        }

        // ascending order
        public int CompareTo(object obj)
        {
            ScoreEntry other = obj as ScoreEntry;
            if (other == null)
                return -1;
            
            return this.Score.CompareTo(other.Score);
        }
    }

    private Dictionary<int, ScoreEntry> Scores;

    private void Awake()
    {
        Scores = new Dictionary<int, ScoreEntry>();
        HideScores();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowScores();
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            HideScores();
        }
    }

    public void ShowScores()
    {
        if (_outOfDate)
            UpdatePanels();
        
        ScorePanelParent.SetActive(true);
        _showingPanel = true;
    }

    public void HideScores()
    {
        ScorePanelParent.SetActive(false);
        _showingPanel = false;
    }

    public void RemovePlayer(int id)
    {
        if (Scores.ContainsKey(id))
            Scores.Remove(id);
        
        _outOfDate = true;

        if (_showingPanel)
            UpdatePanels();
    }

    public void AddPlayer(int id, string playerName)
    {
        Scores[id] = new ScoreEntry(playerName, 0, 0);
        _outOfDate = true;
        
        if (_showingPanel)
            UpdatePanels();
    }

    public void SetPlayerScore(int id, int score, int roundsPlayed)
    {
        if (Scores.ContainsKey(id))
        {
            Scores[id].Score = score;
            Scores[id].Rounds = roundsPlayed;
        }
            
        _outOfDate = true;

        if (_showingPanel)
            UpdatePanels();
    }

    private void UpdatePanels()
    {
        List<ScoreEntry> sortedEntries = Scores.Values.ToList();
        sortedEntries.Sort();

        string names = "";
        string fines = "";
        string rounds = "";
        foreach (var entry in sortedEntries)
        {
            names += entry.Name + "\n";
            fines += "$" + entry.Score + "\n";
            rounds += entry.Rounds + "\n";
        }

        NamesText.text = names;
        FinesText.text = fines;
        RoundsText.text = rounds;
        
        _outOfDate = false;
    }
}
