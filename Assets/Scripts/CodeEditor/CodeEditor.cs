using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

    private float timer = 0f;
    private bool shouldCheck = false;

    private bool isLoadedIn = false;

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
        codeInputField.onSelect.AddListener(delegate { OnCodeInputSelected(); });
    }

    private void OnCodeInputSelected()
    {
        codeRunner.SetIsEditorOpen(true);
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

    public void OnRunButtonPressed()
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
    }

    public void OnCloseButtonPressed()
    {
        isLoadedIn = false;
        codeRunner.SetIsEditorOpen(false);
        SceneManager.UnloadSceneAsync("CodeEditor");
        var player = FindObjectOfType<PlayerInput>();
        player.currentActionMap.Enable();
    }

    public void SetCodeRunner(CodeRunner codeRunner)
    {
        this.codeRunner = codeRunner;
        this.codeRunner.SetIsEditorOpen(true);
        isLoadedIn = true;
        var code = codeRunner.GetCode();
        codeText.text = code;
        codeInputField.text = code;
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
            var extVariable = extVariables[i];
            sb.Append(MakeVariableString(variable, extVariable));
            sb.Append("\n");
            
            extVariable.onChange += (previousValue, value) =>
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(extStuffText.text);
                var textToReplace =
                    MakeVariableString(variable, extVariable, false, previousValue);
                var textToReplaceWith =
                    MakeVariableString(variable, extVariable, false, value);
                sb.Replace(textToReplace, textToReplaceWith);
                extStuffText.text = sb.ToString();
            };
            extVariables[i] = extVariable;
        }

        extStuffText.text = sb.ToString();
    }

    private string MakeVariableString(Token variable, CodeRunner.ExtVariable extVariable, bool useToken = true , object variableValue = null)
    {
        var sb = new StringBuilder();
        var varType = useToken ? variable.seeMMType : extVariable.seeMMType;
        sb.Append($"{varType.ToString().ToLower()}");
        if (extVariable.isArray)
        {
            sb.Append("[]");
        }
        var varName = useToken ? variable.textValue : extVariable.textValue;
        sb.Append($" {varName}: ");
        var varValue = useToken ? variable.literal : variableValue;
        if (varValue is List<object> list)
        {
            sb.Append("{");
            foreach (var value in list)
            {
                sb.Append($"\n  {value}, ");
            }
            if (list.Count > 0)
            {
                sb.Remove(sb.Length - 4, 4);
            }
            sb.Append("\n}");
        }
        else
        {
            sb.Append($"\n  {varValue}");
        }

        return sb.ToString();
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
        if (!isLoadedIn || editorConsole == null || error == null)
        {
            return;
        }
        editorConsole.Log(error);
    }
}