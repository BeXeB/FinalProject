using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ICallable
{
    public int Arity();
    public object Call(Interpreter interpreter, List<object> arguments);
}

public class Clock : ICallable
{
    public int Arity()
    {
        return 0;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        return (decimal)DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
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
}

public class SeeMMExternalFunction : ICallable
{
    private int arity;
    private Func<List<object>, object> unityEvent;
    public SeeMMExternalFunction(int arity, Func<List<object>, object> unityEvent)
    {
        this.arity = arity;
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
}