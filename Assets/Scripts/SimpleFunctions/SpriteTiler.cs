using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteTiler : MonoBehaviour
{

    public Vector2 tileRate=Vector2.zero;

    private SpriteRenderer spriteRenderer;
    private Vector2 objectSize;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
               
        if(spriteRenderer == null)
        {
            return;
        }

        objectSize = (spriteRenderer.sprite.rect.size/spriteRenderer.sprite.pixelsPerUnit) * new Vector2(transform.lossyScale.x,transform.lossyScale.y);
        updateTileRate();
        GameState.state.onDifficultyChanged += onDifficultyChanged;
    }


    void OnDestroy()
    {
        if(GameState.state)
        {
            GameState.state.onDifficultyChanged -= onDifficultyChanged;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onDifficultyChanged(int difficulty,float velocity)
    {
        tileRate = new Vector2(velocity, 0) / objectSize;
        updateTileRate();
    }

    void updateTileRate()
    {
        MaterialPropertyBlock propBlock=new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(propBlock);
        propBlock.SetVector("_TilingRate", new Vector4(tileRate.x, tileRate.y));
        spriteRenderer.SetPropertyBlock(propBlock);
    }
}
