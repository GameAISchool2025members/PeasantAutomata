using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CellResources
{
    public int food;
    public int wood;
    public int stone;
}

[Serializable]
public class CellDataRaw
{
    public string currentEvent;
    public string history;
    public string type;              // raw string from JSON
    public string description;
    public string icon;
    public int population;
    public CellResources resources;
    public int morale;
    public string[] status;          // raw string array
}

public class Cell : MonoBehaviour
{
    private Dictionary<string, int> cellType = new Dictionary<string, int>();
    private Dictionary<string, int> statusType = new Dictionary<string, int>();

    private InteractableTile interactableTile;
    private void Awake()
    {
        interactableTile = GetComponent<InteractableTile>();
        //initializeDictionaries();
    }

    public void initializeDictionaries()
    {
        
        cellType.Add("forest", 0);
        cellType.Add("farmland", 1);
        cellType.Add("village", 2);
        cellType.Add("castle", 3);
        cellType.Add("ruins", 4);
        cellType.Add("bandit_camp", 5);
        cellType.Add("river", 6);
        cellType.Add("plain", 7);
        cellType.Add("hill", 8);
        cellType.Add("mountain", 9);
        cellType.Add("cave", 10);
        
        statusType.Add("storm", 0);
        statusType.Add("fire", 1);
        statusType.Add("fortified", 2);
        statusType.Add("plague", 3);
        statusType.Add("flood", 4);
        statusType.Add("drought", 5);
        statusType.Add("sieged", 6);
    }
    public CellDataRaw rawData;
    public GameObject ListOfObjects;
    public GameObject ListOfStatus;
    public GameObject ListOfBlocks;

    private string exampleJson = @"{
        ""type"": ""farmland"",
        ""description"": ""A serene river winding through the plains."",
        ""icon"": ""🌊"",
        ""population"": 0,
        ""resources"": {
            ""food"": 8,
            ""wood"": 2,
            ""stone"": 0
        },
        ""morale"": 50,
        ""status"": [],
        ""currentEvent"": """",
        ""history"": ""many armies crossed this river""
    }";

    private void Start()
    {
        //Debug.Log($"Description: {rawData.description}");
        //updateVisuals();
    }

    // Parses JSON and fills rawData and enums, returns 'this' for chaining if wanted
    public Cell FromJson(string json)
    {
        rawData = JsonUtility.FromJson<CellDataRaw>(json);
        return this;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(rawData, true);
    }

    public void updateVisuals() //update the block visual
    {
        //deactivate all objects on the tile unless it's currently active
        for (int i = 0; i < ListOfObjects.transform.childCount; i++)
        {
            if (cellType[rawData.type] == i)
            {
                var child = ListOfObjects.transform.GetChild(i).gameObject;
                child.SetActive(true);
            }
            else
            {

                ListOfObjects.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        //update the status on the tile
        foreach (string key in statusType.Keys)
        {
            if (rawData.status.Contains(key))
            {
                ListOfStatus.transform.GetChild(statusType[key]).gameObject.SetActive(true);
            }
            else
            {
                ListOfStatus.transform.GetChild(statusType[key]).gameObject.SetActive(false);
            }
        }

        //update the block if river
        if (rawData.type == "river")
        {
            ListOfBlocks.transform.GetChild(0).gameObject.SetActive(false);
            ListOfBlocks.transform.GetChild(1).gameObject.SetActive(false);
            ListOfBlocks.transform.GetChild(2).gameObject.SetActive(true);
        }
        else if (rawData.status.Contains("drought")) //update if it's drought
        {
            ListOfBlocks.transform.GetChild(0).gameObject.SetActive(false);
            ListOfBlocks.transform.GetChild(1).gameObject.SetActive(true);
            ListOfBlocks.transform.GetChild(2).gameObject.SetActive(false);
        }
        else //forest block
        {
            ListOfBlocks.transform.GetChild(0).gameObject.SetActive(true);
            ListOfBlocks.transform.GetChild(1).gameObject.SetActive(false);
            ListOfBlocks.transform.GetChild(2).gameObject.SetActive(false);
        }
        
        interactableTile.SetHistoryInfo(); // Update history info in the tile

    }
}
