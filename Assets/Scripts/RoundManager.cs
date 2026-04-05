using System;
using System.Collections;
using UnityEngine;

public enum RoundState { Playing, BetweenRounds, GameOver }

public class RoundManager : MonoBehaviour
{
    public static event Action<float> OnTimerChanged;
    public static event Action<RoundState> OnStateChanged;
    public static event Action<bool> OnShapePlacedChanged;

    public static RoundManager Instance;
    public int roundNumber;
    public float roundTime = 15;
    public RoundState state = RoundState.Playing;

    private float _timeRemaining;
    public float timeRemaining
    {
        get => _timeRemaining;
        set { _timeRemaining = value; OnTimerChanged?.Invoke(value); }
    }

    private bool _shapePlacedThisRound;
    public bool shapePlacedThisRound
    {
        get => _shapePlacedThisRound;
        set { _shapePlacedThisRound = value; OnShapePlacedChanged?.Invoke(value); }
    }

    [SerializeField] private float betweenRoundDuration = 1f;
    [SerializeField] private int viewerPenaltyForNotPlacing = 10;

    [HideInInspector] public float secondsSavedThisRound;

    void Awake()
    {
        Instance = this;
        roundNumber = 0;
        timeRemaining = roundTime;
        shapePlacedThisRound = false;
        
        // Ensure cursor is visible since we removed CursorManager which was hiding it
        Cursor.visible = true;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void Start()
    {
        ShapeFactory.Instance.SpawnFacecam();
        StartNewRound();
    }

    void Update()
    {
        if (state != RoundState.Playing) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            EndRound();
        }
    }

    public void EndRound()
    {
        if (state != RoundState.Playing) return;

        state = RoundState.BetweenRounds;
        OnStateChanged?.Invoke(state);

        bool playerPlacedBeforeTimerExpired = shapePlacedThisRound;
        secondsSavedThisRound = playerPlacedBeforeTimerExpired ? timeRemaining : 0f;

        // Force drop if player is mid-drag, then auto-place if still not on board
        var allShapes = FindObjectsOfType<ShapeInstance>();
        foreach (var shape in allShapes)
        {
            shape.ForceDrop();
            if (shape == ShapeFactory.Instance.currentShape && !shapePlacedThisRound)
                shape.AutoPlace();
        }

        // Penalize only if the player didn't place the ad themselves before time ran out
        if (!playerPlacedBeforeTimerExpired)
        {
            GridManager.Instance.CurrentViewers -= viewerPenaltyForNotPlacing;
            GridManager.Instance.CurrentViewers = Mathf.Max(0, GridManager.Instance.CurrentViewers);
            Debug.Log($"Ad not placed in time! Lost {viewerPenaltyForNotPlacing} viewers.");
        }

        GridManager.Instance.HideZoneLabels();
        StartCoroutine(BetweenRoundsDelayThenScore());

        if (GridManager.Instance.CurrentViewers <= 0)
        {
            state = RoundState.GameOver;
            OnStateChanged?.Invoke(state);
            Debug.Log("GAME OVER!");
            return;
        }
    }

    public async void TriggerGameOver()
    {
        state = RoundState.GameOver;
        await LeaderboardManager.Instance.GameOver(GridManager.Instance.CurrentMoney);
        OnStateChanged?.Invoke(state);
    }

    // Called by Done button
    public void OnDoneButtonPressed()
    {
        if (state != RoundState.Playing) return;
        if (!shapePlacedThisRound) return;
        EndRound();
    }

    [Header("Timer Drain")]
    [SerializeField] private float drainDuration = 0.4f;
    [SerializeField] private AnimationCurve drainCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Serialized so you can wire up a sound later:
    // [SerializeField] private AudioSource drainAudioSource;
    // [SerializeField] private AudioClip drainClip;

    public bool isDraining { get; private set; } = false;

    private Coroutine _drainCoroutine;

    /// <summary>
    /// Smoothly drains the timer to 0, then calls onComplete.
    /// To add sound later: play drainAudioSource.PlayOneShot(drainClip) at the start of the coroutine.
    /// </summary>
    public void DrainTimerThen(Action onComplete)
    {
        if (_drainCoroutine != null)
            StopCoroutine(_drainCoroutine);
        _drainCoroutine = StartCoroutine(DrainTimerCoroutine(onComplete));
    }

    private IEnumerator DrainTimerCoroutine(Action onComplete)
    {
        isDraining = true;
        float startTime = timeRemaining;
        float elapsed = 0f;

        // --- Add sound here later ---
        // if (drainAudioSource != null && drainClip != null)
        //     drainAudioSource.PlayOneShot(drainClip);

        while (elapsed < drainDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / drainDuration);
            float curved = drainCurve.Evaluate(t);
            timeRemaining = Mathf.Lerp(startTime, 0f, curved);
            yield return null;
        }

        timeRemaining = 0f;
        isDraining = false;
        _drainCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator BetweenRoundsDelay()
    {
        yield return new WaitForSeconds(betweenRoundDuration);
        StartNewRound();
    }
    private IEnumerator BetweenRoundsDelayThenScore()
    {
        yield return new WaitForSeconds(0.5f);
        GridManager.Instance.resolveScoring();
    }

    public void StartNewRound()
    {
        ViewerDealManager.Instance?.TurnBackgroundLight();
        roundNumber += 1;
        timeRemaining = roundTime;
        shapePlacedThisRound = false;
        GridManager.Instance.GenerateZones();
        bool poolEmpty = !ShapeFactory.Instance.HasShapes;
        ShapeFactory.Instance.SpawnShape();
        // If pool was exhausted and there's no shape to place, skip penalty and enable done button
        if (poolEmpty)
            shapePlacedThisRound = true;
        state = RoundState.Playing;
        OnStateChanged?.Invoke(state);
    }
}
