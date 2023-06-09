using System;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private float range = 15f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private bool isSeekingTurret = false;
    private float fireCountdown = 0f;

    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private LayerMask layers;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private bool isEnabled = true;

    private void Awake()
    {
        codeRunner = GetComponent<CodeRunner>();
        codeRunner.SetCodeFolder("Turrets");
        codeRunner.SetCodeFileName($"Turret{name}CodeFile");
    }

    private void Start()
    {
        InvokeRepeating(nameof(UpdatePlayerPos), 0f, 0.5f);
        InvokeRepeating(nameof(FindEnemies), 0f, 0.5f);
        for (var i = 0; i < codeRunner.extVariables.Count; i++)
        {
            var extVar = codeRunner.extVariables[i];
            if (extVar.textValue != "rotation") continue;
            extVar.onChange += (previousValue, value) =>
            {
                if (isEnabled)
                {
                    transform.rotation =
                    Quaternion.Euler(0, 0, Convert.ToSingle(value, CultureInfo.InvariantCulture));
                }
            };
            codeRunner.extVariables[i] = extVar;
        }
    }

    private void UpdatePlayerPos()
    {
        if (isSeekingTurret && isEnabled)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag(playerTag);
            if (playerGO != null && Vector2.Distance(transform.position, playerGO.transform.position) <= range)
            {
                var target = playerGO.transform.position;
                List<object> playerCoords = new();
                playerCoords.Add(target.x);
                playerCoords.Add(target.y);
                codeRunner.UpdateExternalVariable(new Token
                { textValue = "playerCoords", literal = playerCoords, seeMMType = SeeMMType.FLOAT });
            }
        }
    }

    private void FindEnemies()
    {
        if (isSeekingTurret && isEnabled)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            List<object> enemyCoordsX = new();
            List<object> enemyCoordsY = new();
            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy != gameObject && Vector2.Distance(transform.position, enemy.transform.position) <= range)
                {
                    var target = enemy.transform.position;
                    enemyCoordsX.Add(target.x);
                    enemyCoordsY.Add(target.y);
                }
            }
            codeRunner.UpdateExternalVariable(new Token
            { textValue = "targetCoordsX", literal = enemyCoordsX, seeMMType = SeeMMType.FLOAT });
            codeRunner.UpdateExternalVariable(new Token
            { textValue = "targetCoordsY", literal = enemyCoordsY, seeMMType = SeeMMType.FLOAT });

        }
    }

    private void Update()
    {
        if (fireCountdown <= 0f && isEnabled)
        {
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.up, range);
            if (hit.collider != null && ((1 << hit.transform.gameObject.layer) | layers)  == layers)
            {
                Shoot();
                fireCountdown = 1f / fireRate;
            }
        }

        fireCountdown -= Time.deltaTime;
    }

    public void SetAim(float coordX, float coordY)
    {
        if (!isEnabled)
        {
            return;
        }
        float distanceX = coordX - transform.position.x;
        float distanceY = coordY - transform.position.y;
        float angle = Mathf.Atan2(distanceX, distanceY) * Mathf.Rad2Deg;
        Quaternion endRotation = Quaternion.AngleAxis(angle, Vector3.back);
        transform.rotation = Quaternion.Slerp(transform.rotation, endRotation, Time.deltaTime * turnSpeed);
        codeRunner.UpdateExternalVariable(new Token
        { textValue = "rotation", literal = transform.rotation.eulerAngles.z, seeMMType = SeeMMType.FLOAT });
    }

    public void SetTargetLayer(int layer)
    {
        layers = 1 << layer;
    }

    private void Shoot()
    {
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.ShootForward(bulletGO, transform);
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
        player.GetComponent<PlayerInput>().currentActionMap.Disable();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("CodeEditor"));
        var codeEditor = CodeEditor.instance;
        codeEditor.SetCodeRunner(codeRunner);
        codeEditor.ClearExtStuffText();
        var extVariables = codeRunner.GetExtVariables();
        codeEditor.SetExtVariables(extVariables.Item1, extVariables.Item2);
        codeEditor.SetExtFunctions(codeRunner.GetExtFunctions());
        codeEditor.SetGlobalFunctions(codeRunner.GetGlobalFunctions());
    }

    public void SetTurretState(bool state)
    {
        isEnabled = state;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        player = collision.gameObject;
        canBeInteracted = true;
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        player = null;
        canBeInteracted = false;
    }
}