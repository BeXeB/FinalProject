using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ExternalFunction : MonoBehaviour
{
    public string functionName;
    public int arity;
    public List<SeeMMType> argumentTypes;
    public Func<List<object>, object> function = (List<object> _) => { return null; };
    public SeeMMType returnType;
}   