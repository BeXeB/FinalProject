using System;
using System.Collections.Generic;
using System.Globalization;

public class GetIntruderLayerFunction : ExternalFunction
{
    private PlayerMovement player;
    private void Awake()
    {
        player = FindObjectOfType<PlayerMovement>();
        function = (List<object> args) =>
        {
            return player.gameObject.layer;
        };
        argumentTypes = new List<SeeMMType> {};
    }
}