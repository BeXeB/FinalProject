using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Turret turret;//TODO: Remove this

    private Vector2 movement;

    private void FixedUpdate()
    {
        rigidBody.velocity = movement * moveSpeed;
    }

    private void OnMove(InputValue inputValue)
    {
        movement = inputValue.Get<Vector2>();
    }

    private void OnInteract(InputValue inputValue)
    {
        turret.Interact();
    }
}
