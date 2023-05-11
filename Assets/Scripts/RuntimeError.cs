using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeError : Exception
{
    readonly Token token;

    public RuntimeError(Token token, string message):base(message)
    {
        this.token = token;
    }
}
