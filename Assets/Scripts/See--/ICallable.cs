using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ICallable
{
    public int Arity();
    public object Call(Interpreter interpreter, List<object> arguments);
    public List<SeeMMType> GetArgumentTypes();
}

public class Clock : ICallable
{
    public int Arity()
    {
        return 0;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        return Convert.ToDecimal(Time.deltaTime);
    }

    public List<SeeMMType> GetArgumentTypes()
    {
        return new List<SeeMMType>();
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
        //TODO: Change this to the Code Editor console
        Debug.Log(arguments[0]);
        return null;
    }

    public List<SeeMMType> GetArgumentTypes()
    {
        return new List<SeeMMType> {SeeMMType.ANY};
    }
}

public class SeeMMExternalFunction : ICallable
{
    private int arity;
    private List<SeeMMType> argumentTypes;
    private Func<List<object>, object> unityEvent;
    public SeeMMExternalFunction(int arity, Func<List<object>, object> unityEvent, List<SeeMMType> argumentTypes)
    {
        this.arity = arity;
        this.argumentTypes = argumentTypes;
        this.unityEvent = unityEvent;
    }

    public int Arity()
    {
        return arity;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        return unityEvent.Invoke(arguments);
    }
    
    public List<SeeMMType> GetArgumentTypes()
    {
        return argumentTypes;
    }
}