using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorConsoleMessage : MonoBehaviour
{
    [SerializeField] float ttl = 3;
    [SerializeField] bool shouldDestroy = true;
    
    private void Update() {
        ttl -= Time.deltaTime;
        if (ttl < 0 && shouldDestroy)
        {
            Destroy(gameObject);
        }
    }
}
