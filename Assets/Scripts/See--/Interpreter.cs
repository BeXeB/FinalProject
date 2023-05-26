using System.Collections.Generic;
using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

public class Interpreter : Expression.IExpressionVisitor<object>, Statement.IStatementVisitor<object>
{
    private Environment globals;
    private Environment environment;
    private readonly Dictionary<Expression, int> locals = new();
    private readonly Dictionary<string, ICallable> globalFunctions = new();

    public Interpreter(Dictionary<string,SeeMMExternalFunction> externalFunctions = null, List<Token> externalVariables = null)
    {
        InitGlobals(externalFunctions, externalVariables);
    }

    public void UpdateExternalVariable(Token name, object value)
    {
        globals.Assign(name, value);
    }

    public void InitGlobals(Dictionary<string,SeeMMExternalFunction> externalFunctions = null, List<Token> externalVariables = null)
    {
        globals = new Environment();
        globalFunctions.Clear();
        
        var clock = new Clock();
        globals.Define("deltatime", clock);
        globalFunctions.Add("deltatime", clock);
        var print = new Print();
        globals.Define("print", print);
        globalFunctions.Add("print", print);
        
        if (externalFunctions is not null)
        {
            foreach (var function in externalFunctions)
            {
                globals.Define(function.Key, function.Value);
            }
        }

        if (externalVariables is not null)
        {
            foreach (var variable in externalVariables)
            {
                globals.Define(variable.textValue, variable.literal);
            }
        }
        
        environment = globals;
    }

    public void InterpretCode(List<Statement> statements)
    {
        try
        {
            foreach (Statement statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError error)
        {
            GameManager.RuntimeError(error);
        }
    }

    private void Execute(Statement statement)
    {
        statement.Accept(this);
    }    

    public void ExecuteBlock(List<Statement> statements, Environment environment)
    {
        Environment previous = this.environment;
        try
        {
            this.environment = environment;
            foreach (Statement statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            this.environment = previous;
        }
    }

    public void Resolve(Expression expression, int depth)
    {
        locals.Add(expression, depth);
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
                CheckNumberOperands(left, right, expression.op);
                return Convert.ToDecimal(left, CultureInfo.InvariantCulture) > Convert.ToDecimal(right, CultureInfo.InvariantCulture);
            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(left, right, expression.op);
                return Convert.ToDecimal(left, CultureInfo.InvariantCulture) >= Convert.ToDecimal(right, CultureInfo.InvariantCulture);
            case TokenType.LESS:
                CheckNumberOperands(left, right, expression.op);
                return Convert.ToDecimal(left, CultureInfo.InvariantCulture) < Convert.ToDecimal(right, CultureInfo.InvariantCulture);
            case TokenType.LESS_EQUAL:
                CheckNumberOperands(left, right, expression.op);
                return Convert.ToDecimal(left, CultureInfo.InvariantCulture) <= Convert.ToDecimal(right, CultureInfo.InvariantCulture);
            case TokenType.MINUS:
                CheckNumberOperands(left, right, expression.op);
                if (CheckIntegerOperands(left, right))
                {
                    return Convert.ToInt32(left, CultureInfo.InvariantCulture) - Convert.ToInt32(right, CultureInfo.InvariantCulture);
                }
                return Convert.ToDecimal(left, CultureInfo.InvariantCulture) - Convert.ToDecimal(right, CultureInfo.InvariantCulture);
            case TokenType.PLUS:
                CheckNumberOperands(left, right, expression.op);
                if (CheckIntegerOperands(left, right))
                {
                    return Convert.ToInt32(left, CultureInfo.InvariantCulture) + Convert.ToInt32(right, CultureInfo.InvariantCulture);
                }
                return Convert.ToDecimal(left, CultureInfo.InvariantCulture) + Convert.ToDecimal(right, CultureInfo.InvariantCulture);
            case TokenType.SLASH:
                CheckNumberOperands(left, right, expression.op);
                if (Convert.ToDecimal(right, CultureInfo.InvariantCulture) == 0)
                {
                    throw new RuntimeError(expression.op, "Division by zero.");
                }
                if (CheckIntegerOperands(left, right))
                {
                    return Convert.ToInt32(left, CultureInfo.InvariantCulture) / Convert.ToInt32(right, CultureInfo.InvariantCulture);
                }
                return Convert.ToDecimal(left, CultureInfo.InvariantCulture) / Convert.ToDecimal(right, CultureInfo.InvariantCulture);
            case TokenType.STAR:
                CheckNumberOperands(left, right, expression.op);
                if (CheckIntegerOperands(left, right))
                {
                    return Convert.ToInt32(left, CultureInfo.InvariantCulture) * Convert.ToInt32(right, CultureInfo.InvariantCulture);
                }
                return Convert.ToDecimal(left, CultureInfo.InvariantCulture) * Convert.ToDecimal(right, CultureInfo.InvariantCulture);
            case TokenType.MOD:
                CheckNumberOperands(left, right, expression.op);
                if (CheckIntegerOperands(left, right))
                {
                    return Convert.ToInt32(left, CultureInfo.InvariantCulture) % Convert.ToInt32(right, CultureInfo.InvariantCulture);
                }
                return Convert.ToDecimal(left, CultureInfo.InvariantCulture) % Convert.ToDecimal(right, CultureInfo.InvariantCulture);
        }
        return null;
    }

    public object VisitCallExpression(Expression.CallExpression expression)
    {
        object callee = Evaluate(expression.callee);
        List<object> arguments = new List<object>();
        foreach (Expression argument in expression.arguments)
        {
            arguments.Add(Evaluate(argument));
        }

        if (callee is not ICallable function)
        {
            throw new RuntimeError(expression.paren, "Can only call functions.");
        }

        if (arguments.Count != function.Arity())
        {
            throw new RuntimeError(expression.paren, "Expected " + function.Arity() + " arguments but got " + arguments.Count + ".");
        }
        return function.Call(this, arguments);
    }

    public object VisitAssignmentExpression(Expression.AssignmentExpression expression)
    {
        object value = Evaluate(expression.value);
        object index = null;

        if (expression.index == null)
        {
            if (locals.TryGetValue(expression, out int distance))
            {
                environment.AssignAt(distance, expression.name, value);
            }
            else
            {
                globals.Assign(expression.name, value);
            }
        }
        else
        {
            index = Evaluate(expression.index);
            if (locals.TryGetValue(expression, out int distance))
            {
                environment.AssignAt(distance, expression.name, value, index);
            }
            else
            {
                globals.Assign(expression.name, value, index);
            }
        }
        // environment.Assign(expression.name, value);
        return value;
    }

    public object VisitArrayAssignmentExpression(Expression.ArrayAssignmentExpression expression)
    {
        List<object> array = new ();
        if (expression.values != null)
        {
            foreach (var expr in expression.values)
            {
                var value = Evaluate(expr);
                //check if value is the same type as the array
                switch (expression.type)
                {
                    case SeeMMType.INT when value is not int && value is decimal valueAsDecimal && valueAsDecimal % 1 != 0:
                        throw new RuntimeError(expression.name, "Initializer must be an integer.");
                    case SeeMMType.FLOAT when value is not decimal && value is not int:
                        throw new RuntimeError(expression.name, "Initializer must be a floating point number.");
                    case SeeMMType.BOOL when value is not bool:
                        throw new RuntimeError(expression.name, "Initializer must be a boolean.");
                }
                array.Add(value);
            }
        }
        if (locals.TryGetValue(expression, out int distance))
        {
            environment.AssignAt(distance, expression.name, array);
        }
        else
        {
            globals.Assign(expression.name, array);
        }

        return array;
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

        if ((expression.op.type == TokenType.OR) && (IsTruthy(left))) 
        {
            return left;
        }
        else if (!IsTruthy(left)) 
        {
            return left;
        }

        return Evaluate(expression.right);
    }

    public object VisitVariableExpression(Expression.VariableExpression expression)
    {
        return LookUpVariable(expression.name, expression);
    }

    public object VisitArrayExpression(Expression.ArrayExpression expression)
    {
        var array = LookUpVariable(expression.name, expression);
        if (array is not List<object> list)
        {
            throw new RuntimeError(expression.name, "Variable is not an array.");
        }
        var index = Evaluate(expression.index);
        if (index is not int && !(index is decimal indexAsDecimal && indexAsDecimal % 1 == 0))
        {
            throw new RuntimeError(expression.name, "Index must be an integer.");
        }

        var intIndex = Convert.ToInt32(index, CultureInfo.InvariantCulture);
        
        if (intIndex < 0 || intIndex >= list.Count)
        {
            throw new RuntimeError(expression.name, "Index out of bounds.");
        }
        return list[intIndex];
    }

    public object VisitUnaryExpression(Expression.UnaryExpression expression)
    {
        object right = Evaluate(expression.expression);

        switch (expression.op.type) 
        {
            case TokenType.NOT:
                return !IsTruthy(right);
            case TokenType.MINUS:
                CheckNumberOperand(right, expression.op);
                return -Convert.ToDecimal(right);
        }
        return null;
    }
    
    private bool CheckIntegerOperands(object left, object right)
    {
        return 
            (left is int || left is decimal leftAsDecimal && leftAsDecimal % 1 == 0) && 
            (right is int || right is decimal rightAsDecimal && rightAsDecimal % 1 == 0);
    }

    private void CheckNumberOperand(object operand, Token op)
    {
        if (operand is decimal or int) return;
        throw new RuntimeError(op, "Operand must be a number.");
    }
    private void CheckNumberOperands(object left, object right,Token op)
    {
        if (left is decimal or int && right is decimal or int) return;
        throw new RuntimeError(op, "Operands must be numbers.");
    }
    
    private bool IsEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }
    public object Evaluate(Expression expr)
    {
        return expr.Accept(this);
    }

    private bool IsTruthy(object obj)
    {
        if (obj == null) return false;
        if (obj is bool) return Convert.ToBoolean(obj);
        return true;
    }

    private object LookUpVariable(Token name, Expression expression)
    {
        if (locals.TryGetValue(expression, out int distance))
        {
            return environment.GetAt(distance, name.textValue);
        }
        else
        {
            return globals.Get(name);
        }
    }
    
    public object LookUpVariable(Token name)
    {
        if (locals.TryGetValue(new Expression.VariableExpression(name), out int distance))
        {
            return environment.GetAt(distance, name.textValue);
        }
        else
        {
            return globals.Get(name);
        }
    }

    public object VisitBlockStatement(Statement.BlockStatement statement)
    {
        ExecuteBlock(statement.statements, new Environment(environment));
        return null;
    }

    public object VisitExpressionStatement(Statement.ExpressionStatement statement)
    {
        Evaluate(statement.expression);
        return null;
    }

    public object VisitFunctionStatement(Statement.FunctionStatement statement)
    {
        Function function = new Function(statement, environment);
        environment.Define(statement.name.textValue, function);
        return null;
    }

    public object VisitIfStatement(Statement.IfStatement statement)
    {
        if (IsTruthy(Evaluate(statement.condition)))
        {
            Execute(statement.thenBranch);
        }
        else if (statement.elseBranch != null)
        {
            Execute(statement.elseBranch);
        }
        return null;
    }

    public object VisitReturnStatement(Statement.ReturnStatement statement)
    {
        object value = null;
        if (statement.value != null) value = Evaluate(statement.value);
        throw new Return(value);
    }

    public object VisitBreakStatement(Statement.BreakStatement statement)
    {
        throw new Break();
    }

    public object VisitContinueStatement(Statement.ContinueStatement statement)
    {
        throw new Continue();
    }

    public object VisitIntStatement(Statement.IntStatement statement)
    {
        object value = null;
        if (statement.initializer != null)
        {
            value = Evaluate(statement.initializer);
        }
        if (value is not int && value is decimal leftAsDecimal && leftAsDecimal % 1 != 0)
        {
            throw new RuntimeError(statement.name, "Initializer must be an integer.");
        }
        value = Convert.ToInt32(value, CultureInfo.InvariantCulture);
        environment.Define(statement.name.textValue, value);
        return null;
    }

    public object VisitFloatStatement(Statement.FloatStatement statement)
    {
        object value = null;
        if (statement.initializer != null)
        {
            value = Evaluate(statement.initializer);
        }
        if (value is not decimal)
        {
            throw new RuntimeError(statement.name, "Initializer must be a floating point number.");
        }
        environment.Define(statement.name.textValue, value);
        return null;
    }

    public object VisitBoolStatement(Statement.BoolStatement statement)
    {
        object value = null;
        if (statement.initializer != null)
        {
            value = Evaluate(statement.initializer);
        }
        if (value is not bool)
        {
            throw new RuntimeError(statement.name, "Initializer must be a boolean.");
        }
        environment.Define(statement.name.textValue, value);
        return null;
    }

    public object VisitArrayStatement(Statement.ArrayStatement statement)
    {
        List<object> array = new ();
        if (statement.initializer != null)
        {
            foreach (var initializer in statement.initializer)
            {
                var value = Evaluate(initializer);
                //check if value is the same type as the array
                switch (statement.type)
                {
                    case SeeMMType.INT when value is not int && value is decimal valueAsDecimal && valueAsDecimal % 1 != 0:
                        throw new RuntimeError(statement.name, "Initializer must be an integer.");
                    case SeeMMType.FLOAT when value is not decimal && value is not int:
                        throw new RuntimeError(statement.name, "Initializer must be a floating point number.");
                    case SeeMMType.BOOL when value is not bool:
                        throw new RuntimeError(statement.name, "Initializer must be a boolean.");
                }
                array.Add(value);
            }
        }
        environment.Define(statement.name.textValue, array);
        return null;
    }

    public object VisitWhileStatement(Statement.WhileStatement statement)
    {
        try
        {
            while (IsTruthy(Evaluate(statement.condition)))
            {
                try
                {
                    Execute(statement.body);
                }
                catch (Continue) { continue; }
            }
            return null;
        }
        catch (Break)
        {
            return null;
        }
    }

    public Environment GetGlobals()
    {
        return globals;
    }

    public Dictionary<string,ICallable> GetGlobalFunctions()
    {
        return globalFunctions;
    }
}
