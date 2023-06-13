using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

    private Expression mainCallExpression;
    private Expression setupCallExpression;

    private string code;
    private string codeFilePath;
    private string codeFolderName;
    private string codeFileName;

    private void Awake()
    {
        mainCallExpression = new Expression.CallExpression(
            new Expression.VariableExpression(new Token { textValue = "Main" }),
            new Token { type = TokenType.RIGHT_PAREN },
            new List<Expression>());
        setupCallExpression = new Expression.CallExpression(
            new Expression.VariableExpression(new Token { textValue = "Setup" }),
            new Token { type = TokenType.RIGHT_PAREN },
            new List<Expression>());
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

        interpreter.InterpretCode(statements); //TODO this causes the code to run twice
        interpreter.Evaluate(setupCallExpression);
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
                new SeeMMExternalFunction(func.arity, func.function, func.argumentTypes, func.returnType));
        }
    }

    private bool CheckIfNumber(ExtVariable variable)
    {
        return variable.seeMMType is SeeMMType.INT or SeeMMType.FLOAT;
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
        //get the type of the global and external functions and variables
        //add them to the lexer
        
        Dictionary<string, SeeMMType> extIdentifiers = new();

        foreach (var varToken in extVariableTokens)
        {
            extIdentifiers.Add(varToken.textValue, varToken.seeMMType);
        }
        
        foreach (var func in extFunctions)
        {
            extIdentifiers.Add(func.Key, func.Value.GetReturnType());
        }

        foreach (var globFunc in interpreter.GetGlobalFunctions())
        {
            extIdentifiers.Add(globFunc.Key, globFunc.Value.GetReturnType());
        }
        
        lexer.SetIdentifiers(extIdentifiers);
        tokens = lexer.ScanCode(code);
        parser.SetTokens(tokens);
        statements = parser.Parse();
        
        Dictionary<string, List<SeeMMType>> functions = new();

        foreach (var statement in statements)
        {
            if (statement is Statement.FunctionStatement functionStatement)
            {
                var funcName = functionStatement.name.textValue;
                var funcArgs = functionStatement.parameters.Select(x => x.seeMMType).ToList();
                functions.Add(funcName, funcArgs);
            }
        }

        foreach (var func in extFunctions)
        {
            functions.Add(func.Key, func.Value.GetArgumentTypes());
        }

        foreach (var func in interpreter.GetGlobalFunctions())
        {
            functions.Add(func.Key, func.Value.GetArgumentTypes());
        }
        
        resolver.Clear();
        resolver.AddExtAndGlobalFunctions(functions);
        resolver.Resolve(statements);
    }

    private IEnumerator RunCode()
    {
        shouldRun = false;
        interpreter.Evaluate(mainCallExpression);

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
        interpreter.InterpretCode(statements); //TODO this causes the code to run twice

        try
        {
            interpreter.GetGlobals().Get(new Token { textValue = "Setup" });
            interpreter.Evaluate(setupCallExpression);
            interpreter.GetGlobals().Get(new Token { textValue = "Main" });
            code = editorCode;
            SaveCode(editorCode);
            return true;
        }
        catch (RuntimeError e)
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