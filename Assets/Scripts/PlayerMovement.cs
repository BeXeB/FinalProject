using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rigidBody;

    private Vector2 movement;

    private void FixedUpdate()
    {
        rigidBody.velocity = movement * moveSpeed;
    }

    private void OnMove(InputValue inputValue)
    {
        movement = inputValue.Get<Vector2>();
    }
}
