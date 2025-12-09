using UnityEngine;

public class ClickableSpriteSimple : MonoBehaviour
{
    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.gray;
    public Color clickColor = Color.yellow;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = normalColor;

        // Ensure there's a Collider2D
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    void OnMouseEnter()
    {
        spriteRenderer.color = hoverColor;
    }

    void OnMouseExit()
    {
        spriteRenderer.color = normalColor;
        
    }

    void OnMouseDown()
    {
        spriteRenderer.color = clickColor;
        SoundOptionsController soc = FindFirstObjectByType<SoundOptionsController>();
        if (gameObject.name == "music")
        {
            soc.SetMusic(false);
        }
        else if (gameObject.name == "musicOff")
        {
            soc.SetMusic(true);
        }
        else if (gameObject.name == "sfx")
        {
            soc.SetSfx(false);
        }
        else if (gameObject.name == "sfxOff")
        {
            soc.SetSfx(true);
        }
    }

    void OnMouseUp()
    {
        // Check if still hovering
        if (IsMouseOver())
        {
            spriteRenderer.color = hoverColor;
        }
        else
        {
            spriteRenderer.color = normalColor;
        }

        Debug.Log("Sprite clicked: " + gameObject.name);
    }

    bool IsMouseOver()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        return hit.collider != null && hit.collider.gameObject == gameObject;
    }
}