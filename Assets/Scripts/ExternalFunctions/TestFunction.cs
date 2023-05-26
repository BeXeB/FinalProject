using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class TestFunction : ExternalFunction
{
    private void Awake()
    {
        function = (List<object> args) =>
        {
            return Convert.ToSingle(args[0], CultureInfo.InvariantCulture) + Convert.ToSingle(args[1], CultureInfo.InvariantCulture);
        };
        argumentTypes = new List<SeeMMType> {SeeMMType.FLOAT, SeeMMType.FLOAT};
    }
}