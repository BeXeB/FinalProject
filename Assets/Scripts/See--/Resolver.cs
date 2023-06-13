using System.Collections.Generic;
using System.Linq;

public class Resolver : Expression.IExpressionVisitor<SeeMMType>, Statement.IStatementVisitor<object>
{
    private Interpreter interpreter;
    private readonly Stack<Dictionary<string, bool>> scopes = new();
    private (FunctionType type, Statement.FunctionStatement expression) currentFunction = (FunctionType.NONE, null);
    private bool isInLoop = false;
    private Dictionary<string, List<SeeMMType>> functions = new();

    public void SetInterpreter(Interpreter interpreter)
    {
        this.interpreter = interpreter;
    }

    public void Clear()
    {
        functions.Clear();
        isInLoop = false;
        currentFunction = (FunctionType.NONE, null);
        scopes.Clear();
    }
    
    public void AddExtAndGlobalFunctions(Dictionary<string, List<SeeMMType>> extFunctions)
    {
        foreach (var extFunction in extFunctions)
        {
            functions.Add(extFunction.Key, extFunction.Value);
        }
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
            var valueType = Resolve(statement.initializer);
            if (valueType is not SeeMMType.BOOL)
            {
                GameManager.Error(statement.name, $"Can't assign {valueType} to bool.");
            }
        }
        Define(statement.name);
        return null;
    }

    public object VisitFloatStatement(Statement.FloatStatement statement)
    {
        Declare(statement.name);
        if (statement.initializer != null)
        {
            var valueType = Resolve(statement.initializer);
            if (valueType is not SeeMMType.FLOAT && valueType is not SeeMMType.INT)
            {
                GameManager.Error(statement.name, $"Can't assign {valueType} to float.");
            }
        }
        Define(statement.name);
        return null;
    }

    public object VisitIntStatement(Statement.IntStatement statement)
    {
        Declare(statement.name);
        if (statement.initializer != null)
        {
            var valueType = Resolve(statement.initializer);
            if (valueType is not SeeMMType.INT)
            {
                GameManager.Error(statement.name, $"Can't assign {valueType} to int.");
            }
        }
        Define(statement.name);
        return null;
    }
    
    public object VisitArrayStatement(Statement.ArrayStatement statement)
    {
        Declare(statement.name);
        if (statement.initializer != null)
        {
            foreach (var expression in statement.initializer)
            {
                var valueType = Resolve(expression);
                switch (statement.type)
                {
                    case SeeMMType.INT_ARRAY:
                        if (valueType is not SeeMMType.INT)
                        {
                            GameManager.Error(statement.name, $"Can't assign {valueType} to int array.");
                        }
                        break;
                    case SeeMMType.FLOAT_ARRAY:
                        if (valueType is not SeeMMType.FLOAT && valueType is not SeeMMType.INT)
                        {
                            GameManager.Error(statement.name, $"Can't assign {valueType} to float array.");
                        }
                        break;
                    case SeeMMType.BOOL_ARRAY:
                        if (valueType is not SeeMMType.BOOL)
                        {
                            GameManager.Error(statement.name, $"Can't assign {valueType} to bool array.");
                        }
                        break;
                }
            }
        }
        Define(statement.name);
        return null;
    }

    public object VisitBreakStatement(Statement.BreakStatement statement)
    {
        if (!isInLoop)
        {
            GameManager.Error(statement.keyword, "Can't break when not inside loop.");
        }
        return null;
    }

    public object VisitContinueStatement(Statement.ContinueStatement statement)
    {
        if (!isInLoop)
        {
            GameManager.Error(statement.keyword, "Can't continue when not inside loop.");
        }
        return null;
    }

    public object VisitExpressionStatement(Statement.ExpressionStatement statement)
    {
        Resolve(statement.expression);
        return null;
    }

    public object VisitFunctionStatement(Statement.FunctionStatement statement)
    {
        if (currentFunction.type != FunctionType.NONE)
        {
            GameManager.Error(statement.name, "Can't nest functions.");
        }
        Declare(statement.name);
        Define(statement.name);
        ResolveFunction(statement, FunctionType.FUNCTION);
        return null;
    }

    public object VisitIfStatement(Statement.IfStatement statement)
    {
        //TODO check if condition is bool
        var condType = Resolve(statement.condition);
        if (condType is not SeeMMType.BOOL)
        {
            GameManager.Error(statement.keyword, "Condition must be a bool.");
        }
        Resolve(statement.thenBranch);
        if (statement.elseBranch != null)
        {
            Resolve(statement.elseBranch);
        }
        return null;
    }

    public object VisitReturnStatement(Statement.ReturnStatement statement)
    {
        if (currentFunction.type == FunctionType.NONE)
        {
            GameManager.Error(statement.keyword, "Can't return from top-level code.");
        }

        var valueType = SeeMMType.NONE;
        if (statement.value != null)
        {
           valueType = Resolve(statement.value);
        }
        
        var returnType = currentFunction.expression.returnType;

        if (valueType != returnType)
        {
            GameManager.Error(statement.keyword, $"Can't return {valueType} from {returnType} function.");
        }
        
        return null;
    }

    public object VisitWhileStatement(Statement.WhileStatement statement)
    {
        isInLoop = true;
        var condType = Resolve(statement.condition);
        
        if (condType is not SeeMMType.BOOL)
        {
            GameManager.Error(statement.keyword, "Condition must be a bool.");
        }
        
        Resolve(statement.body);
        isInLoop = false;
        return null;
    }

    public SeeMMType VisitAssignmentExpression(Expression.AssignmentExpression expression)
    {
        var valueType = Resolve(expression.value);
        
        switch (expression.type)
        {
            case SeeMMType.INT:
                if (valueType is not SeeMMType.INT)
                {
                    GameManager.Error(expression.name, $"Can't assign {valueType} to int.");
                }
                break;
            case SeeMMType.FLOAT:
                if (valueType is not SeeMMType.FLOAT && valueType is not SeeMMType.INT)
                {
                    GameManager.Error(expression.name, $"Can't assign {valueType} to float.");
                }
                break;
            case SeeMMType.BOOL:
                if (valueType is not SeeMMType.BOOL)
                {
                    GameManager.Error(expression.name, $"Can't assign {valueType} to bool.");
                }
                break;
        }
        
        ResolveLocal(expression, expression.name);
        return expression.type;
    }

    public SeeMMType VisitArrayAssignmentExpression(Expression.ArrayAssignmentExpression expression)
    {
        foreach (var value in expression.values)
        {
            var valueType = Resolve(value);
            switch (expression.type)
            {
                case SeeMMType.INT_ARRAY:
                    if (valueType is not SeeMMType.INT)
                    {
                        GameManager.Error(expression.name, $"Can't assign {valueType} to int array.");
                    }
                    break;
                case SeeMMType.FLOAT_ARRAY:
                    if (valueType is not SeeMMType.FLOAT && valueType is not SeeMMType.INT)
                    {
                        GameManager.Error(expression.name, $"Can't assign {valueType} to float array.");
                    }
                    break;
                case SeeMMType.BOOL_ARRAY:
                    if (valueType is not SeeMMType.BOOL)
                    {
                        GameManager.Error(expression.name, $"Can't assign {valueType} to bool array.");
                    }
                    break;
            }
        }
        ResolveLocal(expression, expression.name);
        return expression.type;
    }

    public SeeMMType VisitBinaryExpression(Expression.BinaryExpression expression)
    {
        var rightType = Resolve(expression.right);
        var leftType = Resolve(expression.left);

        switch (expression.op.type)
        {
            case TokenType.EQUAL_EQUAL:
            case TokenType.NOT_EQUAL:
                switch (rightType)
                {
                    case SeeMMType.BOOL when leftType is SeeMMType.BOOL:
                    case SeeMMType.INT or SeeMMType.FLOAT when leftType is SeeMMType.INT or SeeMMType.FLOAT:
                        return SeeMMType.BOOL;
                    case SeeMMType.BOOL:
                    case SeeMMType.INT or SeeMMType.FLOAT:
                        GameManager.Error(expression.op, $"Operator can't be applied to {rightType} and {leftType}.");
                        break;
                }
                break;
            
            case TokenType.GREATER:
            case TokenType.GREATER_EQUAL:
            case TokenType.LESS: 
            case TokenType.LESS_EQUAL:
                if (rightType is SeeMMType.INT or SeeMMType.FLOAT && leftType is SeeMMType.INT or SeeMMType.FLOAT)
                {
                    return SeeMMType.BOOL;
                }
                GameManager.Error(expression.op, "Operands must be numbers.");
                break;
            
            case TokenType.MINUS: 
            case TokenType.PLUS:
            case TokenType.SLASH: 
            case TokenType.STAR: 
            case TokenType.MOD:
                switch (rightType)
                {
                    case SeeMMType.INT when leftType is SeeMMType.INT:
                        return SeeMMType.INT;
                    case SeeMMType.FLOAT when leftType is SeeMMType.FLOAT or SeeMMType.INT:
                    case SeeMMType.INT when leftType is SeeMMType.FLOAT:
                        return SeeMMType.FLOAT;
                }
                GameManager.Error(expression.op, $"Operator can't be applied to {rightType} and {leftType}.");
                break;
        }

        return SeeMMType.BOOL;
    }

    public SeeMMType VisitCallExpression(Expression.CallExpression expression)
    {
        if (!functions.TryGetValue(((Expression.VariableExpression)expression.callee).name.textValue, out var func))
        {
            GameManager.Error(expression.paren, "Undefined function.");
            return SeeMMType.NONE;
        }

        var returnType = Resolve(expression.callee);
        for (var index = 0; index < expression.arguments.Count; index++)
        {
            var argument = expression.arguments[index];
            var argType = Resolve(argument);
            if (func[index] != SeeMMType.ANY && argType != func[index])
            {
                GameManager.Error(expression.paren, $"Expected {func[index]} but got {argType}.");
            }
        }
        
        return returnType;
    }

    public SeeMMType VisitGroupingExpression(Expression.GroupingExpression expression)
    {
        return Resolve(expression.expression);;
    }

    public SeeMMType VisitLiteralExpression(Expression.LiteralExpression expression)
    {
        return expression.type;
    }

    public SeeMMType VisitLogicalExpression(Expression.LogicalExpression expression)
    {
        Resolve(expression.left);
        Resolve(expression.right);
        return SeeMMType.BOOL;
    }

    public SeeMMType VisitUnaryExpression(Expression.UnaryExpression expression)
    {
        var expressionType = Resolve(expression.expression);
        return expressionType;
    }

    public SeeMMType VisitVariableExpression(Expression.VariableExpression expression)
    {
        if (scopes.TryPeek(out Dictionary<string, bool> scope) &&
            scope.TryGetValue(expression.name.textValue, out bool value)
            && value == false)
        {
            GameManager.Error(expression.name, "Can't read local variable in its own initializer.");
        }
        ResolveLocal(expression, expression.name);
        return expression.name.seeMMType;
    }
    
    public SeeMMType VisitArrayExpression(Expression.ArrayExpression expression)
    {
        if (scopes.TryPeek(out Dictionary<string, bool> scope) &&
            scope.TryGetValue(expression.name.textValue, out bool value)
            && value == false)
        {
            GameManager.Error(expression.name, "Can't read local variable in its own initializer.");
        }

        var indexType = expression.index.Accept(this);
        
        if (indexType != SeeMMType.INT)
        {
            GameManager.Error(expression.name, "Index must be an integer.");
        }
        
        ResolveLocal(expression, expression.name);

        return expression.name.seeMMType switch 
        {
            SeeMMType.INT_ARRAY => SeeMMType.INT,
            SeeMMType.FLOAT_ARRAY => SeeMMType.FLOAT,
            SeeMMType.BOOL_ARRAY => SeeMMType.BOOL,
            _ => SeeMMType.NONE
        };
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

    private SeeMMType Resolve(Expression expression)
    {
        return expression.Accept(this);
    }

    private void ResolveLocal(Expression expr, Token name)
    {
        for (int i = scopes.Count - 1; i >= 0; i--)
        {
            if (scopes.ToArray()[i].ContainsKey(name.textValue))
            {
                interpreter.Resolve(expr, i);
                return;
            }
        }
    }

    private void ResolveFunction(Statement.FunctionStatement function, FunctionType type)
    {
        currentFunction.type = type;
        currentFunction.expression = function;
        BeginScope();
        foreach (Token param in function.parameters)
        {
            Declare(param);
            Define(param);
        }
        Resolve(function.body);
        EndScope();
        currentFunction.type = FunctionType.NONE;
        currentFunction.expression = null;
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
        
        foreach (var sc in scopes)
        {
            if (sc.ContainsKey(name.textValue))
            {
                GameManager.Error(name, "Variable with this name already exists in this scope.");
            }
        }
        
        scope.Add(name.textValue, false);
    }

    private void Define(Token name)
    {
        if (scopes.Count == 0)
        {
            return;
        }
        scopes.Peek()[name.textValue] = true;
    }
}
