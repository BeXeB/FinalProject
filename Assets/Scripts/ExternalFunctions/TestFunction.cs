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
            return Convert.ToDecimal(args[0], CultureInfo.InvariantCulture) + Convert.ToDecimal(args[1], CultureInfo.InvariantCulture);
        };
        argumentTypes = new List<SeeMMType> {SeeMMType.FLOAT, SeeMMType.FLOAT};
    }
}