using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function : ICallable
{
    private readonly Statement.FunctionStatement declaration;
    private readonly Environment closure;
    private readonly List<SeeMMType> argumentTypes;
    private readonly SeeMMType returnType;

    public Function(Statement.FunctionStatement declaration, Environment closure,  SeeMMType returnType, List<SeeMMType> argumentTypes = null)
    {
        this.closure = closure;
        this.declaration = declaration;
        this.argumentTypes = argumentTypes;
        this.returnType = returnType;
    }

    public int Arity()
    {
        return declaration.parameters.Count;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        Environment environment = new Environment(closure);
        for (int i = 0; i < declaration.parameters.Count; i++) 
        {
            environment.Define(declaration.parameters[i].textValue, arguments[i]);
        }
        try
        {
            interpreter.ExecuteBlock(declaration.body, environment);
        }
        catch (Return returnValue)
        {
            return returnValue.value;
        }
        return null;
    }

    public List<SeeMMType> GetArgumentTypes()
    {
        return argumentTypes;
    }

    public SeeMMType GetReturnType()
    {
        return returnType;
    }
}
