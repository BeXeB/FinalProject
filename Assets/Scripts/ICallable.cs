using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICallable
{
    public int Arity();
    public object Call(Interpreter interpreter, List<object> arguments);
}
