using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipWeapons : MonoBehaviour
{
    public Sprite bulletSprite;

    [Header("Weapon Settings")]
    public float fireRate = 0.01f; // Time between shots in seconds
    public float bulletSpeed = 1000f;
    public float bulletScale = 0.2f;

    private float nextFireTime = 0f;

    private AmmoGauge ammoGauge;

    // Start is called before the first frame update
    void Start()
    {
        ammoGauge = FindAnyObjectByType<AmmoGauge>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {

        // Alternative: Automatic fire while holding space
        
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            TryFireWeapon();
            nextFireTime = Time.time + fireRate;
        }
        
    }
    void TryFireWeapon()
    {
        if (ammoGauge != null && ammoGauge.UseShot())
        {
            FireWeapon();
        }
        else
        {
            // Play empty sound, show message, etc.
            Debug.Log("Out of ammo!");
        }
    }

    void FireWeapon()
    {
        GameObject bullet = new GameObject("PlayerBullet");
        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        CircleCollider2D collider = bullet.AddComponent<CircleCollider2D>();
        bullet.tag = "Bullet";

        // Add bullet component for cleanup
        Bullet bulletComponent = bullet.AddComponent<Bullet>();
        bulletComponent.lifetime = 3f; // Auto-destroy after 3 seconds

        sr.sprite = bulletSprite;
        bullet.transform.localScale = Vector3.one * bulletScale;

        rb.AddForce(this.gameObject.transform.up * bulletSpeed, ForceMode2D.Force);
        rb.gravityScale = 0;
        bullet.transform.position = this.gameObject.transform.position + this.gameObject.transform.up * 0.8f;

        FindFirstObjectByType<AudioManager>().PlayAudio("Shot");

        // Optional: Ignore collision with player
        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    }

    // Separate script for bullet behavior
    public class Bullet : MonoBehaviour
    {
        public float lifetime = 3f;

        void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}
