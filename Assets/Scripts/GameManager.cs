using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;
    Interpreter interpreter = new Interpreter();
    Expression expression = new Expression.BinaryExpression(
        new Expression.LiteralExpression(5), 
        new Token {type = TokenType.PLUS, startIndex=1, value = "+"}, 
        new Expression.GroupingExpression(new Expression.BinaryExpression(
            new Expression.LiteralExpression(3), 
            new Token {type = TokenType.STAR, startIndex = 3, value = "*" },
            new Expression.LiteralExpression(2))));
    // Start is called before the first frame update
    void Start()
    {
        interpreter.InterpretCode(text.text);
        interpreter.TestExpr(expression);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
