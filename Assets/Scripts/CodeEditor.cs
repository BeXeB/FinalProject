using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        if (inputField.text.Contains("function main() {"))//TODO: check this, if main is in globals
        {
            codeRunner.RunFromEditor(inputField.text);
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
