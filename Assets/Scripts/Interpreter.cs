using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Interpreter : Expression.IExpressionVisitor<object>
{

    string code;
    string processedCode = "";

    int startIndex = 0;
    int currentIndex = 0;

    List<Token> tokens = new List<Token>();

    static Dictionary<string, TokenType> keyWords = new Dictionary<string, TokenType>()
    {
        {"else", TokenType.ELSE},
        {"false", TokenType.FALSE},
        {"function", TokenType.FUNC},
        {"if", TokenType.IF},
        {"break", TokenType.BREAK},
        {"continue", TokenType.CONTINUE},
        {"return", TokenType.RETURN},
        {"true", TokenType.TRUE},
        {"int", TokenType.INT},
        {"float", TokenType.FLOAT},
        {"bool", TokenType.BOOL},
        {"while", TokenType.WHILE}
    };
    //Expression Test Method
    public void TestExpr(Expression expr)
    {
        var yes = Evaluate(expr);
    }

    public void InterpretCode(string code)
    {
        if (code == null)
        {
            code = "";
        }

        Lexer(code);
    }

    void Lexer(string rawCode)
    {
        code = rawCode.ToLower();
        while(currentIndex < code.Length)
        {
            startIndex = currentIndex;
            ScanToken();
        }
        AddToken(TokenType.EOF);
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '(': 
                AddToken(TokenType.LEFT_PAREN); 
                break;
            case ')': 
                AddToken(TokenType.RIGHT_PAREN); 
                break;
            case '{': 
                AddToken(TokenType.LEFT_BRACE); 
                break;
            case '}': 
                AddToken(TokenType.RIGHT_BRACE);
                break;
            case '[': 
                AddToken(TokenType.LEFT_SQUAREBRACKET);
                break;
            case ']':
                AddToken(TokenType.RIGHT_SQUAREBRACKET);
                break;
            case ',': 
                AddToken(TokenType.COMMA); 
                break;
            case '.': 
                AddToken(TokenType.DOT); 
                break;
            case '-':
                AddToken(TokenType.MINUS); 
                break;
            case '+':
                AddToken(TokenType.PLUS); 
                break;
            case ';': 
                AddToken(TokenType.SEMICOLON); 
                break;
            case '/': 
                AddToken(TokenType.SLASH);
                break;
            case '*': 
                AddToken(TokenType.STAR); 
                break;
            case '%': 
                AddToken(TokenType.MOD); 
                break;
            case '!':
                AddToken(Match('=') ? TokenType.NOT_EQUAL : TokenType.NOT);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;
            case '&':
                if (Match('&'))
                {
                    AddToken(TokenType.AND);
                }
                else
                {
                    Debug.Log("Unexpected character.");
                }
                break;
            case '|':
                if (Match('|'))
                {
                    AddToken(TokenType.OR);
                }
                else
                {
                    Debug.Log("Unexpected character.");
                }
                break;
            case ' ':
            case '\r':
            case '\t':
            case '\n':
                break;
            default:
                if (char.IsDigit(c))
                {
                    Number();
                }
                else if (char.IsLetter(c))
                {
                    Identifier();
                }
                else
                {
                    Debug.Log("Unexpected character.");
                }
                break;
        }
    }
    
    private char Advance()
    {
        currentIndex++;
        return code[currentIndex - 1];
    }

    private bool Match(char expected)
    {
        if (currentIndex >= code.Length)
        {
            return false;
        }    
        if (code[currentIndex] != expected)
        { 
            return false; 
        }            
        currentIndex++;
        return true;
    }

    private char Peek()
    {
        if (currentIndex >= code.Length)
        {
            return '\0';
        }
        return code[currentIndex];
    }

    private char PeekNext()
    {
        if (currentIndex + 1 >= code.Length)
        {
            return '\0';
        }
        return code[currentIndex + 1];
    }

    private void Number()
    {
        while (char.IsDigit(Peek()))
        {
            Advance();
        }
        // Look for a fractional part.
        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            // Consume the "."
            Advance();
            while (char.IsDigit(Peek()))
            {
                Advance();
            }
        }
        AddToken(TokenType.NUMBER);
    }

    private void Identifier()
    {
        while (char.IsLetterOrDigit(Peek()))
        {
            Advance();
        }
        string text = code.Substring(startIndex, currentIndex-startIndex);
        TokenType type; 
        bool isKeyword = keyWords.TryGetValue(text, out type);
        if (!isKeyword)
        {
            type = TokenType.IDENTIFIER;
        }
        AddToken(type);
    }

    private void AddToken(TokenType type)
    {
        string text = code.Substring(startIndex, currentIndex-startIndex);
        tokens.Add(new Token { type = type, startIndex = startIndex, value = text });
    }

    public object VisitBinaryExpression(Expression.BinaryExpression expression)
    {
        object left = Evaluate(expression.left);
        object right = Evaluate(expression.right);

        switch (expression.op.type)
        {
            case TokenType.NOT_EQUAL: 
                return !IsEqual(left, right);
            case TokenType.EQUAL_EQUAL: 
                return IsEqual(left, right);
            case TokenType.GREATER:
                CheckNumberOperands(left, right);
                return Convert.ToDouble(left) > Convert.ToDouble(right);
            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(left, right);
                return Convert.ToDouble(left) >= Convert.ToDouble(right);
            case TokenType.LESS:
                CheckNumberOperands(left, right);
                return Convert.ToDouble(left) < Convert.ToDouble(right);
            case TokenType.LESS_EQUAL:
                CheckNumberOperands(left, right);
                return Convert.ToDouble(left) <= Convert.ToDouble(right);
            case TokenType.MINUS:
                CheckNumberOperands(left, right);
                return Convert.ToDouble(left) - Convert.ToDouble(right);
            case TokenType.PLUS:
                CheckNumberOperands(left, right);
                return Convert.ToDouble(left) + Convert.ToDouble(right);
            case TokenType.SLASH:
                CheckNumberOperands(left, right);
                return Convert.ToDouble(left) / Convert.ToDouble(right);
            case TokenType.STAR:
                CheckNumberOperands(left, right);
                return Convert.ToDouble(left) * Convert.ToDouble(right);
        }
        return null;
    }

    public object VisitAssignmentExpression(Expression.AssignmentExpression expression)
    {
        object value = Evaluate(expression.value);
        /*
        int distance = locals.get(expr);
        if (distance != null)
        {
            environment.assignAt(distance, expr.name, value);
        }
        else
        {
            globals.assign(expr.name, value);
        }*/

        return value;
    }

    public object VisitLiteralExpression(Expression.LiteralExpression expression)
    {
        return expression.value;
    }

    public object VisitGroupingExpression(Expression.GroupingExpression expression)
    {
        return Evaluate(expression.expression);
    }

    public object VisitLogicalExpression(Expression.LogicalExpression expression)
    {
        object left = Evaluate(expression.left);

        if (expression.op.type == TokenType.OR) {
            if (IsTruthy(left)) return left;
        } 
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(expression.right);
    }

    public object VisitVariableExpression(Expression.VariableExpression expression)
    {
        return LookUpVariable(expression.name, expression);
    }

    public object VisitUnaryExpression(Expression.UnaryExpression expression)
    {
        object right = Evaluate(expression.expression);

        switch (expression.op.type) 
        {
            case TokenType.NOT:
                return !IsTruthy(right);
            case TokenType.MINUS:
                CheckNumberOperand(right);
                return -Convert.ToDouble(right);
        }
        return null;
    }

    private void CheckNumberOperand(object operand)
    {
        if (operand is double) return;
        //throw new RuntimeError(operator, "Operand must be a number.");
    }
    private void CheckNumberOperands(object left, object right)
    {
        if (left is double && right is double) return;
        //throw new RuntimeError(operator, "Operands must be numbers.");
    }
    private bool IsEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }
    private object Evaluate(Expression expr)
    {
        return expr.Accept(this);
    }

    private bool IsTruthy(object obj)
    {
        if (obj == null) return false;
        if (obj is bool) return Convert.ToBoolean(obj);
        return true;
    }

    private object LookUpVariable(Token name, Expression expr)
    {
        /*int distance = locals.get(expr);
        if (distance != null)
        {
            return environment.getAt(distance, name.lexeme);
        }
        else
        {
            return globals.get(name);
        }*/
        return null;
    }
}
