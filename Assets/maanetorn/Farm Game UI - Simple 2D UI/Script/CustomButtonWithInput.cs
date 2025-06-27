using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

namespace FGUIStarter
{
    public class CustomAction : BasicAction
    {
        [Header("Input Field Settings")]
        [SerializeField] private TMP_InputField textInputField;
        [SerializeField] private GameObject inputFieldContainer;
        
        [Header("Input Configuration")]
        [SerializeField] private string placeholderText = "Enter custom action...";
        [SerializeField] private int characterLimit = 100;
        
        // Events for custom actions
        public event System.Action<string> OnCustomActionSubmitted;
        public event System.Action OnCustomActionCanceled;
        
        private string currentInputText = "";

        public override void Start()
        {
            // Set this button to CustomAction type
            actionType = ActionType.CustomAction;
            
            base.Start();
            InitializeInputField();
        }

        private void InitializeInputField()
        {
            if (textInputField != null)
            {
                // Configure input field
                textInputField.characterLimit = characterLimit;
                textInputField.placeholder.GetComponent<TextMeshProUGUI>().text = placeholderText;
                
                // Subscribe to input field events
                textInputField.onValueChanged.AddListener(OnInputValueChanged);
                textInputField.onEndEdit.AddListener(OnInputEndEdit);
            }
            
            // Initially hide input field container
            if (inputFieldContainer != null)
            {
                inputFieldContainer.SetActive(false);
            }
        }

        public override void OnButtonClick()
        {
            if (!IsPressed)
            {
                // Press the button and show input field
                GameManager.Instance.UnpressButtons();
                SetPressedState(true);
                //ShowInputField();
                OnActionActivated();
            }
            else
            {
                // Hide input field and unpress button
                HideInputField();
                UnpressButton();
            }
        }

        private void ShowInputField()
        {
            if (inputFieldContainer != null)
            {
                inputFieldContainer.SetActive(true);
                
                // Focus on input field and clear previous text
                if (textInputField != null)
                {
                    textInputField.text = "";
                    textInputField.Select();
                    textInputField.ActivateInputField();
                }
            }
        }

        private void HideInputField()
        {
            if (inputFieldContainer != null)
            {
                inputFieldContainer.SetActive(false);
            }
            
            currentInputText = "";
        }

        private void OnInputValueChanged(string value)
        {
            currentInputText = value;
        }

        private void OnInputEndEdit(string value)
        {
            // Submit when editing ends (Enter key, clicking outside, etc.)
            if (!string.IsNullOrWhiteSpace(value))
            {
                if(GameManager.Instance.GetCurrentTile() != null)
                {
                    // Submit custom action with the input value
                    SubmitCustomAction(value);
                }
                // else
                // {
                //     Debug.Log("Custom action submitted without a tile selected. Cancelling action.");
                //     // If no tile is selected, just cancel/hide
                //     CancelCustomAction();
                // }
            }
            else
            {
                // If empty, just cancel/hide
                CancelCustomAction();
            }
        }

        private void SubmitCustomAction(string actionText)
        {
            // Invoke custom action event
            OnCustomActionSubmitted?.Invoke(actionText);
            
            // Log for debugging
            Debug.Log($"Custom Action Submitted: {actionText} on {GameManager.Instance.GetCurrentTile()?.name ?? "No Tile Selected"}");
            
            // Hide input field and unpress button
            UnpressButton();
            
            // Here you can add specific logic for handling the custom action
            ProcessCustomAction(actionText);
        }

        private void CancelCustomAction()
        {
            // Invoke cancel event
            OnCustomActionCanceled?.Invoke();
            
            // Hide input field and unpress button
            UnpressButton();
        }

        protected virtual void ProcessCustomAction(string actionText)
        {
            GameManager.Instance.PerformActionOnTile(actionText);
        }

        protected override void OnActionActivated()
        {
            base.OnActionActivated();
            // Override base behavior for custom action
            Debug.Log("Custom action with input activated - showing input field");
            ShowInputField();
        }

        protected override void OnActionDeactivated()
        {
            base.OnActionDeactivated();
            // Override base behavior for custom action
            Debug.Log("Custom action with input deactivated - hiding input field");
            HideInputField();
        }

        public override void UnpressButton()
        {
            HideInputField();
            base.UnpressButton();
        }

        // Additional Update logic for handling input field interactions
        public override void Update()
        {
            //base.Update();
            
            // Handle Escape key to cancel input
            if (IsPressed && inputFieldContainer != null && inputFieldContainer.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CancelCustomAction();
                }
            }

            // Prevent unpressing when clicking on input field elements
            if (IsPressed && Input.GetMouseButtonDown(0))
            {
                //here i should check if the pointer is over a tile to perform
                //the custom action
                if (IsPointerOverInputElements())
                {
                    return; // Don't unpress if clicking on input elements
                }
                CancelCustomAction(); // Unpress if clicking outside input elements
            }
        }

        private bool IsPointerOverInputElements()
        {
            if (inputFieldContainer == null || !inputFieldContainer.activeSelf)
                return false;
                
            // Check if pointer is over input field container or its children
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (result.gameObject == inputFieldContainer ||
                    result.gameObject.transform.IsChildOf(inputFieldContainer.transform))
                {
                    return true;
                }
            }

            return false;
        }

        // Public methods for external access
        public string GetCurrentInputText()
        {
            return currentInputText;
        }

        public void SetPlaceholderText(string placeholder)
        {
            placeholderText = placeholder;
            if (textInputField != null && textInputField.placeholder != null)
            {
                textInputField.placeholder.GetComponent<TextMeshProUGUI>().text = placeholder;
            }
        }

        public void SetCharacterLimit(int limit)
        {
            characterLimit = limit;
            if (textInputField != null)
            {
                textInputField.characterLimit = limit;
            }
        }

        // Method to programmatically set pressed state (override to handle input field)
        protected void SetPressedState(bool pressed)
        {
            isPressed = pressed;
            if (buttonImage != null)
            {
                buttonImage.sprite = pressed ? pressedSprite : defaultSprite;
            }
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            if (textInputField != null)
            {
                textInputField.onValueChanged.RemoveListener(OnInputValueChanged);
                textInputField.onEndEdit.RemoveListener(OnInputEndEdit);
            }
        }
    }
}
