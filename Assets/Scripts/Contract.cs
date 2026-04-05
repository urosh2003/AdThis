using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Contract : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject cutPrefab;
    [SerializeField] private GameObject crossPrefab;

    [Header("Groups")]
    [SerializeField] private Transform cutGroup;
    [SerializeField] private Transform crossGroup;

    private int _currentCut = 0;
    public static Contract Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of Contract detected! There should only be one Contract in the scene.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreateNewCut(10);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            CreateNewCross();
        }
        
    }

    /// <summary>
    /// Instantiates a new cut prefab and moves it to the top of the CutGroup hierarchy.
    /// </summary>
    public void CreateNewCut(int cutAmount)
    {
        if (cutPrefab != null && cutGroup != null)
        {
            GameObject newCut = Instantiate(cutPrefab, cutGroup);
            _currentCut += cutAmount;
            newCut.GetComponent<TMPro.TextMeshProUGUI>().text = _currentCut.ToString() + "%";
            newCut.transform.SetAsFirstSibling();
            Debug.Log("Created new cut and moved to top of hierarchy.");
        }
        else
        {
            Debug.LogWarning("CreateNewCut failed: Prefab or Group is missing!");
        }
    }

    /// <summary>
    /// Instantiates a new cross prefab and moves it to the top of the CrossGroup hierarchy.
    /// </summary>
    public void CreateNewCross()
    {
        if (crossPrefab != null && crossGroup != null)
        {
            GameObject newCross = Instantiate(crossPrefab, crossGroup);
            // SetAsFirstSibling moves the object to Index 0 in the hierarchy
            newCross.transform.SetAsFirstSibling();
            Debug.Log("Created new cross and moved to top of hierarchy.");
        }
        else
        {
            Debug.LogWarning("CreateNewCross failed: Prefab or Group is missing!");
        }
    }
}