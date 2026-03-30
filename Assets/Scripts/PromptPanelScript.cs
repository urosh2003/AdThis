using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PromptPanelScript : MonoBehaviour
{
    [SerializeField] private string _nickname;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private List<TMP_Text> nameText;
    public string Nickname
    {
        get => _nickname;
        set
        {
            _nickname = value;
            for(int i = 0; i < 3; i++)
            {
                if(i >= value.Length)
                    nameText[i].text = "";
                else
                    nameText[i].text = value[i].ToString();
            }
        }
    }
    private void Awake()
    {
        Nickname = "";
        scoreText.text = $"Score: ${LeaderboardManager.Instance.Score:N0}" + " \n Your score is big enough for the leaderboard, please enter your name:";
    }
    
    public void AddLetter(string letter)
    {
        if (Nickname.Length >= 3) return;
        Nickname += letter;
    }
    
    public void RemoveLetter()
    {
        if (Nickname.Length <= 0) return;
        Nickname = Nickname.Substring(0, Nickname.Length - 1);
    }

    public async void Submit()
    {
        var nname = Nickname;
        while (nname.Length < 3)
        {
            nname += "_";
        }

        await LeaderboardManager.Instance.SetNickname(nname);
        gameObject.SetActive(false);
    }
    
    public void Cancel()
    {
        gameObject.SetActive(false);
    }
}
