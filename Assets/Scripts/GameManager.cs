using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;
    Interpreter interpreter = new Interpreter();
    // Start is called before the first frame update
    void Start()
    {
        interpreter.InterpretCode(text.text);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
