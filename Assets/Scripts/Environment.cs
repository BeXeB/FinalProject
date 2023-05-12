using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment
{
    readonly Environment enclosing;
    private readonly Dictionary<string, object> values = new();

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

    public object GetAt(int distance, string name)
    {
        object type;
        Ancestor(distance).values.TryGetValue(name, out type);
        return type;
    }

    public void AssignAt(int distance, Token name, object value)
    {
        Ancestor(distance).values[name.value] = value;
    }

    public void Define(string name, object value)
    {
        values.Add(name, value);
    }

    public void Assign(Token name, object value)
    {
        if (values.ContainsKey(name.value))
        {
            values[name.value] = value;
            return;
        }
        if (enclosing != null)
        {
            enclosing.Assign(name, value);
            return;
        }
        throw new RuntimeError(name, "Undefined variable '" + name.value + "'.");
    }

    Environment Ancestor(int distance)
    {
        Environment environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment.enclosing;
        }
        return environment;
    }
}
