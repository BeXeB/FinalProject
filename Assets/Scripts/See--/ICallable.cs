using System.Collections.Generic;
using UnityEngine;
using System;

public interface ICallable
{
    public int Arity();
    public object Call(Interpreter interpreter, List<object> arguments);
    public List<SeeMMType> GetArgumentTypes();
    public SeeMMType GetReturnType();
}

public class Clock : ICallable
{
    public int Arity()
    {
        return 0;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        return Convert.ToSingle(Time.deltaTime);
    }

    public List<SeeMMType> GetArgumentTypes()
    {
        return new List<SeeMMType>();
    }

    public SeeMMType GetReturnType()
    {
        return SeeMMType.FLOAT;
    }
}

public class Print : ICallable
{
    public int Arity()
    {
        return 1;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        if (CodeEditor.instance)
        {
            CodeEditor.instance.LogError(arguments[0].ToString());
        }

        return null;
    }

    public List<SeeMMType> GetArgumentTypes()
    {
        return new List<SeeMMType> { SeeMMType.ANY };
    }

    public SeeMMType GetReturnType()
    {
        return SeeMMType.VOID;
    }
}

public class SeeMMExternalFunction : ICallable
{
    private readonly int arity;
    private readonly List<SeeMMType> argumentTypes;
    private readonly Func<List<object>, object> unityEvent;
    private readonly SeeMMType returnType;

    public SeeMMExternalFunction(int arity, Func<List<object>, object> unityEvent, List<SeeMMType> argumentTypes, SeeMMType returnType)
    {
        this.arity = arity;
        this.argumentTypes = argumentTypes;
        this.unityEvent = unityEvent;
        this.returnType = returnType;
    }

    public int Arity()
    {
        return arity;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        return unityEvent?.Invoke(arguments);
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

public class SizeOf : ICallable
{
    public int Arity()
    {
        return 1;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        if (arguments[0] is List<object> list)
        {
            return list.Count;
        }

        return 0;
    }

    public List<SeeMMType> GetArgumentTypes()
    {
        return new List<SeeMMType> { SeeMMType.ANY };
    }

    public SeeMMType GetReturnType()
    {
        return SeeMMType.INT;
    }
}