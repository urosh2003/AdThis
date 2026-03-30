using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.Serialization;


public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    private const string LeaderboardId = "NotEnoughScreenLeaderboard";
    private const string NicknameKey = "PlayerNickname";

    public string PlayerNickname { get; private set; }
    [FormerlySerializedAs("_score")] public int Score;

    private Task _initializationTask;
    private bool _isInitialized;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _initializationTask = InitServices();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async Task InitServices()
    {
        if (_isInitialized) return;
        try
        {
            await UnityServices.InitializeAsync();

            if (AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut(true);

            PlayerNickname = PlayerPrefs.GetString(NicknameKey, "");
            if (string.IsNullOrEmpty(PlayerNickname))
            {
                PlayerNickname = "Player" + UnityEngine.Random.Range(1000, 9999);
                PlayerPrefs.SetString(NicknameKey, PlayerNickname);
            }
            _isInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"UGS Init Error: {e}");
        }
    }

    private async Task EnsureInitialized()
    {
        if (_initializationTask != null)
            await _initializationTask;
    }

    public async Task SetNickname(string newName)
    {
        await EnsureInitialized();
        Debug.Log($"SetNickname received: {newName}");
        PlayerNickname = newName;
        Debug.Log($"PlayerNickname: {PlayerNickname}");
        PlayerPrefs.SetString(NicknameKey, newName);
        Debug.Log($"Nickname changed to: {newName}");
        await SubmitScore(Score);
    }

    public async Task SubmitScore(int score)
    {
        await EnsureInitialized();
        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut(true);
            }
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var metadata = new Dictionary<string, string> { { "nickname", PlayerNickname } };
            AddPlayerScoreOptions addPlayerScoreOptions = new()
            {
                Metadata = metadata
            };
            var response = await LeaderboardsService.Instance.AddPlayerScoreAsync(
                LeaderboardId, score, addPlayerScoreOptions
            );
            Debug.Log($"Score submitted: {response.Score} by {PlayerNickname}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error submitting score: {e}");
        }
    }

    public async Task<List<LeaderboardEntry>> GetTopScores(int limit = 10)
    {
        await EnsureInitialized();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        var results = new List<LeaderboardEntry>();
        try
        {
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
                LeaderboardId, new GetScoresOptions { Limit = limit, IncludeMetadata = true }
            );
            results.AddRange(scoresResponse.Results);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error retrieving scores: {e}");
        }

        return results;
    }

    public async Task<double> GetNthScore(int n = 10)
    {
        await EnsureInitialized();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        var results = new List<LeaderboardEntry>();
        try
        {
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
                LeaderboardId, new GetScoresOptions { Limit = n, IncludeMetadata = true }
            );
            results.AddRange(scoresResponse.Results);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error retrieving scores: {e}");
        }

        if (results.Count - 1 >= 0 && results.Count - 1 < results.Count)
            return results[results.Count - 1].Score;
        return 0;
    }

    public async Task GameOver(int score)
    {
        var bottomScore = await GetNthScore();
        if (bottomScore < score)
        {
            Score = score;
            PromptForUsername();
        }
    }

    public void PromptForUsername()
    {
        GameObject obj = Resources
            .FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(g => g.name == "LeaderboardPromptPanel");
        if (obj != null)
            obj.SetActive(true);
    }
}

