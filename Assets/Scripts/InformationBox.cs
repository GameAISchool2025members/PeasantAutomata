using TMPro;
using UnityEngine;

public class InformationBox : MonoBehaviour
{
    private InteractableTile interactableTile;
    private TextMeshProUGUI text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        interactableTile = GetComponentInParent<InteractableTile>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnEnable()
    {
        if (interactableTile != null)
        {
            text.text = interactableTile.HistoryInfo;
        }
    }
}
