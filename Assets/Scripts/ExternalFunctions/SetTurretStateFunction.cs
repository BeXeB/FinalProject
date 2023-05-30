using System;
using System.Collections.Generic;
using System.Globalization;

public class SetTurretStateFunction : ExternalFunction
{
    private Turret turret;
    private void Awake()
    {
        turret = GetComponentInParent<Turret>();
        function = (List<object> args) =>
        {
            turret.SetTurretState(Convert.ToBoolean(args[0]));
            return null;
        };
        argumentTypes = new List<SeeMMType> {SeeMMType.BOOL};
    }
}