using System.Collections.Generic;
using UnityEngine;
using FGUIStarter;
using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private List<GameObject> Grid;
    public List<BasicAction> buttons;
    private InteractableTile currentTile;
    private BasicAction currentAction;
    private PromptHandler promptHandler;
    private OpenAIAgent openAIAgent;
    public TMP_InputField textInputField; // Input field for custom actions

    [SerializeField] private Button playButton;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GetNeighborsPairs(Grid);
        foreach (GameObject cell in Grid)
        {
            Cell cellComponent = cell.GetComponent<Cell>();
            if (cellComponent != null)
            {
                cellComponent.initializeDictionaries();
            }
        }
        RandomizeGrid();
        // Create OpenAI agent as a component
        //openAIAgent = gameObject.AddComponent<OpenAIAgent>();
        this.promptHandler = new PromptHandler();
    }

    [SerializeField] private float cellSpacing = 1f;                   // Grid spacing between cells
    [SerializeField] private Vector3 boxHalfExtents = new Vector3(1f, 1f, 1f);  // Half size of overlap box
    [SerializeField] private LayerMask targetLayer;                    // Layer mask to filter neighbors


    public List<(GameObject, List<GameObject>)> GetNeighborsPairs(List<GameObject> grid)
    {
        int width = 5;
        int height = 5;

        var results = new List<(GameObject, List<GameObject>)>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject center = grid[y * width + x];
                var neighbors = new List<GameObject>();

                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            neighbors.Add(grid[ny * width + nx]);
                        }
                        else
                        {
                            neighbors.Add(null); // null or default value
                        }
                    }
                }
                results.Add((center, neighbors));
            }
        }
        Debug.Log(results);
        return results;
    }

    public void updateGrid()
    {
        StartCoroutine(UpdateGridWithDelay());
    }

    private IEnumerator UpdateGridWithDelay()
    {
        List<(GameObject, List<GameObject>)> neighborsPairs = GetNeighborsPairs(Grid);
        Debug.Log($"Found {neighborsPairs.Count} cells with neighbors");

        foreach ((GameObject current, List<GameObject> neighbors) in neighborsPairs)
        {
            Debug.Log($"Starting UpdateCell coroutine for {current.name} with {neighbors.Count} neighbors");
            yield return StartCoroutine(UpdateCell(current, neighbors));

            // Sleep for 2 seconds between each cell update
            Debug.Log("Waiting 2 seconds before next cell update...");
            yield return new WaitForSeconds(0.1f);
        }
        playButton.interactable = true; // Enable play button after update
        Debug.Log("All grid updates completed!");
    }

    public IEnumerator UpdateCell(GameObject currentTile, List<GameObject> neighborTiles)
    {
        Debug.Log("Sending prompt to OpenAI...");
        yield return StartCoroutine(promptHandler.SendPromptWithList(currentTile, neighborTiles));
        //promptHandler.SendPromptWithList(currentTile, neighborTiles);
        Debug.Log("Update complete.");
        Debug.Log("New Cell JSON:\n" + promptHandler.openAIAgent.responseText);

        // Optionally: update this cell's state based on new response
        string cellDataJSON = promptHandler.openAIAgent.responseText;

        string cleanedJSON = cellDataJSON.Replace("```", "")
                              .Replace("json", "", System.StringComparison.OrdinalIgnoreCase).Trim();
        var newCell = currentTile.GetComponent<Cell>();
        newCell.FromJson(cleanedJSON);
        newCell.updateVisuals();
    }

    public void UnpressButtons()
    {
        foreach (var button in buttons)
        {
            if (button.IsPressed)
            {
                button.UnpressButton();
                SetCurrentTile(null); // Unselect tile when unpressing button
            }
        }
    }

    public bool isActionPerfomed()
    {
        return currentAction != null && currentAction.IsPressed;
    }

    public void SetCurrentTile(InteractableTile tile)
    {
        if (currentTile != null)
        {
            currentTile.SetSelectionState(false);
            currentTile.HideInfoBox();
        }

        currentTile = tile;

    }

    // Method to get the current square
    public InteractableTile GetCurrentTile()
    {
        return currentTile;
    }

    public void SetCurrentAction(BasicAction action)
    {
        currentAction = action;
    }

    public BasicAction GetCurrentAction()
    {
        return currentAction;
    }

    public void PerformActionOnTile(string actionText = null)
    {
        // if currenaction is custom action, we need to get the input from the
        // textInputField
        if (currentAction != null && actionText != null)
        {
            GetCurrentTile().GetComponent<Cell>().rawData.currentEvent = actionText;
        }
    }

    public void RandomizeGrid()
    {
        foreach (GameObject tile in Grid)
        {
            Cell cell = tile.GetComponent<Cell>();
            if (cell == null)
            {
                Debug.LogWarning($"Tile {tile.name} has no Cell component!");
                continue;
            }

            CellDataRaw randomData = GenerateRandomCellData();
            Debug.Log($"Current randomData: {randomData}");
            string json = JsonUtility.ToJson(randomData);
            cell.FromJson(json);
            cell.updateVisuals();
        }
    }

    private CellDataRaw GenerateRandomCellData()
    {
        string[] cellTypes = {
        "forest", "farmland", "village", "castle", "ruins",
        "bandit_camp", "river", "plain", "hill", "mountain", "cave"
        };


        string[] statuses = {
        "storm", "fire", "fortified", "plague",
        "flood", "drought", "sieged"
        };

        // Randomly pick type
        string type = cellTypes[UnityEngine.Random.Range(0, cellTypes.Length)];

        // Randomly pick 0–3 status strings
        // int numStatuses = UnityEngine.Random.Range(0, 2);
        // List<string> selectedStatuses = new List<string>();
        // while (selectedStatuses.Count < numStatuses)
        // {
        //     string s = statuses[UnityEngine.Random.Range(0, statuses.Length)];
        //     if (!selectedStatuses.Contains(s)) selectedStatuses.Add(s);
        // }

        return new CellDataRaw
        {
            type = type,
            description = $"A randomly generated {type}.",
            icon = "🌟",
            population = UnityEngine.Random.Range(0, 100),
            morale = UnityEngine.Random.Range(0, 101),
            resources = new CellResources
            {
                food = UnityEngine.Random.Range(0, 10),
                wood = UnityEngine.Random.Range(0, 10),
                stone = UnityEngine.Random.Range(0, 10)
            },
            // status = selectedStatuses.ToArray(),
            status = new string[] { },
            currentEvent = "",
            history = "This land has seen many things."
        };
    }
}
