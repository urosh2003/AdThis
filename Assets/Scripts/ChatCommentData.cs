using System;

public enum ChatSentiment
{
    Positive,
    Neutral,
    Negative
}

[Serializable]
public class ChatComment
{
    public string username;
    public string message;
}

[Serializable]
public class ChatCommentDatabase
{
    public ChatComment[] positive;
    public ChatComment[] neutral;
    public ChatComment[] negative;

    public ChatComment[] GetCategory(ChatSentiment sentiment)
    {
        switch (sentiment)
        {
            case ChatSentiment.Positive: return positive;
            case ChatSentiment.Neutral:  return neutral;
            case ChatSentiment.Negative: return negative;
            default:                     return neutral;
        }
    }
}
