using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function : ICallable
{
    private readonly Statement.FunctionStatement declaration;
    private readonly Environment closure;
    public Function(Statement.FunctionStatement declaration, Environment closure)
    {
        this.closure = closure;
        this.declaration = declaration;
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
        //If we want to make sure that the function is called with the correct types
        return null;
    }
}
