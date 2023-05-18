using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Turret : MonoBehaviour, IInteractable
{

    private CodeRunner codeRunner;

    private GameObject player;

    private bool canBeInteracted = false;

    public string name;

    private void Awake()
    {
        codeRunner = GetComponent<CodeRunner>();
        codeRunner.SetCodeFolder("Turrets");
        codeRunner.SetCodeFileName($"Turret{name}CodeFile");
    }

    private void Start()
    {
        for (var i = 0; i < codeRunner.extVariables.Count; i++)
        {
            var extVar = codeRunner.extVariables[i];
            if (extVar.textValue != "rotation") continue;
            extVar.onChange += (previousValue, value) =>
            {
                transform.rotation =
                    Quaternion.Euler(0, 0, Convert.ToSingle(value, CultureInfo.InvariantCulture));
            };
            codeRunner.extVariables[i] = extVar;
        }
    }

    public void Interact()
    {
        if (canBeInteracted)
        {
            StartCoroutine(LoadEditor());
        }
    }
    
    private IEnumerator LoadEditor()
    {
        var sceneLoadOp = SceneManager.LoadSceneAsync("CodeEditor", LoadSceneMode.Additive);
        yield return new WaitUntil(() => sceneLoadOp.isDone);
        player.GetComponentInParent<PlayerInput>().currentActionMap.Disable();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("CodeEditor"));
        var codeEditor = CodeEditor.instance;
        codeEditor.SetCodeRunner(codeRunner);
        codeEditor.ClearExtStuffText();
        var extVariables = codeRunner.GetExtVariables();
        codeEditor.SetExtVariables(extVariables.Item1, extVariables.Item2);
        codeEditor.SetExtFunctions(codeRunner.GetExtFunctions());
        codeEditor.setGlobalFunctions(codeRunner.GetGlobalFunctions());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        player = collision.gameObject;
        canBeInteracted = true;
    }
}