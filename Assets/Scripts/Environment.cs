using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment
{
    readonly Environment enclosing;
    private readonly Dictionary<string, object> values = new Dictionary<string, object>();

    public Environment()
    {
        enclosing = null;
    }
    public Environment(Environment enclosing)
    {
        this.enclosing = enclosing;
    }
    public object Get(Token name)
    {
        if (values.ContainsKey(name.value))
        {
            object type;
            values.TryGetValue(name.value, out type);
            return type;
        }
        if (enclosing != null)
        {
            return enclosing.Get(name);
        }
        throw new RuntimeError(name, "Undefined variable '" + name.value + "'.");
    }

    public void Define(string name, object value)
    {
        values.Add(name, value);
    }

    public void Assign(Token name, object value)
    {
        if (values.ContainsKey(name.value))
        {
            values.Add(name.value, value);
            return;
        }
        if (enclosing != null)
        {
            enclosing.Assign(name, value);
            return;
        }
        throw new RuntimeError(name, "Undefined variable '" + name.value + "'.");
    }
}
