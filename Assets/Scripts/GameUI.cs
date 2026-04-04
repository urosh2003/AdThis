using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameUI : MonoBehaviour
{
    [SerializeField] private Image timerImage;
    [SerializeField] private TMP_Text viewersText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private Button doneButton;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverScoreText;

    [Header("Score Punch Effect")]
    [SerializeField] private float punchScale = 1.05f;
    [SerializeField] private float punchDuration = 0.5f;
    [SerializeField] private float dissipateSpeed = 5f;

    [Header("Floating Score Labels")]
    [SerializeField] private TMP_Text floatingLabelPrefab;
    [SerializeField] private RectTransform floatingLabelParent;
    [SerializeField] private float timeBetweenLabels = 0.25f;
    [SerializeField] private float floatSpeed = 20f;
    [SerializeField] private float floatDuration = 1.2f;

    private int _lastViewers = 0;
    private int _lastMoney = 0;
    private Coroutine _punchCoroutine;
    private Coroutine _viewersPunchCoroutine;
    private readonly Vector3 _normalScale = Vector3.one;
    private int _pendingMoneyDiff = 0;
    private Coroutine _floatingLabelCoroutine;

    private readonly Color _moneyPunchColor  = new Color(0x5a / 255f, 0xe1 / 255f, 0x50 / 255f);
    private readonly Color _moneyLossColor   = new Color(0xe9 / 255f, 0x38 / 255f, 0x41 / 255f);
    private readonly Color _viewersGainColor = new Color(0x5a / 255f, 0xe1 / 255f, 0x50 / 255f);
    private readonly Color _viewersLossColor = new Color(0xe9 / 255f, 0x38 / 255f, 0x41 / 255f);
    private Color _moneyOriginalColor;
    private Color _viewersOriginalColor;

    private void Awake()
    {
        _moneyOriginalColor   = moneyText.color;
        _viewersOriginalColor = viewersText.color;
    }

    void OnEnable()
    {
        RoundManager.OnTimerChanged      += UpdateTimer;
        RoundManager.OnStateChanged      += UpdateState;
        RoundManager.OnShapePlacedChanged += UpdateDoneButton;
        GridManager.OnViewersChanged     += UpdateViewers;
        GridManager.OnMoneyChanged       += UpdateMoney;
    }

    void OnDisable()
    {
        RoundManager.OnTimerChanged      -= UpdateTimer;
        RoundManager.OnStateChanged      -= UpdateState;
        RoundManager.OnShapePlacedChanged -= UpdateDoneButton;
        GridManager.OnViewersChanged     -= UpdateViewers;
        GridManager.OnMoneyChanged       -= UpdateMoney;
    }

    private float _lastBeepTime = -1f;

    private void UpdateTimer(float time)
    {
        timerImage.fillAmount = time / RoundManager.Instance.roundTime;

        float timeRemainingRatio = time / RoundManager.Instance.roundTime;
        float currentInterval = Mathf.Lerp(0.06f, 1.75f, timeRemainingRatio);

        if (_lastBeepTime - time >= currentInterval)
        {
            timerImage.GetComponent<AudioSource>().Play();
            _lastBeepTime = time;
        }
    }

    private void UpdateViewers(int viewers)
    {
        viewersText.text = $"Viewers: {viewers:N0}";
        int diff = viewers - _lastViewers;

        if (diff != 0)
        {
            if (_viewersPunchCoroutine != null) StopCoroutine(_viewersPunchCoroutine);

            if (diff > 0)
                _viewersPunchCoroutine = StartCoroutine(PunchEffect(viewersText, diff, _viewersOriginalColor, _viewersGainColor));
            else
                _viewersPunchCoroutine = StartCoroutine(LossEffect(viewersText, Mathf.Abs(diff), _viewersOriginalColor, _viewersLossColor));
        }

        _lastViewers = viewers;
    }

    private void UpdateMoney(int money)
    {
        int diff = money - _lastMoney;

        if (diff > 0)
        {
            _pendingMoneyDiff += diff;
            if (_floatingLabelCoroutine == null)
                _floatingLabelCoroutine = StartCoroutine(FloatingLabelTicker());

            if (_punchCoroutine != null) StopCoroutine(_punchCoroutine);
            _punchCoroutine = StartCoroutine(PunchEffect(moneyText, diff, _moneyOriginalColor, _moneyPunchColor));
        }
        else if (diff < 0)
        {
            SpawnFloatingLabel($"${diff:N0}");
            if (_punchCoroutine != null) StopCoroutine(_punchCoroutine);
            _punchCoroutine = StartCoroutine(LossEffect(moneyText, Mathf.Abs(diff), _moneyOriginalColor, _moneyLossColor));
        }

        _lastMoney = money;
        moneyText.text = $"Money: ${money:N0}";
        if (gameOverPanel != null && gameOverPanel.activeSelf)
            gameOverScoreText.text = $"${money:N0}";
    }

    private IEnumerator PunchEffect(TMP_Text target, int amount, Color originalColor, Color punchColor)
    {
        Vector3 fromScale = target.transform.localScale;
        float scaleMult = Mathf.Clamp(1f + (amount / 200f), 1f, 1.5f);
        Vector3 targetScale = new Vector3(
            Mathf.Max(fromScale.x, _normalScale.x * scaleMult),
            Mathf.Max(fromScale.y, _normalScale.y * scaleMult),
            1f);

        float t = 0f;
        while (t < punchDuration)
        {
            t += Time.deltaTime;
            float ratio = t / punchDuration;
            target.transform.localScale = Vector3.Lerp(fromScale, targetScale, ratio);
            Color c = Color.Lerp(originalColor, punchColor, ratio);
            c.a = originalColor.a;
            target.color = c;
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * dissipateSpeed;
            target.transform.localScale = Vector3.Lerp(targetScale, _normalScale, t);
            Color c = Color.Lerp(punchColor, originalColor, t);
            c.a = originalColor.a;
            target.color = c;
            yield return null;
        }

        target.transform.localScale = _normalScale;
        target.color = originalColor;
    }

    private IEnumerator LossEffect(TMP_Text target, int amount, Color originalColor, Color lossColor)
    {
        Vector3 originalPos = target.transform.localPosition;
        float scaleMult = Mathf.Clamp(1f + (amount / 100f), 1.25f, 1.5f);
        Vector3 targetScale = _normalScale * scaleMult;

        try
        {
            target.color = new Color(lossColor.r, lossColor.g, lossColor.b, originalColor.a);
            target.transform.localScale = targetScale;

            float shakeElapsed = 0f;
            float shakeDuration = 0.3f;
            float shakeMagnitude = Mathf.Clamp(amount / 20f, 5f, 7.5f);
            while (shakeElapsed < shakeDuration)
            {
                shakeElapsed += Time.deltaTime;
                float x = originalPos.x + Random.Range(-shakeMagnitude, shakeMagnitude);
                float y = originalPos.y + Random.Range(-shakeMagnitude, shakeMagnitude);
                target.transform.localPosition = new Vector3(x, y, originalPos.z);
                yield return null;
            }

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * dissipateSpeed;
                target.transform.localScale = Vector3.Lerp(targetScale, _normalScale, t);
                Color c = Color.Lerp(lossColor, originalColor, t);
                c.a = originalColor.a;
                target.color = c;
                yield return null;
            }
        }
        finally
        {
            target.transform.localScale = _normalScale;
            target.transform.localPosition = originalPos;
            target.color = originalColor;
        }
    }

    private IEnumerator FloatingLabelTicker()
    {
        while (_pendingMoneyDiff > 0)
        {
            SpawnFloatingLabel($"+${_pendingMoneyDiff:N0}");
            _pendingMoneyDiff = 0;
            yield return new WaitForSeconds(timeBetweenLabels);
        }

        _floatingLabelCoroutine = null;
    }

    private void SpawnFloatingLabel(string text)
    {
        if (floatingLabelPrefab == null || floatingLabelParent == null) return;
        TMP_Text label = Instantiate(floatingLabelPrefab, floatingLabelParent);
        label.text = text;
        StartCoroutine(FloatLabel(label));
    }

    private IEnumerator FloatLabel(TMP_Text label)
    {
        yield return null;

        float elapsed = 0f;
        Vector3 startPos = label.transform.localPosition;
        Color startColor = label.color;
        float swayFrequency = Random.Range(0.5f, 1f);
        float swayAmplitude = Random.Range(5f, 10f);
        float swayPhase = Random.Range(0f, Mathf.PI * 2f);

        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / floatDuration;
            float swayX = Mathf.Sin(elapsed * swayFrequency * Mathf.PI * 2f + swayPhase) * swayAmplitude;
            label.transform.localPosition = startPos + new Vector3(swayX, floatSpeed * t, 0f);
            label.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            yield return null;
        }

        Destroy(label.gameObject);
    }

    private void UpdateDoneButton(bool placed)
    {
        doneButton.interactable = placed && RoundManager.Instance.state == RoundState.Playing;
    }

    private void UpdateState(RoundState state)
    {
        if (state == RoundState.Playing)
            _lastBeepTime = RoundManager.Instance.roundTime;

        doneButton.interactable = RoundManager.Instance.shapePlacedThisRound && state == RoundState.Playing;
        if (state == RoundState.GameOver && gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverScoreText.text = $"${GridManager.Instance.CurrentMoney:N0}";
        }
    }

    public void OnPlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}