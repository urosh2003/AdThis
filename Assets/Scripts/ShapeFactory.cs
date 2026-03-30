using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeFactory : MonoBehaviour
{
    [SerializeField] private List<TileShape> shapes;
    [SerializeField] private GameObject shapePrefab;
    [SerializeField] private List<TileShape> facecams;
    public int FacecamCount => facecams.Count;
    [SerializeField] private Vector2Int facecamSpawnPosition;
    public static ShapeFactory Instance;
    public ShapeInstance currentShape;
    public bool HasShapes => shapes.Count > 0;

    void Awake()
    {
        Instance = this;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public void SpawnFacecam()
    {
        var camId = PlayerPrefs.GetInt("SelectedStreamerIndex", 0);
        if (camId >= facecams.Count) camId = 0;
        
        shapePrefab.GetComponent<ShapeInstance>().shapeData = facecams[camId];
        var spawnedFacecam = Instantiate(shapePrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<ShapeInstance>();
        spawnedFacecam.PlaceAt(facecamSpawnPosition, false);
    }
    
    public void SpawnShape()
    {
        if (shapes.Count == 0) return;
        
        var shapeId = UnityEngine.Random.Range(0, shapes.Count);
        var shape = shapes[shapeId];
        shapes.RemoveAt(shapeId);
        shapePrefab.GetComponent<ShapeInstance>().shapeData = shape;
        currentShape = Instantiate(shapePrefab, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity).GetComponent<ShapeInstance>();
        switch (shape.spawnOffset)
        {
            case 1:
                currentShape.transform.position += new Vector3(2, 0, 0);
                break;
            case 2:
                currentShape.transform.position += new Vector3(1.5f, 0, 0);
                break;
            case 3:
                currentShape.transform.position += new Vector3(1f, 0, 0);
                break;
            case 5:
                currentShape.transform.position += new Vector3(0, 0, 0);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
