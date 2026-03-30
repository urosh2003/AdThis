using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > spriteSwapTime)
        {
            timeElapsed = 0;
            currentSprite = currentSprite == 1 ? 2 : 1;
            var childrenRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
            for (var i = 0; i < childrenRenderers.Length; i++)
            {
                if (currentSprite == 1)
                    childrenRenderers[i].sprite = shapeData.cellSprites[i];
                else if (currentSprite == 2 && shapeData.cellSpritesSecond.Length != 0)
                    childrenRenderers[i].sprite = shapeData.cellSpritesSecond[i];
            }
        }
    }

    private void Start()
    {
        spriteSwapTime = shapeData.spriteSwapTime;
        mainCamera = Camera.main;
        originalPosition = transform.position;
        InitializeVisuals();
        timeElapsed = 0;
        currentSprite = 1;
    }

    private void InitializeVisuals()
    {
        if (shapeData == null) return;

        for (int i = 0; i < shapeData.cells.Length; i++)
        {
            var cellOffset = shapeData.cells[i];
            
            GameObject tile = new GameObject("Tile_" + i);
            tile.transform.SetParent(transform);
            tile.transform.localPosition = new Vector3(cellOffset.x, cellOffset.y, 0);
            
            var renderer = tile.AddComponent<SpriteRenderer>();
            if (shapeData.cellSprites != null && i < shapeData.cellSprites.Length)
            {
                renderer.sprite = shapeData.cellSprites[i];
            }
            
            tileVisuals.Add(tile);

            // Add a collider to the root for each cell to capture mouse events
            var col = gameObject.AddComponent<BoxCollider2D>();
            col.offset = new Vector2(cellOffset.x, cellOffset.y);
            col.size = new Vector2(1, 1);
        }
    }

    public void AutoPlace()
    {
        if (isPlaced) return;
        Vector2Int? firstLegal = GridManager.Instance.FindFirstLegalPosition(shapeData);
        if (firstLegal.HasValue)
        {
            PlaceAt(firstLegal.Value);
        }
    }

    public void ForceDrop()
    {
        if (!isDragging) return;
        isDragging = false;
        SetVisualsSortingOrder(1);

        // Try current hover position first, otherwise return to original (undo behavior)
        Vector2Int gridCoord = GridManager.Instance.WorldToGrid(transform.position);
        if (GridManager.Instance.IsPositionLegal(gridCoord, shapeData))
        {
            PlaceAt(gridCoord);
        }
        else
        {
            // Return to original position or some "tray"
            transform.position = originalPosition;
            gridCoord = GridManager.Instance.WorldToGrid(originalPosition);

            // If it was at originalPosition and that was a valid grid position, re-place it
            if (GridManager.Instance.IsInBounds(gridCoord) && GridManager.Instance.IsPositionLegal(gridCoord, shapeData))
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
        originalPosition = transform.position;
        GridManager.Instance.PlaceShape(shapeData, coord);
        currentGridPos = coord;
        isPlaced = true;
        if (ShapeFactory.Instance.currentShape == this)
            RoundManager.Instance.shapePlacedThisRound = true;
        SetVisualsColor(defaultColor);
        if (playSound)
            PlayPlaceSound();
    }

    private void OnMouseDown()
    {
        if (isDragging) return;
        if (RoundManager.Instance.state != RoundState.Playing) return;
        if (SettingsManager.Instance.IsSettingsOpen()) return;

        isDragging = true;
        dragOffset = transform.position - GetMouseWorldPos();
        SetVisualsSortingOrder(10);
        PlayPickupSound();
        // Remove from grid when picked up
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

    private void OnMouseDrag()
    {
        if (RoundManager.Instance.state != RoundState.Playing) return;
        if (!isDragging) return;

        Vector3 newPos = GetMouseWorldPos() + dragOffset;
        transform.position = newPos;

        UpdateGhostVisuals();
    }

    private void OnMouseUp()
    {
        if (RoundManager.Instance.state != RoundState.Playing) return;
        isDragging = false;
        SetVisualsSortingOrder(1);
        if (SettingsManager.Instance.IsSettingsOpen()) return;
        
        Vector2Int gridCoord = GridManager.Instance.WorldToGrid(transform.position);
        
        if (GridManager.Instance.IsPositionLegal(gridCoord, shapeData))
        {
            // Snap to exact grid position
            transform.position = GridManager.Instance.GridToWorld(gridCoord);
            originalPosition = transform.position;
            GridManager.Instance.PlaceShape(shapeData, gridCoord);
            currentGridPos = gridCoord;
            isPlaced = true;
            if (ShapeFactory.Instance.currentShape == this)
                RoundManager.Instance.shapePlacedThisRound = true;

            SetVisualsColor(defaultColor);
        }
        else
        {
            // Return to original position or some "tray"
            transform.position = originalPosition;
            gridCoord = GridManager.Instance.WorldToGrid(originalPosition);
            
            // If it was at originalPosition and that was a valid grid position, re-place it
            if (GridManager.Instance.IsInBounds(gridCoord) && GridManager.Instance.IsPositionLegal(gridCoord, shapeData))
            {
                GridManager.Instance.PlaceShape(shapeData, gridCoord);
                currentGridPos = gridCoord;
            }
            
            SetVisualsColor(defaultColor);
        }
        PlayPlaceSound();
    }

    private void UpdateGhostVisuals()
    {
        Vector2Int gridCoord = GridManager.Instance.WorldToGrid(transform.position);
        bool isLegal = GridManager.Instance.IsPositionLegal(gridCoord, shapeData);
        
        SetVisualsColor(isLegal ? legalColor : illegalColor);

        if (isLegal)
        {
            // Snap while dragging
            transform.position = GridManager.Instance.GridToWorld(gridCoord);
            if(gridCoord != lastLegalGridCoord)
            {
                PlayHoverSound();
                lastLegalGridCoord = gridCoord;
            }
        }
    }

    private void SetVisualsSortingOrder(int order)
    {
        var childrenRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in childrenRenderers)
        {
            renderer.sortingOrder = order;
        }
    }

    private void SetVisualsColor(Color color)
    {
        foreach (var tile in tileVisuals)
        {
            var renderer = tile.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = color;
            }
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    private void PlayPickupSound()
    {
        if (pickUpSound != null)
            _audioSource.PlayOneShot(pickUpSound);
    }

    private void PlayPlaceSound()
    {
        if (placeSound != null)
            _audioSource.PlayOneShot(placeSound);
    }
    
    private void PlayHoverSound()
    {
        if (hoverSound != null)
            _audioSource.PlayOneShot(hoverSound);
    }
}
