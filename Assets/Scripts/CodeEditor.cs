using TMPro;
using UnityEngine;

public class CodeEditor : MonoBehaviour
{
    CodeRunner codeRunner;
    public static CodeEditor instance;

    [SerializeField] private TMP_InputField inputField;

    private CodeEditor()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void OnButtonPressed()
    {
        codeRunner.RunFromEditor(inputField.text);

        if (codeRunner.RunFromEditor(inputField.text))
        {
            codeRunner.SetIsEditorOpen(false);
        }
        else
        {
            Debug.Log("Missing Main()");
        }
    }

    public void SetCodeRunner(CodeRunner codeRunner)
    {
        this.codeRunner = codeRunner;
        this.codeRunner.SetIsEditorOpen(true);
    }
}
