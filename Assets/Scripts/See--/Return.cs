using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Return : Exception
{
    public readonly object value;
    public Return(object value)
    {
        this.value = value;
    }
}
