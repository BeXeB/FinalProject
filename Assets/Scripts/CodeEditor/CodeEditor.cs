using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class CodeEditor : MonoBehaviour
{
    CodeRunner codeRunner;
    public static CodeEditor instance;
    
    [SerializeField] private EditorConsole editorConsole;

    [SerializeField] private TMP_InputField codeText;
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private TMP_Text extStuffText;

    [SerializeField] private Color identifierColor;
    [SerializeField] private Color keywordColor;

    private bool isOpen = false;
    private float timer = 0f;
    private bool shouldCheck = false;

    //TODO set to false when closing editor
    private CodeEditor()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        codeInputField.onValueChanged.AddListener(delegate { OnCodeChanged(); });
    }

    private void OnCodeChanged()
    {
        timer = 1f;
        codeText.text = codeInputField.text;
        shouldCheck = true;
    }

    private void OnDisable()
    {
        codeInputField.onValueChanged.RemoveAllListeners();
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        
        if (!(timer <= 0f) || !shouldCheck) return;

        editorConsole.Clear();
        codeRunner.CheckCode(codeInputField.text);
        HighlightCode(codeRunner.GetTokens().Distinct().ToList());
        shouldCheck = false;
    }

    public void OnButtonPressed()
    {
        codeRunner.RunFromEditor(codeInputField.text);

        if (codeRunner.RunFromEditor(codeInputField.text))
        {
            codeRunner.SetIsEditorOpen(false);
        }
        else
        {
            Debug.Log("Missing Main()");
        }

        isOpen = false; //TODO remove
    }

    public void SetCodeRunner(CodeRunner codeRunner)
    {
        this.codeRunner = codeRunner;
        this.codeRunner.SetIsEditorOpen(true);
        var code = codeRunner.GetCode();
        codeText.text = code;
        codeInputField.text = code;
        isOpen = true;
        //StartCoroutine(CheckCode());
    }

    // private IEnumerator CheckCode()
    // {
    //     while (isOpen)
    //     {
    //         RemoveHighlightColors();
    //         codeRunner.CheckCode(codeInputField.text);
    //         HighlightCode(codeRunner.GetTokens().Distinct().ToList());
    //         yield return new WaitForSeconds(.2f);
    //     }
    // }
    

    private void HighlightCode(List<Token> tokens)
    {
        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            switch (token.type)
            {
                case TokenType.IDENTIFIER:
                    codeText.text = codeText.text.Replace(token.textValue,
                        $"<color=#{ColorUtility.ToHtmlStringRGB(identifierColor)}>{token.textValue}</color>");
                    break;
                case TokenType.ELSE:
                case TokenType.FALSE:
                case TokenType.IF:
                case TokenType.BREAK:
                case TokenType.CONTINUE:
                case TokenType.RETURN:
                case TokenType.TRUE:
                case TokenType.INT:
                case TokenType.FLOAT:
                case TokenType.BOOL:
                case TokenType.WHILE:
                case TokenType.FUNC:
                    codeText.text = codeText.text.Replace(token.textValue,
                        $"<color=#{ColorUtility.ToHtmlStringRGB(keywordColor)}>{token.textValue}</color>");
                    break;
                default:
                    continue;
            }
        }
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
                var textToReplace =
                    $"{extVariable.seeMMType.ToString().ToLower()} {extVariable.textValue}: {previousValue}";
                var textToReplaceWith =
                    $"{extVariable.seeMMType.ToString().ToLower()} {extVariable.textValue}: {value}";
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

    public void SetGlobalFunctions(Dictionary<string, ICallable> getGlobalFunctions)
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

    public void ClearExtStuffText()
    {
        extStuffText.text = "";
    }
    
    public void LogError(string error)
    {
        editorConsole.Log(error);
    }
}