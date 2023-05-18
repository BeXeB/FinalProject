using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeRunner : MonoBehaviour
{
    [Serializable]
    public struct ExtVariable
    {
        public string textValue;
        public float literalNumber;
        public bool literalBool;
        public TokenType seeMMType;
        public Action<object> onChange;
    }


    [SerializeField] private TextAsset codeFile;
    [SerializeField] private GameObject functionHolder;
    [SerializeField] public List<ExtVariable> extVariables;

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

    private void ConvertExtVariables()
    {
        extVariableTokens = new List<Token>();
        foreach (var variable in extVariables)
        {
            extVariableTokens.Add(new Token
            {
                type = TokenType.IDENTIFIER,
                //Convert type to the correct type
                literal = CheckIfNumber(variable)
                    ? variable.seeMMType is TokenType.INT
                        ? Convert.ToInt32(variable.literalNumber)
                        : Convert.ToDecimal(variable.literalNumber)
                    : variable.literalBool,
                textValue = variable.textValue,
                seeMMType = variable.seeMMType,
                line = -1,
                startIndex = -1
            });
        }

        extVariableTokensPrev = new List<Token>(extVariableTokens);
    }

    private void ConvertExtFunctions()
    {
        extFunctions = new Dictionary<string, SeeMMExternalFunction>();

        foreach (var func in externalFunctions)
        {
            extFunctions.Add(func.functionName, new SeeMMExternalFunction(func.arity, func.function));
        }
    }

    private bool CheckIfNumber(ExtVariable variable)
    {
        return variable.seeMMType is TokenType.INT or TokenType.FLOAT;
    }

    private void Start()
    {
        CheckCode(codeFile.text);
        interpreter.InterpretCode(statements);
        callExpression = new Expression.CallExpression(
            new Expression.VariableExpression(new Token { textValue = "main" }),
            new Token { type = TokenType.RIGHT_PAREN },
            new List<Expression>());
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
            extVariableTokensPrev[i] = variable;
            //handle the change
            extVariables[i].onChange?.Invoke(value);
        }

        yield return null;//new WaitForSeconds(.1f);
        shouldRun = true;
    }

    public void SetIsEditorOpen(bool isEditorOpen)
    {
        this.isEditorOpen = isEditorOpen;
    }

    public bool RunFromEditor(string code)
    {
        interpreter.InitGlobals(extFunctions, extVariableTokens);
        CheckCode(code);
        interpreter.InterpretCode(statements);

        try
        {
            interpreter.GetGlobals().Get(new Token { textValue = "main" });
            return true;
        }
        catch (RuntimeError)
        {
            return false;
        }
    }
}