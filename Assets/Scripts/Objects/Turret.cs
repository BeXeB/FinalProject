using System;
using System.Globalization;
using UnityEngine;

public class Turret : MonoBehaviour
{
    private CodeRunner codeRunner;

    private void Awake()
    {
        codeRunner = GetComponent<CodeRunner>();
    }

    private void Start()
    {
        for (var i = 0; i < codeRunner.extVariables.Count; i++)
        {
            var extVar = codeRunner.extVariables[i];
            if (extVar.textValue != "rotation") continue;
            extVar.onChange += (object value) =>
            {
                transform.rotation =
                    Quaternion.Euler(0, 0, Convert.ToSingle(value, CultureInfo.InvariantCulture));
            };
            codeRunner.extVariables[i] = extVar;
        }
    }
}