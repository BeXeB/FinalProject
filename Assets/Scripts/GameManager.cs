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
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // void Start()
    // {
    // interpreter = new Interpreter();
    // lexer = new Lexer();
    // var tokens = lexer.ScanCode(text.text);
    //
    // parser = new Parser(tokens);
    // var statements = parser.Parse();
    //
    // resolver = new Resolver(interpreter);
    // resolver.Resolve(statements);
    // if (hadError || hadRuntimeError)
    // {
    //     return;
    // }
    // interpreter.InterpretCode(statements);
    // }

    public static void Error(Token token, string message)
    {
        if (token.type == TokenType.EOF)
        {
            Report(token.line, " at end", message);
        }
        else
        {
            Report(token.line, " at '" + token.textValue + "'", message);
        }
    }

    private static void Report(int line, string where, string message)
    {
        //TODO: print to text field in UI
        print("[line " + line + "] Error" + where + ": " + message);
        hadError = true;
    }

    public static void RuntimeError(RuntimeError error)
    {
        //TODO: print to text field in UI
        print("[" + error.token.textValue + "]" + error.Message + "\n[line " + error.token.line + "]");
        hadRuntimeError = true;
    }
}
