using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(menuName = "Game/TileShape")]
public class TileShape : ScriptableObject {
    public string shapeName;
    public Vector2Int[] cells; // e.g. L-shape: {(0,0),(0,1),(0,2),(1,0)}
    public Sprite[] cellSprites; 
    public Sprite[] cellSpritesSecond; 
    public int pointsPerCell = 10;
    public float spriteSwapTime = 0.5f;
    [FormerlySerializedAs("width")] public int spawnOffset = 1;
    public Sprite previewSprite;
}
