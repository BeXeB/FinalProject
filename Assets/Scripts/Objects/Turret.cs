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

    private Transform target;

    [SerializeField] private float range = 15f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private bool isSeekingTurret = false;
    private float fireCountdown = 0f;

    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private void Awake()
    {
        codeRunner = GetComponent<CodeRunner>();
        codeRunner.SetCodeFolder("Turrets");
        codeRunner.SetCodeFileName($"Turret{name}CodeFile");
    }

    private void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
        // for (var i = 0; i < codeRunner.extVariables.Count; i++)
        // {
        //     var extVar = codeRunner.extVariables[i];
        //     if (extVar.textValue != "rotation") continue;
        //     extVar.onChange += (previousValue, value) =>
        //     {
        //         transform.rotation =
        //             Quaternion.Euler(0, 0, Convert.ToSingle(value, CultureInfo.InvariantCulture));
        //     };
        //     codeRunner.extVariables[i] = extVar;
        // }
    }

    private void UpdateTarget()
    {
        if (isSeekingTurret)
        {
            GameObject nearestEnemy = GameObject.FindGameObjectWithTag(playerTag);
            if (nearestEnemy != null && Vector2.Distance(transform.position, nearestEnemy.transform.position) <= range)
            {
                target = nearestEnemy.transform;
            }
            else
            {
                target = null;
            }
        }
    }

    private void Update()
    {
        if (isSeekingTurret)
        {
            if (target == null)
            {
                return;
            }

            float distanceX = target.position.x - transform.position.x;
            float distanceY = target.position.y - transform.position.y;
            float angle = Mathf.Atan2(distanceX, distanceY) * Mathf.Rad2Deg;

            Quaternion endRotation = Quaternion.AngleAxis(angle, Vector3.back);
            transform.rotation = Quaternion.Slerp(transform.rotation, endRotation, Time.deltaTime * turnSpeed);
            codeRunner.UpdateExternalVariable(new Token
                { textValue = "rotation", literal = transform.rotation.eulerAngles.z, seeMMType = SeeMMType.FLOAT });
        }

        if (fireCountdown <= 0f)
        {
            //Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    private void Shoot()
    {
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.ShootForward(bulletGO, transform, target, isSeekingTurret);
            Destroy(bulletGO, 5);
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
        codeEditor.SetGlobalFunctions(codeRunner.GetGlobalFunctions());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        player = collision.gameObject;
        canBeInteracted = true;
    }
}