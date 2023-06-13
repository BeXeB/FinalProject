using System;
using System.Collections.Generic;
using System.Globalization;

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
        if (values.ContainsKey(name.textValue))
        {
            object type;
            values.TryGetValue(name.textValue, out type);
            return type;
        }

        if (enclosing != null)
        {
            return enclosing.Get(name);
        }

        throw new RuntimeError(name, "Undefined variable '" + name.textValue + "'.");
    }

    public object GetAt(int distance, string name)
    {
        object type;
        Ancestor(distance).values.TryGetValue(name, out type);
        return type;
    }

    public void AssignAt(int distance, Token name, object value)
    {
        Ancestor(distance).Assign(name, value);
    }

    public void AssignAt(int distance, Token name, object value, object index)
    {
        Ancestor(distance).Assign(name, value, index);
    }

    public void Define(string name, object value)
    {
        values.Add(name, value);
    }

    public void Assign(Token name, object value)
    {
        if (values.ContainsKey(name.textValue))
        {
            values[name.textValue] = value;
            return;
        }

        if (enclosing != null)
        {
            enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, "Undefined variable '" + name.textValue + "'.");
    }

    public void Assign(Token name, object value, object index)
    {
        if (values.ContainsKey(name.textValue))
        {
            if (values[name.textValue] is not List<object> list)
                throw new RuntimeError(name, "Variable is not an array.");
            
            list[Convert.ToInt32(index, CultureInfo.InvariantCulture)] = value;
            return;
        }

        if (enclosing != null)
        {
            enclosing.Assign(name, value, index);
            return;
        }

        throw new RuntimeError(name, "Undefined variable '" + name.textValue + "'.");
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