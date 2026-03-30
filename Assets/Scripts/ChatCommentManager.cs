using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatCommentManager : MonoBehaviour
{
    [SerializeField] private ChatWidgetUI chatWidgetUI;

    public static ChatCommentManager Instance;

    private ChatCommentDatabase database;
    private Dictionary<ChatSentiment, HashSet<int>> usedIndices = new Dictionary<ChatSentiment, HashSet<int>>();

    void Awake()
    {
        Instance = this;

        var json = Resources.Load<TextAsset>("ChatComments");
        if (json == null)
        {
            Debug.LogError("ChatComments.json not found in Resources!");
            return;
        }
        database = JsonUtility.FromJson<ChatCommentDatabase>(json.text);

        usedIndices[ChatSentiment.Positive] = new HashSet<int>();
        usedIndices[ChatSentiment.Neutral] = new HashSet<int>();
        usedIndices[ChatSentiment.Negative] = new HashSet<int>();
    }

    void Start()
    {
        int streamerIndex = PlayerPrefs.GetInt("SelectedStreamerIndex", 0);
        int speedIndex = ShapeFactory.Instance.FacecamCount - 1; // Speed is last in the list

        if (streamerIndex == speedIndex)
        {
            var speedJson = Resources.Load<TextAsset>("ChatComments_Speed");
            if (speedJson != null)
                database = JsonUtility.FromJson<ChatCommentDatabase>(speedJson.text);
        }
    }

    void OnEnable()
    {
        RoundManager.OnStateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        RoundManager.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(RoundState state)
    {
        if (state == RoundState.BetweenRounds)
            chatWidgetUI.Show();
        else
            chatWidgetUI.Hide();
    }

    public void ShowComment(ChatSentiment sentiment, float delay)
    {
        StartCoroutine(ShowCommentAfterDelay(sentiment, delay));
    }

    private IEnumerator ShowCommentAfterDelay(ChatSentiment sentiment, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (database == null || chatWidgetUI == null) yield break;
        var comment = PickComment(sentiment);
        if (comment != null)
            chatWidgetUI.AddMessage(comment.username, comment.message);
    }

    private ChatComment PickComment(ChatSentiment sentiment)
    {
        var pool = database.GetCategory(sentiment);
        if (pool == null || pool.Length == 0) return null;

        var used = usedIndices[sentiment];
        if (used.Count >= pool.Length)
            used.Clear();

        var available = new List<int>();
        for (int i = 0; i < pool.Length; i++)
        {
            if (!used.Contains(i))
                available.Add(i);
        }

        if (available.Count == 0) return null;

        int chosen = available[Random.Range(0, available.Count)];
        used.Add(chosen);
        return pool[chosen];
    }
}
