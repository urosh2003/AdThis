using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
[System.Serializable]
public class PlayerMetadata  // Better to use a more specific name
{
    public string nickname;
}

public class TableLeaderboard : MonoBehaviour
{
    [SerializeField] private TMP_Text leaderboardText;
    // Start is called before the first frame update
    void Start()
    {
        FetchLeaderboard();
    }

    private async Task FetchLeaderboard()
    {
        while (LeaderboardManager.Instance == null)
        {
            await Task.Yield();
        }
        var results = await LeaderboardManager.Instance.GetTopScores();
        if (results == null)
        {
            Debug.LogError("Failed to retrieve leaderboard scores");
            leaderboardText.text = "Failed to retrieve leaderboard scores";
            return;
        }
        leaderboardText.text = "";
        foreach (var result in results)
        {
            PlayerMetadata metadata = JsonUtility.FromJson<PlayerMetadata>(result.Metadata);
            leaderboardText.text += $"{result.Rank + 1}. {metadata.nickname} — ${result.Score:N0}\n";
        }       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
