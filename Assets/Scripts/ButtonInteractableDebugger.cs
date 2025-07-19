using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonInteractableDebugger : MonoBehaviour
{
    private Button button;
    private bool lastInteractable;

    void Awake()
    {
        button = GetComponent<Button>();
        lastInteractable = button.interactable;
    }

    void Update()
    {
        if (button.interactable != lastInteractable)
        {
            Debug.Log($"[ButtonInteractableDebugger] {gameObject.name} interactable changed: {lastInteractable} → {button.interactable}");
            lastInteractable = button.interactable;
        }
    }
}
