using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipWeapons : MonoBehaviour
{
    public Sprite bulletSprite;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Rotation with A/D or Left/Right arrows
        if (Input.GetKey(KeyCode.Space))
        {
            FireWeapon();
        }

    }

    void FireWeapon()
    {
        GameObject bullet = new GameObject("PlayerBullet");
        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        CircleCollider2D collider = bullet.AddComponent<CircleCollider2D>();
        bullet.tag = "Bullet";
        

        sr.sprite = bulletSprite;
        bullet.transform.localScale = bullet.transform.localScale * 0.2f;

        rb.AddForce(this.gameObject.transform.up * 1000, ForceMode2D.Force);
        rb.gravityScale = 0;
        bullet.transform.position = this.gameObject.transform.position + this.gameObject.transform.up * 0.8f;
    }
}