using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
        Ancestor(distance).values[name.textValue] = value;
    }

    public void Define(string name, object value)
    {
        values.Add(name, value);
    }

    public void Assign(Token name, object value)
    {
        if (values.ContainsKey(name.textValue))
        {
            switch (name.seeMMType)
            {
                case TokenType.INT when value is not int && value is decimal valueAsDecimal && valueAsDecimal % 1 != 0:
                    throw new RuntimeError(name, "Cannot assign a non-int value to an int variable.");
                case TokenType.INT when value is int || value is decimal valueAsDecimal && valueAsDecimal % 1 == 0:
                    values[name.textValue] = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                    return;
                case TokenType.BOOL when value is not bool:
                    throw new RuntimeError(name, "Cannot assign a non-bool value to a bool variable.");
                case TokenType.FLOAT when value is not decimal && value is not int:
                    throw new RuntimeError(name, "Cannot assign a non-floating point value to a float variable.");
                default:
                    values[name.textValue] = value;
                    return;
            }
        }
        if (enclosing != null)
        {
            enclosing.Assign(name, value);
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
