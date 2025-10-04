using UnityEngine;

public class AfterburnerAnimation : MonoBehaviour
{
    
    SpriteRenderer spriteRenderer;

    private readonly float updateInterval = 0.05f;
    [SerializeField] private float updateTimer = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {

        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0)
        {
            spriteRenderer.color =  new Color(spriteRenderer.color.r, Random.Range(0.8f,1f), spriteRenderer.color.b);
            updateTimer = updateInterval;
        }
    }
}
