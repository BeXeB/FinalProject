using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parser
{
    private class ParseError : System.Exception { }

    private List<Token> tokens;
    private int current = 0;
    private int fuctions = 0;
    
    public void SetTokens(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    public List<Statement> Parse()
    {
        List<Statement> statements = new List<Statement>();
        while (!IsAtEnd())
        {
            var declaration = Declaration();
            if (declaration is Statement.FunctionStatement)
            {
                statements.Insert(fuctions, declaration);
                fuctions++;
            }
            else
            {
                statements.Add(declaration);
            }
        }
        return statements;
    }

    private Statement Declaration() 
    {
        try
        {
            if (Match(TokenType.FUNC)) return Function();
            if (Match(TokenType.INT)) return VariableDeclaration(TokenType.INT);
            if (Match(TokenType.FLOAT)) return VariableDeclaration(TokenType.FLOAT);
            if (Match(TokenType.BOOL)) return VariableDeclaration(TokenType.BOOL);
            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    private Statement.FunctionStatement Function()
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect function name.");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after function name.");
        List<Token> parameters = new List<Token>();
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }
                parameters.Add(
                Consume(TokenType.IDENTIFIER, "Expect parameter name."));
            } while (Match(TokenType.COMMA));
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
        Consume(TokenType.LEFT_BRACE, "Expect '{' before function body.");
        List<Statement> body = Block();
        return new Statement.FunctionStatement(name, parameters, body);
    }

    private Statement VariableDeclaration(TokenType type)
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");
        Expression initializer = null;
        if (Match(TokenType.EQUAL))
        {
            initializer = Expr();
        }
        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
        switch (type)
        {
            case TokenType.INT:
                return new Statement.IntStatement(name, initializer);
            case TokenType.FLOAT:
                return new Statement.FloatStatement(name, initializer);
            case TokenType.BOOL:
                return new Statement.BoolStatement(name, initializer);
            default:
                return null;
        }
    }


    private Statement Statement()
    {
        if (Match(TokenType.IF)) return IfStatement();
        if (Match(TokenType.RETURN)) return ReturnStatement();
        if (Match(TokenType.BREAK)) return BreakStatement();
        if (Match(TokenType.CONTINUE)) return ContinueStatement();
        if (Match(TokenType.WHILE)) return WhileStatement();
        if (Match(TokenType.LEFT_BRACE)) return new Statement.BlockStatement(Block());

        return ExpressionStatement();
    }

    private Statement IfStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        Expression condition = Expr();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");
        Statement thenBranch = Statement();
        Statement elseBranch = null;
        if (Match(TokenType.ELSE))
        {
            elseBranch = Statement();
        }
        return new Statement.IfStatement(condition, thenBranch, elseBranch);
    }

    private Statement ReturnStatement()
    {
        Token keyword = Previous();
        Expression value = null;
        if (!Check(TokenType.SEMICOLON))
        {
            value = Expr();
        }
        Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
        return new Statement.ReturnStatement(keyword, value);
    }
    
    private Statement BreakStatement()
    {
        Token keyword = Previous();
        Consume(TokenType.SEMICOLON, "Expect ';' after break.");
        return new Statement.BreakStatement(keyword);
    }
    
    private Statement ContinueStatement()
    {
        Token keyword = Previous();
        Consume(TokenType.SEMICOLON, "Expect ';' after continue.");
        return new Statement.ContinueStatement(keyword);
    }

    private Statement ExpressionStatement()
    {
        Expression expression = Expr();
        Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
        return new Statement.ExpressionStatement(expression);
    }

    private Statement WhileStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        Expression condition = Expr();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
        Statement body = Statement();
        return new Statement.WhileStatement(condition, body);
    }

    private List<Statement> Block()
    {
        List<Statement> statements = new List<Statement>();
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            var declaration = Declaration();
            if (declaration is Statement.FunctionStatement)
            {
                statements.Insert(fuctions, declaration);
                fuctions++;
            }
            else
            {
                statements.Add(declaration);
            }
        }
        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Expression Expr()
    {
        return Assignment();
    }

    private Expression Assignment()
    {
        Expression expression = Or();
        if (Match(TokenType.EQUAL))
        {
            Token equals = Previous();
            Expression value = Assignment();
            if (expression is Expression.VariableExpression) 
            {
                Token name = ((Expression.VariableExpression)expression).name;
                return new Expression.AssignmentExpression(name, value);
            }
            Error(equals, "Invalid assignment target.");
        }
        return expression;
    }

    private Expression Or()
    {
        Expression expression = And();
        while (Match(TokenType.OR))
        {
            Token op = Previous();
            Expression right = And();
            expression = new Expression.LogicalExpression(expression, op, right);
        }
        return expression;
    }

    private Expression And()
    {
        Expression expr = Equality();
        while (Match(TokenType.AND))
        {
            Token op = Previous();
            Expression right = Equality();
            expr = new Expression.LogicalExpression(expr, op, right);
        }
        return expr;
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
        return Call();
    }

    private Expression Call()
    {
        Expression expression = Primary();

        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
            {
                expression = FinishCall(expression);
            }
            else break;
        }
        return expression;
    }

    private Expression FinishCall(Expression callee)
    {
        List<Expression> arguments = new List<Expression>();
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }
                arguments.Add(Expr());
            } while (Match(TokenType.COMMA));
        }
        Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");
        return new Expression.CallExpression(callee, paren, arguments);
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
            return new Expression.LiteralExpression(Previous().literal);
        }
        if (Match(TokenType.IDENTIFIER))
        {
            return new Expression.VariableExpression(Previous());
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
        GameManager.Error(token, message);
        return new ParseError();
    }

    private void Synchronize()
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
