using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parser
{
    private class ParseError : System.Exception { }

    private GameManager gameManager;
    private readonly List<Token> tokens;
    private int current = 0;
    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
        gameManager = GameManager.instance;
    }

    public Expression Parse()
    {
        try
        {
            return Expr();
        }
        catch (ParseError error)
        {
            return null;
        }
    }

    private Expression Expr()
    {
        return Equality();
    }

    private Expression Equality()
    {
        Expression expr = Comparison();
        while (Match(TokenType.NOT_EQUAL, TokenType.EQUAL_EQUAL))
        {
            Token op = Previous();
            Expression right = Comparison();
            expr = new Expression.BinaryExpression(expr, op, right);
        }
        return expr;
    }

    private Expression Comparison()
    {
        Expression expr = Term();
        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            Token op = Previous();
            Expression right = Term();
            expr = new Expression.BinaryExpression(expr, op, right);
        }
        return expr;
    }

    private Expression Term()
    {
        Expression expr = Factor();
        while (Match(TokenType.MINUS, TokenType.PLUS))
        {
            Token op = Previous();
            Expression right = Factor();
            expr = new Expression.BinaryExpression(expr, op, right);
        }
        return expr;
    }

    private Expression Factor()
    {
        Expression expr = Unary();
        while (Match(TokenType.SLASH, TokenType.STAR, TokenType.MOD))
        {
            Token op = Previous();
            Expression right = Unary();
            expr = new Expression.BinaryExpression(expr, op, right);
        }
        return expr;
    }

    private Expression Unary()
    {
        if (Match(TokenType.NOT, TokenType.MINUS))
        {
            Token op = Previous();
            Expression right = Unary();
            return new Expression.UnaryExpression(op, right);
        }
        return Primary();
    }

    private Expression Primary()
    {
        if (Match(TokenType.FALSE))
        {
            return new Expression.LiteralExpression(false);
        }
        if (Match(TokenType.TRUE))
        {
            return new Expression.LiteralExpression(true);
        }
        if (Match(TokenType.NUMBER))
        {
            return new Expression.LiteralExpression(Previous().value);
        }
        if (Match(TokenType.LEFT_PAREN))
        {
            Expression expr = Expr();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expression.GroupingExpression(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd())
        {
            return false;
        }
        return Peek().type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            current++;
        }
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().type == TokenType.EOF;
    }

    private Token Peek()
    {
        return tokens[current];
    }

    private Token Previous()
    {
        return tokens[current - 1];
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type))
        {
            return Advance();
        }
        throw Error(Peek(), message);
    }

    private ParseError Error(Token token, string message)
    {
        gameManager.Error(token, message);
        return new ParseError();
    }

    private void Syncronize()
    {
        Advance();
        while (!IsAtEnd())
        {
            if (Previous().type == TokenType.SEMICOLON) return;
            switch (Peek().type)
            {
                case TokenType.FUNC:
                case TokenType.IF:
                case TokenType.BREAK:
                case TokenType.CONTINUE:
                case TokenType.RETURN:
                case TokenType.INT:
                case TokenType.FLOAT:
                case TokenType.BOOL:
                case TokenType.WHILE:
                    return;
            }
            Advance();
        }
    }
}
