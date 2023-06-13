using System.Collections.Generic;

public class Parser
{
    private class ParseError : System.Exception { }

    private List<Token> tokens;
    private int current = 0;
    private int functions = 0;

    public void SetTokens(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    public List<Statement> Parse()
    {
        current = 0;
        functions = 0;
        List<Statement> statements = new List<Statement>();
        while (!IsAtEnd())
        {
            var declaration = Declaration();
            if (declaration is Statement.FunctionStatement)
            {
                statements.Insert(functions, declaration);
                functions++;
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
            if (Match(TokenType.INT)) return VariableDeclaration(SeeMMType.INT);
            if (Match(TokenType.FLOAT)) return VariableDeclaration(SeeMMType.FLOAT);
            if (Match(TokenType.BOOL)) return VariableDeclaration(SeeMMType.BOOL);
            if (Match(TokenType.VOID)) return VariableDeclaration(SeeMMType.VOID);
            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    private Statement.FunctionStatement Function(Token name, SeeMMType returnType)
    {
        List<Token> parameters = new List<Token>();
        List<SeeMMType> parameterTypes = new List<SeeMMType>();
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                //find type and then the identifier
                switch (Peek().type)
                {
                    case TokenType.INT:
                        parameterTypes.Add(SeeMMType.INT);
                        Advance();
                        if (Match(TokenType.LEFT_SQUAREBRACKET))
                        {
                            Consume(TokenType.RIGHT_SQUAREBRACKET, "Expect ']' after '['.");
                            parameterTypes[^1] = SeeMMType.INT_ARRAY;
                        }

                        break;
                    case TokenType.FLOAT:
                        parameterTypes.Add(SeeMMType.FLOAT);
                        Advance();
                        if (Match(TokenType.LEFT_SQUAREBRACKET))
                        {
                            Consume(TokenType.RIGHT_SQUAREBRACKET, "Expect ']' after '['.");
                            parameterTypes[^1] = SeeMMType.FLOAT_ARRAY;
                        }

                        break;
                    case TokenType.BOOL:
                        parameterTypes.Add(SeeMMType.BOOL);
                        Advance();
                        if (Match(TokenType.LEFT_SQUAREBRACKET))
                        {
                            Consume(TokenType.RIGHT_SQUAREBRACKET, "Expect ']' after '['.");
                            parameterTypes[^1] = SeeMMType.BOOL_ARRAY;
                        }

                        break;
                    default:
                        Error(Peek(), "Expect parameter type.");
                        break;
                }

                parameters.Add(
                    Consume(TokenType.IDENTIFIER, "Expect parameter name."));
            } while (Match(TokenType.COMMA));
        }

        Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
        Consume(TokenType.LEFT_BRACE, "Expect '{' before function body.");
        List<Statement> body = Block();
        return new Statement.FunctionStatement(name, parameters, body, parameterTypes, returnType);
    }

    private Statement VariableDeclaration(SeeMMType type)
    {
        if (Match(TokenType.LEFT_SQUAREBRACKET))
        {
            return ArrayDeclaration(type);
        }

        Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

        //check if it's a function
        if (Match(TokenType.LEFT_PAREN))
        {
            return Function(name , type);
        }

        Expression initializer = null;
        if (Match(TokenType.EQUAL))
        {
            initializer = Expr();
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
        switch (type)
        {
            case SeeMMType.INT:
                return new Statement.IntStatement(name, initializer);
            case SeeMMType.FLOAT:
                return new Statement.FloatStatement(name, initializer);
            case SeeMMType.BOOL:
                return new Statement.BoolStatement(name, initializer);
            default:
                return null;
        }
    }

    private Statement ArrayDeclaration(SeeMMType type)
    {
        type = type switch
        {
            SeeMMType.INT => SeeMMType.INT_ARRAY,
            SeeMMType.FLOAT => SeeMMType.FLOAT_ARRAY,
            SeeMMType.BOOL => SeeMMType.BOOL_ARRAY,
            _ => type
        };

        Consume(TokenType.RIGHT_SQUAREBRACKET, "Expect ']' after '['.");
        Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

        //check if it's a function
        if (Match(TokenType.LEFT_PAREN))
        {
            return Function(name ,type);
        }

        List<Expression> initializer = new();
        if (Match(TokenType.EQUAL))
        {
            Consume(TokenType.LEFT_BRACE, "Expect '{' after '='.");
            initializer.Add(Expr());
            while (Match(TokenType.COMMA))
            {
                initializer.Add(Expr());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after array initializer.");
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");

        return new Statement.ArrayStatement(name, name.seeMMType, initializer.ToArray(), initializer.Count);
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
        var keyword = Previous();
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        Expression condition = Expr();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");
        Statement thenBranch = Statement();
        Statement elseBranch = null;
        if (Match(TokenType.ELSE))
        {
            elseBranch = Statement();
        }

        return new Statement.IfStatement(condition, thenBranch, elseBranch, keyword);
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
        var keyword = Previous();
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        Expression condition = Expr();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
        Statement body = Statement();
        return new Statement.WhileStatement(condition, body, keyword);
    }

    private List<Statement> Block()
    {
        List<Statement> statements = new List<Statement>();
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            var declaration = Declaration();
            if (declaration is Statement.FunctionStatement)
            {
                statements.Insert(functions, declaration);
                functions++;
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

            if (Match(TokenType.LEFT_BRACE))
            {
                if (expression is Expression.VariableExpression variableExpression)
                {
                    var expressions = new List<Expression>();
                    var name = variableExpression.name;

                    expressions.Add(Assignment());
                    while (Match(TokenType.COMMA))
                    {
                        expressions.Add(Assignment());
                    }

                    if (Match(TokenType.RIGHT_BRACE))
                        return new Expression.ArrayAssignmentExpression(name, expressions.ToArray(), name.seeMMType);
                    Error(equals, "Expect '}' after array initializer.");
                }

                Error(equals, "Invalid assignment target.");
            }

            Expression value = Assignment();
            switch (expression)
            {
                case Expression.VariableExpression variableExpression:
                {
                    Token name = variableExpression.name;
                    return new Expression.AssignmentExpression(name, value, name.seeMMType);
                }
                case Expression.ArrayExpression arrayExpression:
                {
                    Token name = arrayExpression.name;
                    Expression index = arrayExpression.index;
                    return new Expression.AssignmentExpression(name, index, value, name.seeMMType);
                }
                default:
                    Error(equals, "Invalid assignment target.");
                    break;
            }
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
        
        if (Match(TokenType.LEFT_PAREN))
        {
            expression = FinishCall(expression);
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
            return new Expression.LiteralExpression(false, SeeMMType.BOOL);
        }

        if (Match(TokenType.TRUE))
        {
            return new Expression.LiteralExpression(true, SeeMMType.BOOL);
        }

        if (Match(TokenType.INT_NUMBER))
        {
            return new Expression.LiteralExpression(Previous().literal, SeeMMType.INT);
        }
        
        if (Match(TokenType.FLOAT_NUMBER))
        {
            return new Expression.LiteralExpression(Previous().literal, SeeMMType.FLOAT);
        }

        if (Match(TokenType.IDENTIFIER))
        {
            var name = Previous();
            if (Match(TokenType.LEFT_SQUAREBRACKET))
            {
                Expression index = Expr();
                Consume(TokenType.RIGHT_SQUAREBRACKET, "Expect ']' after index.");
                return new Expression.ArrayExpression(name, index);
            }

            return new Expression.VariableExpression(name);
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
                case TokenType.IF:
                case TokenType.BREAK:
                case TokenType.CONTINUE:
                case TokenType.RETURN:
                case TokenType.INT:
                case TokenType.FLOAT:
                case TokenType.BOOL:
                case TokenType.VOID:
                case TokenType.WHILE:
                    return;
            }

            Advance();
        }
    }
}