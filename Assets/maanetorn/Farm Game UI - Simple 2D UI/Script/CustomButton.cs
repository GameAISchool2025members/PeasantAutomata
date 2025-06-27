using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace FGUIStarter
{
    public enum ActionType
    {
        Fire,
        CustomAction
    }
    public class BasicAction : MonoBehaviour
    {
        [Header("Action Configuration")]
        [SerializeField] protected ActionType actionType = ActionType.Fire;
        
        [Header("Button Components")]
        public Button button;
        public Sprite defaultSprite;
        public Sprite pressedSprite;

        protected bool isPressed = false;
        public bool IsPressed => isPressed;
        public ActionType ActionType => actionType;
        protected Image buttonImage;
        //[SerializeField]protected TMP_InputField inputField;

        public virtual void Start()
        {
            button = GetComponent<Button>();
            buttonImage = button.GetComponent<Image>();

            // Ensure default state
            buttonImage.sprite = defaultSprite;

            // Subscribe to click event
            button.onClick.AddListener(OnButtonClick);
        }

        public virtual void Update()
        {
            if (IsPressed && !IsPointerOverUIObject(gameObject) && Input.GetMouseButtonDown(0))
            {
                UnpressButton();
            }
        }

        public virtual void OnButtonClick()
        {
            if (!isPressed)
            {
                GameManager.Instance.UnpressButtons(); // Unpress other buttons
                isPressed = true;
                buttonImage.sprite = pressedSprite;
                
                
                // Trigger action-specific behavior
                OnActionActivated();
            }
            else
            {
                UnpressButton();
            }
        }

        protected virtual void OnActionActivated()
        {
            GameManager.Instance.SetCurrentAction(this);
            // Override this method in derived classes for specific action behavior
            switch (actionType)
            {
                case ActionType.Fire:
                    Debug.Log("Fire action activated - ready to target");
                    break;
                case ActionType.CustomAction:
                    Debug.Log("Custom action activated - ready for input");
                    break;
            }
        }

        public virtual void UnpressButton()
        {
            isPressed = false;
            buttonImage.sprite = defaultSprite;
            GameManager.Instance.SetCurrentAction(null);
            // Trigger action-specific cleanup
            //OnActionDeactivated();
        }

        protected virtual void OnActionDeactivated()
        {
            GameManager.Instance.SetCurrentAction(null);
            // Override this method in derived classes for cleanup behavior
            switch (actionType)
            {
                case ActionType.Fire:
                    Debug.Log("Fire action deactivated");
                    break;
                case ActionType.CustomAction:
                    Debug.Log("Custom action deactivated");
                    break;
            }
        }

        // Detect if pointer is over a specific UI GameObject
        bool IsPointerOverUIObject(GameObject target)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (result.gameObject == target)
                    return true;
            }

            return false;
        }
    }
}
