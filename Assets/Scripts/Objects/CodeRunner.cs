using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class CodeRunner : MonoBehaviour
{
    [Serializable]
    public struct ExtVariable
    {
        public string textValue;
        public float literalNumber;
        public bool literalBool;
        public SeeMMType seeMMType;
        public bool isArray;
        public List<float> arrayNumericValues;
        public List<bool> arrayBoolValues;
        public Action<object, object> onChange;
    }

    [SerializeField] private GameObject functionHolder;
    [SerializeField] public List<ExtVariable> extVariables;
    [SerializeField] private TextAsset baseTemplate;

    private List<Token> extVariableTokens = new();
    private List<Token> extVariableTokensPrev = new();

    private Dictionary<string, SeeMMExternalFunction> extFunctions;

    private ExternalFunction[] externalFunctions;

    private Lexer lexer;
    private Parser parser;
    private Resolver resolver;
    private Interpreter interpreter;

    private bool shouldRun = true;
    private bool isEditorOpen = false;

    private List<Token> tokens;
    private List<Statement> statements;

    private Expression callExpression;

    private string code;
    private string codeFilePath;
    private string codeFolderName;
    private string codeFileName;

    public void SetCodeFolder(string folderName)
    {
        codeFolderName = folderName;
    }

    public void SetCodeFileName(string fileName)
    {
        codeFileName = fileName;
    }

    private void ConvertExtVariables()
    {
        extVariableTokens = new List<Token>();
        foreach (var variable in extVariables)
        {
            object literal = null;
            if (variable.isArray)
            {
                if (CheckIfNumber(variable))
                {
                    var temp = new List<object>();
                    variable.arrayNumericValues.ForEach(x => temp.Add(
                        variable.seeMMType is SeeMMType.INT ? Convert.ToInt32(x) : Convert.ToSingle(x)));
                    literal = temp;
                }
                else
                {
                    literal = variable.arrayBoolValues;
                }
            }
            else
            {
                literal = CheckIfNumber(variable)
                    ? variable.seeMMType is SeeMMType.INT
                        ? Convert.ToInt32(variable.literalNumber)
                        : Convert.ToSingle(variable.literalNumber)
                    : variable.literalBool;
            }

            var token = new Token
            {
                type = TokenType.IDENTIFIER,
                literal = literal,
                textValue = variable.textValue,
                seeMMType = variable.seeMMType,
                line = -1,
                startIndex = -1
            };
            extVariableTokens.Add(token);
        }

        extVariableTokensPrev = new List<Token>(extVariableTokens);
    }

    private void ConvertExtFunctions()
    {
        extFunctions = new Dictionary<string, SeeMMExternalFunction>();

        foreach (var func in externalFunctions)
        {
            extFunctions.Add(func.functionName,
                new SeeMMExternalFunction(func.arity, func.function, func.argumentTypes));
        }
    }

    private bool CheckIfNumber(ExtVariable variable)
    {
        return variable.seeMMType is SeeMMType.INT or SeeMMType.FLOAT;
    }

    private void Start()
    {
        externalFunctions = functionHolder.GetComponents<ExternalFunction>();

        ConvertExtFunctions();

        ConvertExtVariables();

        interpreter = new Interpreter(extFunctions, extVariableTokens);
        lexer = new Lexer();
        parser = new Parser();
        resolver = new Resolver();
        resolver.SetInterpreter(interpreter);
        
        CheckAndReadCodeFile();
        
        CheckCode(code);
        
        resolver.Resolve(statements);
        interpreter.InterpretCode(statements);
        
        callExpression = new Expression.CallExpression(
            new Expression.VariableExpression(new Token { textValue = "Main" }),
            new Token { type = TokenType.RIGHT_PAREN },
            new List<Expression>());
    }

    private void CheckAndReadCodeFile()
    {
        var path = SeeMMScriptsHelper.GetBasePath($"{codeFolderName}/") + $"{codeFileName}.txt";
        if (!File.Exists(path))
        {
            File.WriteAllText(path, baseTemplate.text);
        }

        codeFilePath = path;
        code = File.ReadAllText(path);
    }

    private void Update()
    {
        if (!shouldRun || isEditorOpen)
        {
            CheckChangedVariables();
            return;
        }

        StartCoroutine(RunCode());
    }

    private void CheckChangedVariables()
    {
        for (var i = 0; i < extVariableTokens.Count; i++)
        {
            var variable = extVariableTokens[i];
            var variablePrev = extVariableTokensPrev[i];
            var value = interpreter.LookUpVariable(variable);
            if (value == variablePrev.literal)
            {
                continue;
            }
            variable.literal = value;
            extVariableTokens[i] = variable;
            //handle the change
            extVariables[i].onChange?.Invoke(extVariableTokensPrev[i].literal, value);
            extVariableTokensPrev[i] = variable;
        }
    }

    public void CheckCode(string code)
    {
        tokens = lexer.ScanCode(code);
        parser.SetTokens(tokens);
        statements = parser.Parse();
    }

    private IEnumerator RunCode()
    {
        shouldRun = false;
        interpreter.Evaluate(callExpression);

        CheckChangedVariables();

        yield return null; //new WaitForSeconds(.1f);
        shouldRun = true;
    }

    public void SetIsEditorOpen(bool isEditorOpen)
    {
        this.isEditorOpen = isEditorOpen;
    }

    public bool RunFromEditor(string editorCode)
    {
        interpreter.InitGlobals(extFunctions, extVariableTokens);
        CheckCode(editorCode);
        resolver.Resolve(statements);
        interpreter.InterpretCode(statements);

        try
        {
            interpreter.GetGlobals().Get(new Token { textValue = "Main" });
            code = editorCode;
            SaveCode(editorCode);
            return true;
        }
        catch (RuntimeError)
        {
            return false;
        }
    }

    public void UpdateExternalVariable(Token extToken)
    {
        foreach (var token in extVariableTokens)
        {
            if (token.textValue == extToken.textValue)
            {
                object newValue;
                if (extToken.literal is List<object> list)
                {
                    var temp = new List<object>();
                    foreach (var value in list)
                    {
                        temp.Add(extToken.seeMMType switch
                        {
                            SeeMMType.INT => Convert.ToInt32(value, CultureInfo.InvariantCulture),
                            SeeMMType.FLOAT => Convert.ToSingle(value, CultureInfo.InvariantCulture),
                            SeeMMType.BOOL => Convert.ToBoolean(value, CultureInfo.InvariantCulture),
                            _ => throw new ArgumentOutOfRangeException()
                        });
                    }
                    newValue = temp;
                }
                else
                {
                    newValue = extToken.seeMMType switch
                    {
                        SeeMMType.INT => Convert.ToInt32(extToken.literal, CultureInfo.InvariantCulture),
                        SeeMMType.FLOAT => Convert.ToSingle(extToken.literal, CultureInfo.InvariantCulture),
                        SeeMMType.BOOL => Convert.ToBoolean(extToken.literal, CultureInfo.InvariantCulture),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }

                interpreter.UpdateExternalVariable(token, newValue);
            }
        }
    }

    private void SaveCode(string editorCode)
    {
        File.WriteAllText(codeFilePath, editorCode);
    }

    public string GetCode()
    {
        return code;
    }

    public Dictionary<string, SeeMMExternalFunction> GetExtFunctions()
    {
        return extFunctions;
    }

    public (List<Token>, List<ExtVariable>) GetExtVariables()
    {
        return (extVariableTokens, extVariables);
    }

    public Dictionary<string, ICallable> GetGlobalFunctions()
    {
        return interpreter.GetGlobalFunctions();
    }

    public List<Token> GetTokens()
    {
        return tokens;
    }
}