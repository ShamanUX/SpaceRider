using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float thrustForce = 5f;
    public float rotationSpeed = 300f;
    public float maxVelocity = 10f;
    public float dragInSpace = 0.5f;

    [Header("Visual Effects")]
    public ParticleSystem thrustParticles;

    private Rigidbody2D rb;
    private bool isThrusting = false;

    public GameObject afterBurner;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = dragInSpace; // Small drag to prevent infinite movement
    }

    void Update()
    {
        HandleInput();
        HandleThrustEffects();
    }

    void HandleInput()
    {
        // Rotation with A/D or Left/Right arrows
        float rotation = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            rotation = 1f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            rotation = -1f;
        }

        // Apply rotation
        transform.Rotate(0, 0, rotation * rotationSpeed * Time.deltaTime);

        // Thrust with W/Up arrow
        isThrusting = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
    }

    void FixedUpdate()
    {
        
        if (isThrusting)
        {
            afterBurner.SetActive(true);
            // Apply thrust in the direction the ship is facing
            Vector2 thrustDirection = transform.up; // In 2D, up is the forward direction
            rb.AddForce(thrustDirection * thrustForce, ForceMode2D.Force);

            // Limit maximum velocity
            if (rb.linearVelocity.magnitude > maxVelocity)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
            }
        } else
        {
            afterBurner.SetActive(false);
        }
    }

    void HandleThrustEffects()
    {
        // Control thrust particle effects
        if (thrustParticles != null)
        {
            if (isThrusting && !thrustParticles.isPlaying)
            {
                thrustParticles.Play();
            }
            else if (!isThrusting && thrustParticles.isPlaying)
            {
                thrustParticles.Stop();
            }
        }
    }
}