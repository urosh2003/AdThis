using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private Image timerImage;
    [SerializeField] private TMP_Text viewersText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private Button doneButton;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverScoreText;

    void OnEnable()
    {
        RoundManager.OnTimerChanged += UpdateTimer;
        RoundManager.OnStateChanged += UpdateState;
        RoundManager.OnShapePlacedChanged += UpdateDoneButton;
        GridManager.OnViewersChanged += UpdateViewers;
        GridManager.OnMoneyChanged += UpdateMoney;
    }

    void OnDisable()
    {
        RoundManager.OnTimerChanged -= UpdateTimer;
        RoundManager.OnStateChanged -= UpdateState;
        RoundManager.OnShapePlacedChanged -= UpdateDoneButton;
        GridManager.OnViewersChanged -= UpdateViewers;
        GridManager.OnMoneyChanged -= UpdateMoney;
    }

    private float _lastBeepTime = -1f;

    private void UpdateTimer(float time)
    {
        timerImage.fillAmount = time / RoundManager.Instance.roundTime;

        // Alarm sound similar to a bomb: shorter period as time decreases
        float timeRemainingRatio = time / RoundManager.Instance.roundTime;
        // Interval decreases from 1.5s (at start) to 0.05s (at finish)
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
    }

    private void UpdateMoney(int money)
    {
        moneyText.text = $"Money: ${money:N0}";
        if (gameOverPanel != null && gameOverPanel.activeSelf)
            gameOverScoreText.text = $"${money:N0}";
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
