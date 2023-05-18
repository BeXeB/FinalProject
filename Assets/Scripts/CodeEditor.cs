using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class CodeEditor : MonoBehaviour
{
    CodeRunner codeRunner;
    public static CodeEditor instance;

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text extStuffText;

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
        inputField.text = codeRunner.GetCode();
    }

    public void SetExtVariables(List<Token> codeRunnerExtVariables, List<CodeRunner.ExtVariable> extVariables)
    {
        StringBuilder sb = new StringBuilder();
        
        sb.Append("External Variables:\n");
        for (var i = 0; i < codeRunnerExtVariables.Count; i++)
        {
            var variable = codeRunnerExtVariables[i];
            sb.Append($"{variable.seeMMType.ToString().ToLower()} {variable.textValue}: {variable.literal}\n");
            var extVariable = extVariables[i];
            extVariable.onChange += (previousValue, value) =>
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(extStuffText.text);
                var textToReplace = $"{extVariable.seeMMType.ToString().ToLower()} {extVariable.textValue}: {previousValue}";
                var textToReplaceWith = $"{extVariable.seeMMType.ToString().ToLower()} {extVariable.textValue}: {value}";
                sb.Replace(textToReplace, textToReplaceWith);
                extStuffText.text = sb.ToString();
            };
            extVariables[i] = extVariable;
        }

        extStuffText.text = sb.ToString();
    }

    public void SetExtFunctions(Dictionary<string, SeeMMExternalFunction> codeRunnerExtFunctions)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(extStuffText.text);
        sb.Append("\nExternal Functions:\n");
        foreach (var function in codeRunnerExtFunctions)
        {
            sb.Append($"{function.Key}(");
            var argumentTypes = function.Value.GetArgumentTypes();
            if (argumentTypes.Count > 0)
            {
                foreach (var argument in argumentTypes)
                {
                    sb.Append($"{argument.ToString().ToLower()}, ");
                }
                sb.Remove(sb.Length - 2, 2);
            }
            sb.Append(")\n");
        }
        
        extStuffText.text = sb.ToString();
    }

    public void ClearExtStuffText()
    {
        extStuffText.text = "";
    }

    public void setGlobalFunctions( Dictionary<string,ICallable> getGlobalFunctions)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(extStuffText.text);
        sb.Append("\nGlobal Functions:\n");
        foreach (var function in getGlobalFunctions)
        {
            sb.Append($"{function.Key}(");
            var argumentTypes = function.Value.GetArgumentTypes();
            if (argumentTypes.Count > 0)
            {
                foreach (var argument in argumentTypes)
                {
                    sb.Append($"{argument.ToString().ToLower()}, ");
                }
                sb.Remove(sb.Length - 2, 2);
            }
            sb.Append(")\n");
        }
        
        extStuffText.text = sb.ToString();
    }
}
