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
        private set { _timeRemaining = value; OnTimerChanged?.Invoke(value); }
    }

    private bool _shapePlacedThisRound;
    public bool shapePlacedThisRound
    {
        get => _shapePlacedThisRound;
        set { _shapePlacedThisRound = value; OnShapePlacedChanged?.Invoke(value); }
    }

    [SerializeField] private float betweenRoundDuration = 1f;
    [SerializeField] private int viewerPenaltyForNotPlacing = 10;

    void Awake()
    {
        Instance = this;
        roundNumber = 0;
        timeRemaining = roundTime;
        shapePlacedThisRound = false;
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
