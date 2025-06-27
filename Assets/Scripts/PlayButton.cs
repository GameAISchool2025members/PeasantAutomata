using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{

    [SerializeField]private Button playButton;
    void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonClick);
    }

    private void OnPlayButtonClick()
    {
        GameManager.Instance.updateGrid();
        playButton.interactable = false; // Disable the button after clicking
    }
}
