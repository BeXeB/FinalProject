using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class SetAimFunction : ExternalFunction
{
    private Turret turret;
    private void Awake()
    {
        turret = GetComponentInParent<Turret>();
        function = (List<object> args) =>
        {
            var coordX = Convert.ToSingle(args[0], CultureInfo.InvariantCulture);
            var coordY = Convert.ToSingle(args[1], CultureInfo.InvariantCulture);
            turret.SetAim(coordX, coordY);
            return null;
        };
        argumentTypes = new List<SeeMMType> {SeeMMType.FLOAT, SeeMMType.FLOAT};
        returnType = SeeMMType.VOID;
    }
}