using UnityEngine;
using TMPro;

public class CellController : MonoBehaviour
{
    private TMP_InputField inputField;
    private int row;
    private int column;

    private PuzzleManager puzzleManager;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.characterLimit = 1;
        inputField.onValueChanged.AddListener(OnCellValueChanged);

        // Find the PuzzleManager in the scene
        puzzleManager = FindFirstObjectByType<PuzzleManager>();
    }

    public void SetupCell(int r, int c)
    {
        row = r;
        column = c;
    }

    void OnCellValueChanged(string text)
    {
        int value = 0;

        if (!string.IsNullOrEmpty(text))
        {
            char digit = text[0];

            if (digit >= '1' && digit <= '9')
            {
                value = digit - '0';
                inputField.text = digit.ToString();
            }
            else
            {
                inputField.text = "";
            }
        }
        else
        {
            value = 0;
        }

        puzzleManager.UpdateCell(row, column, value);

        if (puzzleManager.IsValid(row, column))
        {
            GetComponent<TMP_InputField>().image.color = Color.white;
        }
        else
        {
            GetComponent<TMP_InputField>().image.color = Color.red;
        }
    }

    public void SetValue(int value, bool locked)
    {
        if (value > 0)
            inputField.text = value.ToString();
        else
            inputField.text = "";

        inputField.interactable = !locked;
    }
}