using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resolver : Expression.IExpressionVisitor<object>, Statement.IStatementVisitor<object>
{
    private readonly Interpreter interpreter;
    private readonly Stack<Dictionary<string, bool>> scopes = new();
    private FunctionType currentFunction = FunctionType.NONE;

    public Resolver(Interpreter interpreter)
    {
        this.interpreter = interpreter;
    }

    private enum FunctionType
    {
        NONE,
        FUNCTION
    }

    public object VisitBlockStatement(Statement.BlockStatement statement)
    {
        BeginScope();
        Resolve(statement.statements);
        EndScope();
        return null;
    }

    public object VisitBoolStatement(Statement.BoolStatement statement)
    {
        Declare(statement.name);
        if (statement.initializer != null)
        {
            Resolve(statement.initializer);
        }
        Define(statement.name);
        return null;
    }

    public object VisitFloatStatement(Statement.FloatStatement statement)
    {
        Declare(statement.name);
        if (statement.initializer != null)
        {
            Resolve(statement.initializer);
        }
        Define(statement.name);
        return null;
    }

    public object VisitIntStatement(Statement.IntStatement statement)
    {
        Declare(statement.name);
        if (statement.initializer != null)
        {
            Resolve(statement.initializer);
        }
        Define(statement.name);
        return null;
    }

    public object VisitBreakStatement(Statement.BreakStatement statement)
    {
        EndScope();//TODO: Fix this shit(if we break in if statement, we dont want to break from if, but from the loop itself!!!!!)
        return null;
    }

    public object VisitContinueStatement(Statement.ContinueStatement statement)
    {
        throw new System.NotImplementedException();//TODO: This shit
    }

    public object VisitExpressionStatement(Statement.ExpressionStatement statement)
    {
        Resolve(statement.expression);
        return null;
    }

    public object VisitFunctionStatement(Statement.FunctionStatement statement)
    {
        Declare(statement.name);
        Define(statement.name);
        ResolveFunction(statement, FunctionType.FUNCTION);
        return null;
    }

    public object VisitIfStatement(Statement.IfStatement statement)
    {
        Resolve(statement.condition);
        Resolve(statement.thenBranch);
        if (statement.elseBranch != null)
        {
            Resolve(statement.elseBranch);
        }
        return null;
    }

    public object VisitReturnStatement(Statement.ReturnStatement statement)
    {
        if (currentFunction == FunctionType.NONE)
        {
            GameManager.Error(statement.keyword, "Can't return from top-level code.");
        }
        if (statement.value != null)
        {
            Resolve(statement.value);
        }
        return null;
    }

    public object VisitWhileStatement(Statement.WhileStatement statement)
    {
        Resolve(statement.condition);
        Resolve(statement.body);
        return null;
    }

    public object VisitAssignmentExpression(Expression.AssignmentExpression expression)
    {
        Resolve(expression.value);
        ResolveLocal(expression, expression.name);
        return null;
    }

    public object VisitBinaryExpression(Expression.BinaryExpression expression)
    {
        Resolve(expression.right);
        Resolve(expression.left);
        return null;
    }

    public object VisitCallExpression(Expression.CallExpression expression)
    {
        Resolve(expression.callee);
        foreach (Expression argument in expression.arguments)
        {
            Resolve(argument);
        }
        return null;
    }

    public object VisitGroupingExpression(Expression.GroupingExpression expression)
    {
        Resolve(expression.expression);
        return null;
    }

    public object VisitLiteralExpression(Expression.LiteralExpression expression)
    {
        return null;
    }

    public object VisitLogicalExpression(Expression.LogicalExpression expression)
    {
        Resolve(expression.left);
        Resolve(expression.right);
        return null;
    }

    public object VisitUnaryExpression(Expression.UnaryExpression expression)
    {
        Resolve(expression.expression);
        return null;
    }

    public object VisitVariableExpression(Expression.VariableExpression expression)
    {
        if (scopes.TryPeek(out Dictionary<string, bool> scope) &&
            scope.TryGetValue(expression.name.value, out bool value)
            && value == false)
        {
            GameManager.Error(expression.name, "Can't read local variable in its own initializer.");
        }
        ResolveLocal(expression, expression.name);
        return null;
    }

    public void Resolve(List<Statement> statements)
    {
        foreach (Statement statement in statements)
        {
            Resolve(statement);
        }
    }

    private void Resolve(Statement statement)
    {
        statement.Accept(this);
    }

    private void Resolve(Expression expression)
    {
        expression.Accept(this);
    }

    private void ResolveLocal(Expression expr, Token name)//TODO: Look at this later!!!
    {
        for (int i = scopes.Count - 1; i >= 0; i--)
        {
            if (scopes.ToArray()[i].ContainsKey(name.value))
            {
                interpreter.Resolve(expr, i);
                return;
            }
        }
    }

    private void ResolveFunction(Statement.FunctionStatement function, FunctionType type)
    {
        currentFunction = type;
        BeginScope();
        foreach (Token param in function.parameters)
        {
            Declare(param);
            Define(param);
        }
        Resolve(function.body);
        EndScope();
    }

    private void BeginScope()
    {
        scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        scopes.Pop();
    }

    private void Declare(Token name)
    {
        if (scopes.Count == 0)
        {
            return;
        }
        Dictionary<string, bool> scope = scopes.Peek();
        if (scope.ContainsKey(name.value))//TODO: Look at this, so no 2 variables with same type and name exist!!!!
        {
            GameManager.Error(name, "Already variable with this name in this scope.");
        }
        scope.Add(name.value, false);
    }

    private void Define(Token name)
    {
        if (scopes.Count == 0)
        {
            return;
        }
        scopes.Peek()[name.value] = true;
    }

    
}
