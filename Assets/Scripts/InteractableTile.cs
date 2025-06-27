using FGUIStarter;
using UnityEngine;

public class InteractableTile : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private GameObject outline;
    [SerializeField] private InformationBox infoBox;

    [Header("Tile Information")]
    [TextArea(3, 5)]
    [SerializeField] private string historyInfo = "This is a placeholder.";

    [Header("State")]
    [SerializeField] private bool isSelected = false;

    public bool IsSelected => isSelected;
    public string HistoryInfo => historyInfo;
    private Cell cell;

    #region Unity Lifecycle
    void Update()
    {
        HandleMouseInput();
    }

    void Awake()
    {
        cell = GetComponent<Cell>();

    }
    #endregion

    #region Input Handling
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsClickedOn())
            {
                HandleTileClick();
            }
            else if (isSelected)
            {
                // If clicking outside while this tile is selected, deselect it
                DeselectTile();
            }
        }
    }

    private bool IsClickedOn()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.transform == transform;
        }
        return false;
    }

    private void HandleTileClick()
    {
        if (GameManager.Instance.GetCurrentTile() == this)
        {
            // If this tile is already selected, deselect it
            DeselectTile();
        }
        else
        {
            // Select this tile
            SelectTile();
        }
    }
    #endregion

    #region Tile Selection
    public void SelectTile()
    {
        SetSelectionState(true);
        GameManager.Instance.SetCurrentTile(this);
        // Show info box only if no action is being performed
        if (!GameManager.Instance.isActionPerfomed())
        {
            ShowInfoBox();
        }
        else
        {
            //process the action on the tile
            GameManager.Instance.PerformActionOnTile(GameManager.Instance.GetCurrentAction().GetComponent<CustomAction>()?.GetCurrentInputText());
        }


    }

    public void DeselectTile()
    {
        HideInfoBox();
        SetSelectionState(false);
        GameManager.Instance.SetCurrentTile(null);
    }

    public void SetSelectionState(bool selected)
    {
        isSelected = selected;
        outline.SetActive(selected);
    }
    #endregion

    #region Information Display
    private void ShowInfoBox()
    {
        if (infoBox != null)
        {
            infoBox.gameObject.SetActive(true);
        }
    }

    public void HideInfoBox()
    {
        if (infoBox != null)
        {
            infoBox.gameObject.SetActive(false);
        }
    }

    public void SetHistoryInfo()
    {
        historyInfo = cell.rawData.history; // Initialize history info from Cell data
    }
    #endregion

    #region Validation
    private void OnValidate()
    {
        // Ensure required components are assigned
        if (outline == null)
        {
            Debug.LogWarning($"Outline GameObject is not assigned on {gameObject.name}", this);
        }

        if (infoBox == null)
        {
            Debug.LogWarning($"InformationBox is not assigned on {gameObject.name}", this);
        }
    }
    #endregion
}
