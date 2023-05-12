using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField]
    private TMP_Text text;
    Interpreter interpreter;
    Parser parser;
    Lexer lexer;
    Resolver resolver;
    static bool hadRuntimeError = false;
    static bool hadError = false;

    Expression expression = new Expression.BinaryExpression(
        new Expression.LiteralExpression(5), 
        new Token {type = TokenType.PLUS, startIndex=1, value = "+"},
        new Expression.GroupingExpression(new Expression.BinaryExpression(
            new Expression.LiteralExpression(3), 
            new Token {type = TokenType.STAR, startIndex = 3, value = "*" },
            new Expression.LiteralExpression(2))));
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        interpreter = new Interpreter();
        lexer = new Lexer();
        var tokens = lexer.ScanCode(text.text);
        //interpreter.TestExpr(expression);
        parser = new Parser(tokens);
        var statements = parser.Parse();
        resolver = new Resolver(interpreter);
        resolver.Resolve(statements);
        if (hadError)
        {
            return;
        }
        interpreter.InterpretCode(statements);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void Error(Token token, string message)
    {
        if (token.type == TokenType.EOF)
        {
            Report(token.line, " at end", message);
        }
        else
        {
            Report(token.line, " at '" + token.value + "'", message);
        }
    }

    private static void Report(int line, string where, string message)
    {
        //TODO: print to text field in UI
        //System.err.println("[line " + line + "] Error" + where + ": " + message);
        hadError = true;
    }

    public static void RuntimeError(RuntimeError error)
    {
        //TODO: print to text field in UI
        //System.err.println(error.getMessage() + "\n[line " + error.token.line + "]");
        hadRuntimeError = true;
    }
}
