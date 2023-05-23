using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EditorConsole : MonoBehaviour
{
    [SerializeField] GameObject messagePrefab;
    [SerializeField] Transform messageParent;
    [SerializeField] Scrollbar scrollbar;

    public void Log(string text)
    {
        var newLog = Instantiate(messagePrefab, messageParent);
        newLog.GetComponent<TMP_Text>().text = text;
        scrollbar.value = 0;
    }

    public void Clear()
    {
        foreach (Transform child in messageParent)
        {
            Destroy(child.gameObject);
        }
    }
}
