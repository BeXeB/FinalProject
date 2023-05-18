using System;
using System.Collections;
using System.Collections.Generic;
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
        public Action<object, object> onChange;
    }
    
    [SerializeField] private GameObject functionHolder;
    [SerializeField] public List<ExtVariable> extVariables;
    [SerializeField] private TextAsset baseTemplate;

    private List<Token> extVariableTokens = new ();
    private List<Token> extVariableTokensPrev = new ();
    
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

    private void Awake()
    {
        externalFunctions = functionHolder.GetComponents<ExternalFunction>();
        
        ConvertExtFunctions();

        ConvertExtVariables();

        interpreter = new Interpreter(extFunctions, extVariableTokens);
        lexer = new Lexer();
        parser = new Parser();
        resolver = new Resolver();
        resolver.SetInterpreter(interpreter);
    }
    
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
            var token = new Token
            {
                type = TokenType.IDENTIFIER,
                //Convert type to the correct type
                literal = CheckIfNumber(variable)
                    ? variable.seeMMType is SeeMMType.INT
                        ? Convert.ToInt32(variable.literalNumber)
                        : Convert.ToDecimal(variable.literalNumber)
                    : variable.literalBool,
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
            extFunctions.Add(func.functionName, new SeeMMExternalFunction(func.arity, func.function, func.argumentTypes));
        }
    }

    private bool CheckIfNumber(ExtVariable variable)
    {
        return variable.seeMMType is SeeMMType.INT or SeeMMType.FLOAT;
    }

    private void Start()
    {
        CheckAndReadCodeFile();
        CheckCode(code);
        interpreter.InterpretCode(statements);
        callExpression = new Expression.CallExpression(
            new Expression.VariableExpression(new Token { textValue = "main" }),
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
            return;
        }

        StartCoroutine(RunCode());
    }

    private void CheckCode(string code)
    {
        tokens = lexer.ScanCode(code);
        parser.SetTokens(tokens);
        statements = parser.Parse();
        resolver.Resolve(statements);
    }

    private IEnumerator RunCode()
    {
        shouldRun = false;
        interpreter.Evaluate(callExpression);
        
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

        yield return null;//new WaitForSeconds(.1f);
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
        interpreter.InterpretCode(statements);

        try
        {
            interpreter.GetGlobals().Get(new Token { textValue = "main" });
            SaveCode(editorCode);
            return true;
        }
        catch (RuntimeError)
        {
            return false;
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

    public Dictionary<string,ICallable> GetGlobalFunctions()
    {
        return interpreter.GetGlobalFunctions();
    }
}