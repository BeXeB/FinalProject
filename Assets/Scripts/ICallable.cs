using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        return (decimal)System.DateTime.Now.Ticks / System.TimeSpan.TicksPerSecond;
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}