using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeRunner : MonoBehaviour
{
    [Serializable]
    private struct ExtVariable
    {
        public string textValue;
        public float literalNumber;
        public bool literalBool;
        public TokenType seeMMType;
    }


    [SerializeField] private TextAsset codeFile;
    [SerializeField] private GameObject functionHolder;
    [SerializeField] private List<ExtVariable> extVariables;

    private ExternalFunction[] externalFunctions;

    private Lexer lexer;
    private Parser parser;
    private Resolver resolver;
    private Interpreter interpreter;

    private bool shouldRun = true;
    private bool isEditorOpen = false;

    private List<Token> tokens;
    private List<Statement> statements;

    private void Awake()
    {
        var extFunctions = new Dictionary<string, SeeMMExternalFunction>();
        externalFunctions = functionHolder.GetComponents<ExternalFunction>();
        foreach (var func in externalFunctions)
        {
            extFunctions.Add(func.functionName, new SeeMMExternalFunction(func.arity, func.function));
        }

        var extVariableTokens = new List<Token>();
        foreach (var variable in extVariables)
        {
            extVariableTokens.Add(new Token
            {
                type = TokenType.IDENTIFIER,
                literal = CheckIfNumber(variable) ? (decimal)variable.literalNumber : variable.literalBool,
                textValue = variable.textValue,
                seeMMType = variable.seeMMType,
                line = -1,
                startIndex = -1
            });
        }

        interpreter = new Interpreter(extFunctions, extVariableTokens);
        lexer = new Lexer();
        parser = new Parser();
        resolver = new Resolver();
        resolver.SetInterpreter(interpreter);
    }
    
    private bool CheckIfNumber(ExtVariable variable)
    {
        return variable.seeMMType is TokenType.INT or TokenType.FLOAT;
    }

    private void Start()
    {
        CheckCode();
    }

    private void Update()
    {
        if (!shouldRun || isEditorOpen)
        {
            return;
        }

        StartCoroutine(RunCode());
    }

    private void CheckCode()
    {
        tokens = lexer.ScanCode("{" + codeFile.text + "}");
        parser.SetTokens(tokens);
        statements = parser.Parse();
        resolver.Resolve(statements);
    }

    private IEnumerator RunCode()
    {
        shouldRun = false;
        interpreter.InterpretCode(statements);
        yield return new WaitForSeconds(.1f);
        shouldRun = true;
    }
}