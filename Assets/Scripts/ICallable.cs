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

public class ExternalFunction : ICallable
{
    private int arity;
    private Action<List<object>> unityEvent;
    public ExternalFunction(int arity, Action<List<object>> unityEvent)
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
        unityEvent.Invoke(arguments);
        return null;
    }
}