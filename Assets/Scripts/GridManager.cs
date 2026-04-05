using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridCell
{
    public TileType TileType; // Normal, Bonus, Forbidden
    public TileShape OccupiedBy; // reference to placed shape piece
    
    public bool IsOccupied => OccupiedBy != null;
    public bool IsEmpty => !IsOccupied;
    
    public bool IsBonus => TileType == TileType.Bonus;
    public bool IsForbidden => TileType == TileType.Forbidden;

    public GridCell(TileType type = TileType.Normal, TileShape occupiedBy = null)
    {
        TileType = type;
        OccupiedBy = occupiedBy;
    }
}

public enum TileType
{
    Normal,
    Bonus,
    Forbidden
}

public class GridManager : MonoBehaviour
{
    [SerializeField] public int width;
    [SerializeField] public int height;
    [SerializeField] private Sprite greenSprite;
    [SerializeField] private Sprite redSprite;
    public GridCell[,] Grid;
    public static GridManager Instance;
    [SerializeField] private int cellSize = 1;

    public static event Action<int> OnViewersChanged;
    public static event Action<int> OnMoneyChanged;
    private TilemapRenderer outlineGrid;

    private int _currentMoney;
    private int _currentViewers;

    public int moneyPerAd = 0;
    public float moneyPerViewerPerCell = 0.1f;
    public float moneyPerAdPerCell = 0;

    // Power-up modifier fields
    public bool facecamGivesMoney = false;
    public float facecamMoneyPerCell = 500f;
    public bool preventViewerLoss = false;
    public float moneyMultiplier = 1f;
    public float viewerMultiplier = 1f;

    [HideInInspector] public int lastTotalRoundMoney;


    public int CurrentMoney
    {
        get => _currentMoney;
        set { _currentMoney = value; OnMoneyChanged?.Invoke(value); }
    }

    public int CurrentViewers
    {
        get => _currentViewers;
        set { _currentViewers = value; OnViewersChanged?.Invoke(value); }
    }

    [SerializeField] private int startingViewers = 100;
    public int StartingViewers => startingViewers;
    [SerializeField] private int passiveViewersPerRound = 5;
    [SerializeField] public int viewerLossPerForbiddenCell = 5;
    [SerializeField] public int viewersPerFacecamCell = 3;
    [SerializeField] public float bonusMultiplier = 2;

    [Header("Zone Visuals")]
    [SerializeField] private float zonesOppacity = 1f;
    [SerializeField] private Color bonusLabelColor = Color.green;
    [SerializeField] private Color forbiddenLabelColor = Color.red;
    [SerializeField] private TMP_FontAsset zoneLabelFont;

    private List<GameObject> zoneOverlays = new List<GameObject>();
    private Dictionary<Vector2Int, GameObject> zoneLabelsByCell = new Dictionary<Vector2Int, GameObject>();
    
    [Header("Particle System Prefabs")]
    [SerializeField] private ParticleSystem normalParticleSystem;
    [SerializeField] private ParticleSystem bonusParticleSystem;
    [SerializeField] private ParticleSystem forbiddenParticleSystem;
    [SerializeField] private float particleSystemOffset = 0.05f;

    [SerializeField] private StreamImageManager streamImageManager;

    private List<ParticleSystem> activeParticleSystems = new List<ParticleSystem>();
    
    private int lastZone = 0;

    private void Awake()
    {
        Instance = this;
        outlineGrid = GetComponent<TilemapRenderer>();
    }

    private void Start()
    {
        CurrentViewers = startingViewers;
        CurrentMoney = 0;

        Grid = new GridCell[width, height];

        for (var i = 0; i < height; i++)
        {
            for (var j = 0; j < width; j++)
            {
                Grid[j, i] = new GridCell();
            }
        }
    }
    
    // Convert world position → grid coordinate
    public Vector2Int WorldToGrid(Vector3 worldPos) {
        int x = Mathf.FloorToInt((worldPos.x - transform.position.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.y - transform.position.y) / cellSize);
        return new Vector2Int(x, y);
    }

    // Convert grid coordinate → world position (cell center)
    public Vector3 GridToWorld(Vector2Int coord) {
        return new Vector3(
            coord.x * cellSize + cellSize * 0.5f + transform.position.x,
            coord.y * cellSize + cellSize * 0.5f + transform.position.y,
            0f
        );
    }

    public bool IsInBounds(Vector2Int coord) =>
        coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height;

    public Vector2Int? FindFirstLegalPosition(TileShape shape)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var coord = new Vector2Int(x, y);
                if (IsPositionLegal(coord, shape))
                    return coord;
            }
        }
        return null;
    }

    public GridCell GetCell(Vector2Int coord) => Grid[coord.x, coord.y];

    public bool IsCellFree(Vector2Int coord) =>
        IsInBounds(coord) && Grid[coord.x, coord.y].IsEmpty;
    
    public bool IsPositionLegal(Vector2Int coord, TileShape shape)
    {
        foreach (var cellOffset in shape.cells)
        {
            if (!IsCellFree(coord + cellOffset))
                return false;
        }
        return true;
    }

    public void PlaceShape(TileShape shape, Vector2Int coord)
    {
        if (!IsPositionLegal(coord, shape))
            return;

        foreach (var cellOffset in shape.cells)
        {
            var pos = new Vector2Int(coord.x + cellOffset.x, coord.y + cellOffset.y);
            Grid[pos.x, pos.y].OccupiedBy = shape;
            if (zoneLabelsByCell.TryGetValue(pos, out var label))
                label.SetActive(true);
        }
    }

    public void RemoveShape(TileShape shape, Vector2Int coord)
    {
        foreach (var cellOffset in shape.cells)
        {
            Vector2Int pos = coord + cellOffset;
            if (IsInBounds(pos) && Grid[pos.x, pos.y].OccupiedBy == shape)
            {
                Grid[pos.x, pos.y].OccupiedBy = null;
                if (zoneLabelsByCell.TryGetValue(pos, out var label))
                    label.SetActive(false);
            }
        }
    }

    public void GenerateZones()
    {
        for (int x = 0; x <= 12; x++)
        {
            for (int y = 0; y <= 9; y++)
            {
                Grid[x, y].TileType = TileType.Normal;
            }
        }
        outlineGrid.enabled = true;
        
        var nextZone = UnityEngine.Random.Range(1, 11);
        while (nextZone == lastZone)
            nextZone = UnityEngine.Random.Range(1, 11);

        Debug.Log($"Generating zone {nextZone}");
        if (streamImageManager != null)
        {
            streamImageManager.SetStream(nextZone);
        }
        
        switch (nextZone)
        {
            case 1:
                for (int x = 0; x <= 2; x++)
                {
                    for (int y = 0; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Forbidden;
                    }
                }
                for (int x = 3; x <= 9; x++)
                {
                    for (int y = 2; y <= 7; y++)
                    {
                        Grid[x, y].TileType = TileType.Forbidden;
                    }
                }
                for (int x = 10; x <= 12; x++)
                {
                    for (int y = 0; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                break;
            case 2:
                for (int x = 0; x <= 2; x++)
                {
                    for (int y = 0; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                for (int x = 3; x <= 9; x++)
                {
                    for (int y = 2; y <= 7; y++)
                    {
                        Grid[x, y].TileType = TileType.Forbidden;
                    }
                }
                for (int x = 3; x <= 10; x++)
                {
                    for (int y = 0; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Forbidden;
                    }
                }
                break;
            case 3:
                for (int x = 0; x <= 12; x++)
                {
                    for (int y = 3; y <= 6; y++)
                    {
                        Grid[x, y].TileType = TileType.Forbidden;
                    }
                }
                for (int x = 5; x <= 7; x++)
                {
                    for (int y = 0; y <= 2; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                for (int x = 5; x <= 7; x++)
                {
                    for (int y = 7; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                break;
            case 4:
                for (int x = 0; x <= 8; x++)
                {
                    for (int y = 0; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Forbidden;
                    }
                }
                for (int x = 0; x <= 2; x++)
                {
                    for (int y = 2; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                break;
            case 5:
                for (int x = 0; x <= 8; x++)
                {
                    for (int y = 0; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Forbidden;
                    }
                }
                for (int x = 0; x <= 2; x++)
                {
                    for (int y = 0; y <= 7; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                break;
            case 6:
                for (int x = 4; x <= 12; x++)
                {
                    for (int y = 0; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Forbidden;
                    }
                }
                for (int x = 10; x <= 12; x++)
                {
                    for (int y = 2; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                break;
            case 7:
                for (int x = 4; x <= 12; x++)
                {
                    for (int y = 0; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Forbidden;
                    }
                }
                for (int x = 10; x <= 12; x++)
                {
                    for (int y = 0; y <= 7; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                break;
            case 8:
                for (int x = 3; x <= 9; x++)
                {
                    for (int y = 3; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Forbidden;
                    }
                }
                for (int x = 0; x <= 12; x++)
                {
                    for (int y = 0; y <= 2; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                break;
            case 9:
                for (int x = 3; x <= 9; x++)
                {
                    for (int y = 0; y <= 7; y++)
                    {
                        Grid[x, y].TileType = TileType.Forbidden;
                    }
                }
                for (int x = 0; x <= 12; x++)
                {
                    for (int y = 7; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                break;
            case 10:
                for (int x = 0; x <= 2; x++)
                {
                    for (int y = 0; y <= 2; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                for (int x = 0; x <= 2; x++)
                {
                    for (int y = 7; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                for (int x = 10; x <= 12; x++)
                {
                    for (int y = 0; y <= 2; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                for (int x = 10; x <= 12; x++)
                {
                    for (int y = 7; y <= 9; y++)
                    {
                        Grid[x, y].TileType = TileType.Bonus;
                    }
                }
                for (int x = 3; x <= 9; x++)
                {
                    for (int y = 2; y <= 7; y++)
                    {
                        Grid[x, y].TileType = TileType.Forbidden;
                    }
                }
                Grid[2, 4].TileType = TileType.Forbidden;
                Grid[2, 5].TileType = TileType.Forbidden;
                Grid[10, 4].TileType = TileType.Forbidden;
                Grid[10, 5].TileType = TileType.Forbidden;
                break;
        }
        lastZone = nextZone;
        PowerUpManager.instance.ApplyPowerUps(PowerUpType.AfterZoneSetup);
        UpdateZoneVisuals();
    }

    private void UpdateZoneVisuals()
    {
        foreach (var overlay in zoneOverlays)
            Destroy(overlay);
        zoneOverlays.Clear();
        zoneLabelsByCell.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = Grid[x, y];
                if (cell.TileType == TileType.Normal) continue;

                var coord = new Vector2Int(x, y);
                var overlay = new GameObject($"ZoneLabel_{x}_{y}");
                Vector3 cellCenter = GridToWorld(coord);
                overlay.transform.position = cellCenter;
                overlay.transform.SetParent(transform);
                var spriteRenderer = overlay.AddComponent<SpriteRenderer>();
                
                if (cell.TileType == TileType.Bonus)
                {
                    spriteRenderer.sprite = greenSprite;
                }
                else if (cell.TileType == TileType.Forbidden)
                {
                    spriteRenderer.sprite = redSprite;
                }

                spriteRenderer.sortingOrder = 999;
                spriteRenderer.color = new Color(1, 1, 1, zonesOppacity);

                // Add text label describing cell effect (hidden until an ad covers it)
                var textObj = new GameObject("Label");
                textObj.transform.SetParent(overlay.transform);
                textObj.transform.localPosition = new Vector3(0, 0, -0.01f);
                var tmp = textObj.AddComponent<TextMeshPro>();
                tmp.text = cell.TileType == TileType.Bonus ? "x2" : "-" + viewerLossPerForbiddenCell;
                tmp.color = cell.TileType == TileType.Bonus ? bonusLabelColor : forbiddenLabelColor;
                tmp.fontSize = 4f;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.sortingOrder = 1000;
                if (zoneLabelFont != null) tmp.font = zoneLabelFont;
                tmp.rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
                textObj.SetActive(cell.IsOccupied);
                zoneLabelsByCell[coord] = textObj;

                zoneOverlays.Add(overlay);
            }
        }
    }

    private void CreateRandomZone(TileType zoneType)
    {
        // Randomly choose zone type: 0 = rows, 1 = columns, 2 = rectangle
        int zoneShape = UnityEngine.Random.Range(0, 3);
        
        if (zoneShape == 0) // Rows
        {
            int startRow = UnityEngine.Random.Range(0, height);
            int endRow = UnityEngine.Random.Range(startRow, height);
            for (int y = startRow; y <= endRow; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Grid[x, y].TileType = zoneType;
                }
            }
        }
        else if (zoneShape == 1) // Columns
        {
            int startCol = UnityEngine.Random.Range(0, width);
            int endCol = UnityEngine.Random.Range(startCol, width);
            for (int x = startCol; x <= endCol; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Grid[x, y].TileType = zoneType;
                }
            }
        }
        else // Rectangle
        {
            int x1 = UnityEngine.Random.Range(0, width);
            int x2 = UnityEngine.Random.Range(x1, width);
            int y1 = UnityEngine.Random.Range(0, height);
            int y2 = UnityEngine.Random.Range(y1, height);
            for (int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                {
                    Grid[x, y].TileType = zoneType;
                }
            }
        }
    }

    public void AddPassiveViewers()
    {
        if (CurrentViewers <= 0) return; // EZ Fix :D
        CurrentViewers += passiveViewersPerRound;
        Debug.Log($"Gained {passiveViewersPerRound} passive viewers. Total: {CurrentViewers}");
    }

    public void CalculateRoundEnd()
    {
        // updateParticleSystems();

        int totalMoney = 0;
        int totalViewerLoss = 0;
        int totalViewerGain = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridCell cell = Grid[x, y];

                if (!cell.IsOccupied) continue;

                // Facecam logic: pointsPerCell == 0 means it gives viewers, not money
                if (cell.OccupiedBy.pointsPerCell == 0)
                {
                    totalViewerGain += viewersPerFacecamCell;
                }
                else
                {
                    // Normal ad: gives money
                    int cellMoney = cell.OccupiedBy.pointsPerCell;
                    if (cell.IsBonus)
                        cellMoney *= (int) bonusMultiplier;
                    totalMoney += cellMoney;
                }

                // Forbidden zone penalty applies to all shapes
                if (cell.IsForbidden)
                {
                    totalViewerLoss += viewerLossPerForbiddenCell;
                }
            }
        }

        CurrentMoney += totalMoney;
        CurrentViewers = Mathf.Max(0, CurrentViewers + totalViewerGain - totalViewerLoss);

        Debug.Log("=== ROUND END ===");
        Debug.Log($"Money earned: +{totalMoney} (Total: {CurrentMoney})");
        Debug.Log($"Viewers gained: +{totalViewerGain}");
        Debug.Log($"Viewers lost: -{totalViewerLoss}");
        Debug.Log($"Current Viewers: {CurrentViewers}");

        if (CurrentViewers <= 0)
        {
            Debug.Log("GAME OVER! You lost all your viewers!");
        }
    }

    public void resolveScoring()
    {
        PowerUpManager.instance.ApplyPowerUps(PowerUpType.DuringScoring);
        outlineGrid.enabled = false;
        foreach (var overlay in zoneOverlays)
        {
            overlay.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, zonesOppacity * 0.5f);
        }
        // Clear existing particle systems
        foreach (var ps in activeParticleSystems)
        {
            if (ps != null)
                Destroy(ps.gameObject);
        }
        activeParticleSystems.Clear();

        // Create new particle systems based on current grid state
        float timeOffset = 0f;

        // Count distinct ads/shapes per cell type (bonus, normal, forbidden)
        var shapesPerType = new HashSet<TileShape>[3] { new(), new(), new() };
        for (int row = 0; row < height; row++)
            for (int col = 0; col < width; col++)
            {
                GridCell c = Grid[col, height - 1 - row];
                if (!c.IsOccupied) continue;
                if (c.IsBonus) shapesPerType[0].Add(c.OccupiedBy);
                else if (c.IsForbidden) shapesPerType[2].Add(c.OccupiedBy);
                else shapesPerType[1].Add(c.OccupiedBy);
            }

        var sentiments = new[] { ChatSentiment.Positive, ChatSentiment.Neutral, ChatSentiment.Negative };
        var totalRoundMoney = 0;
        var totalJimmysCut = PowerUpManager.instance.GetTotalJimmysCut();
        for (int iter = 0; iter < 3; iter++)
        {
            for (int i = 0; i < shapesPerType[iter].Count; i++)
                ChatCommentManager.Instance.ShowComment(sentiments[iter], timeOffset);

            var pitch = 1f;
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    GridCell cell = Grid[col, height - 1 - row];
                    if (!cell.IsOccupied) continue;
                    ParticleSystem psPrefab = normalParticleSystem;
                    int moneyChange = 0;
                    int viewerChange = 0;
                    double rotationMultiplier = 1;
                    switch (cell.OccupiedBy.rotationStep % 4)
                    {
                        case 1: // 90 degrees
                            rotationMultiplier = 0.75;
                            break;
                        case 2: // 180 degrees
                            rotationMultiplier = 0.5;
                            break;
                        case 3: // 270 degrees
                            rotationMultiplier = 0.75;
                            break;
                        default: // 0 degrees
                            rotationMultiplier = 1;
                            break;
                    }

                    // Facecam logic: pointsPerCell == 0 means it gives viewers, not money
                    if (iter == 1)
                        if (cell.OccupiedBy.pointsPerCell == 0)
                        {
                            viewerChange = viewersPerFacecamCell;
                            if (facecamGivesMoney)
                                moneyChange = (int)facecamMoneyPerCell;
                        }
                        else
                            moneyChange = (int)(_currentViewers * moneyPerViewerPerCell + moneyPerAdPerCell);

                if (cell.IsBonus)
                    {
                        psPrefab = bonusParticleSystem;
                        if(cell.OccupiedBy.pointsPerCell == 0)
                        {
                            viewerChange = (int)(viewersPerFacecamCell * bonusMultiplier);
                            if (facecamGivesMoney)
                                moneyChange = (int)(facecamMoneyPerCell * bonusMultiplier);
                        }
                        else
                            moneyChange = (int)((_currentViewers * moneyPerViewerPerCell + moneyPerAdPerCell) * bonusMultiplier);

                        if (iter != 0)
                            continue;
                        pitch += 0.1f;
                        pitch = Mathf.Min(pitch, 2f);
                    }
                    else if (cell.IsForbidden)
                    {
                        psPrefab = forbiddenParticleSystem;
                        viewerChange = preventViewerLoss ? 0 : -viewerLossPerForbiddenCell;
                        moneyChange = cell.OccupiedBy.pointsPerCell; // still give money for forbidden placements
                        if (iter != 2)
                            continue;
                        pitch -= 0.1f;
                        pitch = Mathf.Max(pitch, 0.5f);
                    }
                    else if (iter == 1)
                    {
                        pitch += 0.05f;
                        pitch = Mathf.Min(pitch, 2f);
                    }
                    else continue;
                    Vector3 spawnPos = GridToWorld(new Vector2Int(col, height - 1 - row));
                    var psInstance = Instantiate(psPrefab, spawnPos, Quaternion.identity);
                    activeParticleSystems.Add(psInstance);
                    StartCoroutine(particleSystemPlayback(psInstance, timeOffset, pitch));
                    moneyChange = (int)(moneyChange * rotationMultiplier * moneyMultiplier);
                    if (viewerChange > 0)
                        viewerChange = (int)(viewerChange * viewerMultiplier);
                    totalRoundMoney += moneyChange;
                    if (moneyChange != 0)
                        StartCoroutine(updateCurrentMoney(moneyChange, timeOffset));
                    if (viewerChange != 0)
                        StartCoroutine(updateCurrentViewers(viewerChange, timeOffset));
                    timeOffset += particleSystemOffset; // Stagger particle system start times
                }
            }

            timeOffset += 0.5f;
        }
        // Early finish bonus: 2% per second saved
        float secondsSaved = RoundManager.Instance.secondsSavedThisRound;
        if (secondsSaved > 0 && totalRoundMoney > 0)
        {
            int earlyBonus = (int)(0.02f * secondsSaved * totalRoundMoney);
            if (earlyBonus > 0)
                StartCoroutine(updateCurrentMoney(earlyBonus, timeOffset + 0.3f));
            timeOffset += 0.3f;
            totalRoundMoney += earlyBonus;
        }
        
        StartCoroutine(updateCurrentMoney(-(int)(totalJimmysCut*totalRoundMoney), timeOffset));

        

        lastTotalRoundMoney = totalRoundMoney;
        StartCoroutine(WaitForParticles(timeOffset+0.5f));
    }
    
    private IEnumerator WaitForParticles(float timeOffset)
    {
        yield return new WaitForSeconds(timeOffset);
        if (RoundManager.Instance.state == RoundState.GameOver || CurrentViewers <= 0)
        {
            if (RoundManager.Instance.state != RoundState.GameOver)
                RoundManager.Instance.TriggerGameOver();
            yield break;
        }

        // Drain remaining timer after scoring, then continue
        if (RoundManager.Instance.timeRemaining > 0)
        {
            bool drained = false;
            RoundManager.Instance.DrainTimerThen(() => drained = true);
            yield return new WaitUntil(() => drained);
        }

        PowerUpManager.instance.TickRound();
        PowerUpManager.instance.TryOfferPowerUpSelection(() =>
        {
            RoundManager.Instance.StartNewRound();
        });
    }

    private IEnumerator particleSystemPlayback(ParticleSystem ps, float delay, float pitch)
    {
        yield return new WaitForSeconds(delay);
        ps.Play();
        var audioSource = ps.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.pitch = pitch;
            audioSource.Play();
        }
    }

    private IEnumerator updateCurrentMoney(int amount, float delay)
    {
        yield return new WaitForSeconds(delay);
        CurrentMoney += amount;
    }

    private IEnumerator updateCurrentViewers(int amount, float delay)
    {
        yield return new WaitForSeconds(delay);
        CurrentViewers = Mathf.Max(0, CurrentViewers + amount);
    }
}