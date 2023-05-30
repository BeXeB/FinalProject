using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class SetTargetFunction : ExternalFunction
{
    private Turret turret;
    private void Awake()
    {
        turret = GetComponentInParent<Turret>();
        function = (List<object> args) =>
        {
            var layer = Convert.ToInt32(args[0], CultureInfo.InvariantCulture);
            turret.SetTargetLayer(layer);
            return null;
        };
        argumentTypes = new List<SeeMMType> {SeeMMType.INT};
    }
}