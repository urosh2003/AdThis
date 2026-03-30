using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StreamerManager : MonoBehaviour
{
    [SerializeField] private List<TileShape> streamerShapes;
    [SerializeField] private Image streamerPreviewImage;
    [SerializeField] private float spriteSwapTime = 0.5f;

    private int _currentStreamerIndex = 0;
    private float _timeElapsed;
    private int _currentSprite = 1;
    private const string StreamerIndexKey = "SelectedStreamerIndex";

    void Start()
    {
        _currentStreamerIndex = PlayerPrefs.GetInt(StreamerIndexKey, 0);
        if (_currentStreamerIndex >= streamerShapes.Count)
        {
            _currentStreamerIndex = 0;
        }
        UpdateStreamerUI();
    }

    void Update()
    {
        if (streamerShapes.Count == 0 || streamerPreviewImage == null) return;

        TileShape currentShape = streamerShapes[_currentStreamerIndex];
        if (currentShape.previewSprite != null)
        {
            streamerPreviewImage.sprite = currentShape.previewSprite;
            return;
        }

        _timeElapsed += Time.deltaTime;
        float swapTime = currentShape.spriteSwapTime > 0 ? currentShape.spriteSwapTime : spriteSwapTime;

        if (_timeElapsed > swapTime)
        {
            _timeElapsed = 0;
            _currentSprite = _currentSprite == 1 ? 2 : 1;
            
            if (_currentSprite == 1)
            {
                if (currentShape.cellSprites.Length > 0)
                    streamerPreviewImage.sprite = currentShape.cellSprites[0];
            }
            else
            {
                if (currentShape.cellSpritesSecond.Length > 0)
                    streamerPreviewImage.sprite = currentShape.cellSpritesSecond[0];
                else if (currentShape.cellSprites.Length > 0)
                    streamerPreviewImage.sprite = currentShape.cellSprites[0];
            }
        }
    }

    public void NextStreamer()
    {
        _currentStreamerIndex = (_currentStreamerIndex + 1) % streamerShapes.Count;
        SaveAndRefresh();
    }

    public void PreviousStreamer()
    {
        _currentStreamerIndex--;
        if (_currentStreamerIndex < 0)
        {
            _currentStreamerIndex = streamerShapes.Count - 1;
        }
        SaveAndRefresh();
    }

    private void SaveAndRefresh()
    {
        PlayerPrefs.SetInt(StreamerIndexKey, _currentStreamerIndex);
        PlayerPrefs.Save();
        _timeElapsed = 0;
        _currentSprite = 1;
        UpdateStreamerUI();
    }

    private void UpdateStreamerUI()
    {
        if (streamerShapes.Count == 0 || streamerPreviewImage == null) return;
        
        TileShape currentShape = streamerShapes[_currentStreamerIndex];
        if (currentShape.previewSprite != null)
        {
            streamerPreviewImage.sprite = currentShape.previewSprite;
        }
        else if (currentShape.cellSprites.Length > 0)
        {
            streamerPreviewImage.sprite = currentShape.cellSprites[0];
        }
    }
}
