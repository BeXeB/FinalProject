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
    Expression expression = new Expression.BinaryExpression(
        new Expression.LiteralExpression(5), 
        new Token {type = TokenType.PLUS, startIndex=1, value = "+"}, //Test with % !!!!!!!
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
        var tokens = interpreter.InterpretCode(text.text);
        //interpreter.TestExpr(expression);
        parser = new Parser(tokens);
        var expr = parser.Parse();
        interpreter.TestExpr(expr);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Error(Token token, string message)
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
    }
}
