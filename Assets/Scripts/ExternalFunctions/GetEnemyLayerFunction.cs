using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class GetEnemyLayerFunction : ExternalFunction
{
    private Turret turret;
    private void Awake()
    {
        turret = GetComponentInParent<Turret>();
        function = (List<object> args) =>
        {
            return turret.gameObject.layer;
        };
        argumentTypes = new List<SeeMMType> {};
        returnType = SeeMMType.INT;
    }
}