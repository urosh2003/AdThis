using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatWidgetUI : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform contentParent;
    [SerializeField] private TMP_FontAsset chatFont;
    [SerializeField] private int maxMessages = 20;
    [SerializeField] private int fontSize = 16;
    [SerializeField] private Color messageColor = Color.white;

    // ARQ16 palette (bright subset) for cycling username colors
    private static readonly Color[] UsernamePalette =
    {
        new Color32(0xb1, 0x3e, 0x53, 0xFF), // #b13e53 red
        new Color32(0xef, 0x7d, 0x57, 0xFF), // #ef7d57 orange
        new Color32(0xff, 0xcd, 0x75, 0xFF), // #ffcd75 yellow
        new Color32(0xa7, 0xf0, 0x70, 0xFF), // #a7f070 light green
        new Color32(0x38, 0xb7, 0x64, 0xFF), // #38b764 green
        new Color32(0x3b, 0x5d, 0xc9, 0xFF), // #3b5dc9 blue
        new Color32(0x41, 0xa6, 0xf6, 0xFF), // #41a6f6 light blue
        new Color32(0x73, 0xef, 0xf7, 0xFF), // #73eff7 cyan
        new Color32(0xf4, 0xf4, 0xf4, 0xFF), // #f4f4f4 white
        new Color32(0x94, 0xb0, 0xc2, 0xFF), // #94b0c2 light gray
    };

    private int _colorIndex;

    public void Show()
    {
    }

    public void Hide()
    {
        ClearAllMessages();
    }

    [SerializeField] private float textWidth = 150f;
    [SerializeField] private float textHeight = 20f;
    [SerializeField] private float textFontSize;
    public void AddMessage(string username, string message)
    {
        if (contentParent == null) return;

        // Cull oldest messages
        if (contentParent.childCount >= maxMessages)
        {
            int childrenToDestroy = contentParent.childCount - maxMessages + 1;
            for (int i = 0; i < childrenToDestroy; i++)
            {
                if (contentParent.childCount == 0) break;
                var child = contentParent.GetChild(0);
                child.SetParent(null);
                Destroy(child.gameObject);
            }
        }

        // Create message GO
        var messageGO = new GameObject("ChatMessage");
        messageGO.transform.SetParent(contentParent, false);

        var rectTransform = messageGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.sizeDelta = new Vector2(textWidth, textHeight);

        var tmp = messageGO.AddComponent<TextMeshProUGUI>();
        Color nameColor = UsernamePalette[_colorIndex];
        _colorIndex = (_colorIndex + 1) % UsernamePalette.Length;
        string hexColor = ColorUtility.ToHtmlStringRGB(nameColor);
        string hexMsg = ColorUtility.ToHtmlStringRGB(messageColor);
        tmp.text = $"<color=#{hexColor}>{username}</color>: <color=#{hexMsg}>{message}</color>";
        tmp.fontSize = fontSize;
        tmp.richText = true;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;

        if (chatFont != null)
            tmp.font = chatFont;

        var layout = messageGO.AddComponent<LayoutElement>();
        layout.minHeight = fontSize + 4;
        layout.flexibleWidth = 1;

        var fitter = messageGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Force layout rebuild and scroll to bottom
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ClearAllMessages()
    {
        if (contentParent == null) return;
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}
