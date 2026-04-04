using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShapeInstance : MonoBehaviour
{
    [SerializeField] public TileShape shapeData;
    [SerializeField] private Color legalColor = Color.green;
    [SerializeField] private Color illegalColor = Color.red;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private float spriteSwapTime = 0.5f;
    [SerializeField] private AudioClip pickUpSound;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip rotateSound;

    private AudioSource _audioSource;
    public bool isPlaced = false;
    public float timeElapsed;
    public int currentSprite = 1;
    
    private List<GameObject> tileVisuals = new List<GameObject>();
    private Vector3 dragOffset;
    private bool isDragging = false;
    private Vector3 originalPosition;
    private Vector2Int? currentGridPos = null; // Track where it's currently placed
    private Camera mainCamera;
    private Vector2Int lastLegalGridCoord;

    // Input actions
    private InputAction leftClickAction;
    private InputAction rightClickAction;
    private InputAction pointerPositionAction;

    // ─────────────────────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        SetupInputActions();
    }

    private void Start()
    {
        spriteSwapTime = shapeData.spriteSwapTime;
        mainCamera = Camera.main;
        originalPosition = transform.position;
        InitializeVisuals();
        timeElapsed = 0;
        currentSprite = 1;
        shapeData.rotationStep = 0;
    }

    private void OnEnable()
    {
        leftClickAction.Enable();
        rightClickAction.Enable();
        pointerPositionAction.Enable();

        leftClickAction.started  += OnLeftClickStarted;
        leftClickAction.canceled += OnLeftClickCanceled;
        rightClickAction.started += OnRightClickStarted;
    }

    private void OnDisable()
    {
        leftClickAction.started  -= OnLeftClickStarted;
        leftClickAction.canceled -= OnLeftClickCanceled;
        rightClickAction.started -= OnRightClickStarted;

        leftClickAction.Disable();
        rightClickAction.Disable();
        pointerPositionAction.Disable();
    }

    private void OnDestroy()
    {
        leftClickAction.Dispose();
        rightClickAction.Dispose();
        pointerPositionAction.Dispose();
    }

    private void Update()
    {
        // Sprite animation
        timeElapsed += Time.deltaTime;
        if (timeElapsed > spriteSwapTime)
        {
            timeElapsed = 0;
            currentSprite = currentSprite == 1 ? 2 : 1;
            var childrenRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < childrenRenderers.Length; i++)
            {
                if (currentSprite == 1)
                    childrenRenderers[i].sprite = shapeData.cellSprites[i];
                else if (currentSprite == 2 && shapeData.cellSpritesSecond.Length != 0)
                    childrenRenderers[i].sprite = shapeData.cellSpritesSecond[i];
            }
        }

        // Drive drag each frame while held
        if (isDragging)
            HandleDrag();
    }

    // ─────────────────────────────────────────────────────────────
    // Input setup
    // ─────────────────────────────────────────────────────────────

    private void SetupInputActions()
    {
        leftClickAction = new InputAction(
            "LeftClick",
            InputActionType.Button,
            binding: "<Mouse>/leftButton");

        rightClickAction = new InputAction(
            "RightClick",
            InputActionType.Button,
            binding: "<Mouse>/rightButton");

        pointerPositionAction = new InputAction(
            "PointerPosition",
            InputActionType.Value,
            binding: "<Mouse>/position");
    }

    // ─────────────────────────────────────────────────────────────
    // Input callbacks
    // ─────────────────────────────────────────────────────────────

    private void OnLeftClickStarted(InputAction.CallbackContext ctx)
    {
        if (!IsInteractionAllowed()) return;
        if (!IsPointerOverThisObject())    return;

        isDragging = true;
        dragOffset = transform.position - GetPointerWorldPos();
        SetVisualsSortingOrder(10);
        PlayPickupSound();

        if (currentGridPos.HasValue)
        {
            GridManager.Instance.RemoveShape(shapeData, currentGridPos.Value);
            lastLegalGridCoord = currentGridPos.Value;
            currentGridPos = null;
        }
        else
        {
            lastLegalGridCoord = new Vector2Int(-1, -1);
        }
    }

    private void OnLeftClickCanceled(InputAction.CallbackContext ctx)
    {
        if (!isDragging) return;
        isDragging = false;
        SetVisualsSortingOrder(1);

        if (SettingsManager.Instance.IsSettingsOpen()) return;

        Vector2Int gridCoord = GridManager.Instance.WorldToGrid(transform.position);

        if (GridManager.Instance.IsPositionLegal(gridCoord, shapeData))
        {
            PlaceAt(gridCoord);
        }
        else
        {
            transform.position = originalPosition;
            gridCoord = GridManager.Instance.WorldToGrid(originalPosition);

            if (GridManager.Instance.IsInBounds(gridCoord) &&
                GridManager.Instance.IsPositionLegal(gridCoord, shapeData))
            {
                GridManager.Instance.PlaceShape(shapeData, gridCoord);
                currentGridPos = gridCoord;
            }

            SetVisualsColor(defaultColor);
        }

        PlayPlaceSound();
    }

    private void OnRightClickStarted(InputAction.CallbackContext ctx)
    {
        if (!IsInteractionAllowed()) return;

        // Only rotate when hovering over or actively dragging this shape
        if (!isDragging && !IsPointerOverThisObject()) return;

        RotateShape();
    }

    // ─────────────────────────────────────────────────────────────
    // Drag
    // ─────────────────────────────────────────────────────────────

    private void HandleDrag()
    {
        if (!IsInteractionAllowed())
        {
            // State changed mid-drag; force-drop
            ForceDrop();
            return;
        }

        Vector3 newPos = GetPointerWorldPos() + dragOffset;
        transform.position = newPos;
        UpdateGhostVisuals();
    }

    // ─────────────────────────────────────────────────────────────
    // Rotation
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Rotates the shape 90° clockwise.
    /// shapeData.rotationStep: 0 = 0°, 1 = 90°, 2 = 180°, 3 = 270°.
    /// </summary>
    public void RotateShape()
    {
        Vector2Int? previousGridPos = currentGridPos;
        if (currentGridPos.HasValue)
        {
            GridManager.Instance.RemoveShape(shapeData, currentGridPos.Value);
            currentGridPos = null;
            isPlaced = false;
        }

        // Capture bounding box BEFORE rotation
        Vector2Int preMin = GetCellsBoundsMin(shapeData.cells);

        shapeData.rotationStep = (shapeData.rotationStep + 1) % 4;
        ApplyRotationToShapeData();
        RebuildColliders();

        if (rotateSound != null)
            _audioSource.PlayOneShot(rotateSound);

        if (previousGridPos.HasValue)
        {
            // Align post-rotation bounding box top-left to pre-rotation top-left
            Vector2Int postMin = GetCellsBoundsMin(shapeData.cells);
            Vector2Int alignedOrigin = previousGridPos.Value + (preMin - postMin);

            Vector2Int? bestPos = FindClosestLegalPosition(alignedOrigin);
            if (bestPos.HasValue)
                PlaceAt(bestPos.Value, playSound: false);
            else
                UpdateGhostVisuals();
        }
        else
        {
            UpdateGhostVisuals();
        }
    }

    /// <summary>
    /// Returns the minimum (top-left) corner of the bounding box of the given cells.
    /// </summary>
    private Vector2Int GetCellsBoundsMin(Vector2Int[] cells)
    {
        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (var c in cells)
        {
            if (c.x < minX) minX = c.x;
            if (c.y < minY) minY = c.y;
        }
        return new Vector2Int(minX, minY);
    }

    /// <summary>
    /// Searches in expanding rings around <paramref name="origin"/> for the nearest
    /// legal grid position, up to <paramref name="maxRadius"/> cells away.
    /// </summary>
    private Vector2Int? FindClosestLegalPosition(Vector2Int origin, int maxRadius = 5)
    {
        // Check origin first
        if (GridManager.Instance.IsPositionLegal(origin, shapeData))
            return origin;

        // Expand outward ring by ring
        for (int radius = 1; radius <= maxRadius; radius++)
        {
            Vector2Int? best = null;
            float bestDist = float.MaxValue;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    // Only check the cells on the edge of this ring
                    if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
                        continue;

                    Vector2Int candidate = new Vector2Int(origin.x + dx, origin.y + dy);
                    if (!GridManager.Instance.IsInBounds(candidate)) continue;
                    if (!GridManager.Instance.IsPositionLegal(candidate, shapeData)) continue;

                    float dist = ((Vector2)(candidate - origin)).sqrMagnitude;
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = candidate;
                    }
                }
            }

            if (best.HasValue)
                return best;
        }

        return null;
    }

    /// <summary>
    /// Rotates each cell offset 90° clockwise: (x, y) → (y, -x).
    /// </summary>
    private void ApplyRotationToShapeData()
    {
        for (int i = 0; i < shapeData.cells.Length; i++)
        {
            Vector2Int c = shapeData.cells[i];
            // 90° clockwise: (x, y) → (y, -x)
            shapeData.cells[i] = new Vector2Int(c.y, -c.x);
        }

        // Sync visual child positions
        for (int i = 0; i < tileVisuals.Count; i++)
        {
            var offset = shapeData.cells[i];
            tileVisuals[i].transform.localPosition = new Vector3(offset.x, offset.y, 0f);
            tileVisuals[i].transform.localRotation = Quaternion.Euler(0f, 0f, -shapeData.rotationStep * 90f);
        }
    }

    private void RebuildColliders()
    {
        // Remove old box colliders
        foreach (var col in GetComponents<BoxCollider2D>())
            Destroy(col);

        // Re-add one per cell
        foreach (var cell in shapeData.cells)
        {
            var col = gameObject.AddComponent<BoxCollider2D>();
            col.offset = new Vector2(cell.x, cell.y);
            col.size   = Vector2.one;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Placement helpers
    // ─────────────────────────────────────────────────────────────

    public void AutoPlace()
    {
        if (isPlaced) return;
        Vector2Int? firstLegal = GridManager.Instance.FindFirstLegalPosition(shapeData);
        if (firstLegal.HasValue)
            PlaceAt(firstLegal.Value);
    }

    public void ForceDrop()
    {
        if (!isDragging) return;
        isDragging = false;
        SetVisualsSortingOrder(1);

        Vector2Int gridCoord = GridManager.Instance.WorldToGrid(transform.position);
        if (GridManager.Instance.IsPositionLegal(gridCoord, shapeData))
        {
            PlaceAt(gridCoord);
        }
        else
        {
            transform.position = originalPosition;
            gridCoord = GridManager.Instance.WorldToGrid(originalPosition);

            if (GridManager.Instance.IsInBounds(gridCoord) &&
                GridManager.Instance.IsPositionLegal(gridCoord, shapeData))
            {
                GridManager.Instance.PlaceShape(shapeData, gridCoord);
                currentGridPos = gridCoord;
                isPlaced = true;
                if (ShapeFactory.Instance.currentShape == this)
                    RoundManager.Instance.shapePlacedThisRound = true;
                SetVisualsColor(defaultColor);
                PlayPlaceSound();
            }
        }

        SetVisualsColor(defaultColor);
    }

    public void PlaceAt(Vector2Int coord, bool playSound = true)
    {
        transform.position = GridManager.Instance.GridToWorld(coord);
        originalPosition   = transform.position;
        GridManager.Instance.PlaceShape(shapeData, coord);
        currentGridPos = coord;
        isPlaced = true;
        if (ShapeFactory.Instance.currentShape == this)
            RoundManager.Instance.shapePlacedThisRound = true;
        SetVisualsColor(defaultColor);
        if (playSound) PlayPlaceSound();
    }

    // ─────────────────────────────────────────────────────────────
    // Visuals / ghost
    // ─────────────────────────────────────────────────────────────

    private void InitializeVisuals()
    {
        if (shapeData == null) return;

        for (int i = 0; i < shapeData.cells.Length; i++)
        {
            var cellOffset = shapeData.cells[i];

            GameObject tile = new GameObject("Tile_" + i);
            tile.transform.SetParent(transform);
            tile.transform.localPosition = new Vector3(cellOffset.x, cellOffset.y, 0);

            var rend = tile.AddComponent<SpriteRenderer>();
            if (shapeData.cellSprites != null && i < shapeData.cellSprites.Length)
                rend.sprite = shapeData.cellSprites[i];

            tileVisuals.Add(tile);

            var col = gameObject.AddComponent<BoxCollider2D>();
            col.offset = new Vector2(cellOffset.x, cellOffset.y);
            col.size   = Vector2.one;
        }
    }

    private void UpdateGhostVisuals()
    {
        Vector2Int gridCoord = GridManager.Instance.WorldToGrid(transform.position);
        bool isLegal = GridManager.Instance.IsPositionLegal(gridCoord, shapeData);

        SetVisualsColor(isLegal ? legalColor : illegalColor);

        if (isLegal)
        {
            transform.position = GridManager.Instance.GridToWorld(gridCoord);
            if (gridCoord != lastLegalGridCoord)
            {
                PlayHoverSound();
                lastLegalGridCoord = gridCoord;
            }
        }
    }

    private void SetVisualsSortingOrder(int order)
    {
        foreach (var rend in gameObject.GetComponentsInChildren<SpriteRenderer>())
            rend.sortingOrder = order;
    }

    private void SetVisualsColor(Color color)
    {
        foreach (var tile in tileVisuals)
        {
            var rend = tile.GetComponent<SpriteRenderer>();
            if (rend != null) rend.color = color;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Utilities
    // ─────────────────────────────────────────────────────────────

    private bool IsInteractionAllowed() =>
        RoundManager.Instance.state == RoundState.Playing &&
        !SettingsManager.Instance.IsSettingsOpen();

    /// <summary>
    /// Returns true if the pointer is currently over one of this object's colliders.
    /// </summary>
    private bool IsPointerOverThisObject()
    {
        Vector2 worldPoint = GetPointerWorldPos();
        var hits = Physics2D.OverlapPointAll(worldPoint);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject)
                return true;
        }
        return false;
    }

    private Vector3 GetPointerWorldPos()
    {
        Vector2 screenPos = pointerPositionAction.ReadValue<Vector2>();
        Vector3 worldPos  = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -mainCamera.transform.position.z));
        worldPos.z = 0f;
        return worldPos;
    }

    // ─────────────────────────────────────────────────────────────
    // Audio
    // ─────────────────────────────────────────────────────────────

    private void PlayPickupSound()
    {
        if (pickUpSound != null) _audioSource.PlayOneShot(pickUpSound);
    }

    private void PlayPlaceSound()
    {
        if (placeSound != null) _audioSource.PlayOneShot(placeSound);
    }

    private void PlayHoverSound()
    {
        if (hoverSound != null) _audioSource.PlayOneShot(hoverSound);
    }
}