using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardRenderer : MonoBehaviour
{
    [SerializeField] private int textureSize = 2048;
    [SerializeField] private int penSize = 10;

    private Texture2D texture;
    private MeshRenderer renderComponent;
    private Color[] colorArray;
    
    private bool strokeCleared = false;

    public bool StrokeCleared
    {
        set => strokeCleared = value;
    }

    private RaycastHit collisionHit;
    public RaycastHit CollisionHit
    {
        set
        {
            collisionHit = value;
            RenderCollisionHit();
        }
    }

    private Vector2 lastPixelUV;

    void Start()
    {
        BoardPointCollector collector = GetComponent<BoardPointCollector>();
        collisionHit = collector.raycastHit;
        
        renderComponent = GetComponent<MeshRenderer>();


        texture = new Texture2D(textureSize, textureSize);
        renderComponent.material.mainTexture = texture;
    }

    private void RenderCollisionHit()
    {
        if (renderComponent == null || renderComponent.sharedMaterial == null || renderComponent.sharedMaterial.mainTexture == null)  return;
        
        Vector2 pixelUV = collisionHit.textureCoord;
        Debug.Log("pixelUV TEXCOORD: " + pixelUV);

        pixelUV.x *= texture.width;
        pixelUV.y *= texture.height;

        if (!strokeCleared)
        {
            for (float t = 0f; t < 1f; t += 0.01f)
            {
                int lerpedX = (int) Mathf.Lerp(lastPixelUV.x, pixelUV.x, t);
                int lerpedY = (int) Mathf.Lerp(lastPixelUV.y, pixelUV.y, t);
                texture.SetPixels(lerpedX, lerpedY, penSize, penSize, colorArray);
            }
        }
        else
        {
            texture.SetPixels((int)pixelUV.x, (int)pixelUV.y, penSize, penSize, colorArray);
            strokeCleared = false;
        }

        texture.Apply();
        lastPixelUV = pixelUV;
    }

    public void SetColor(Color color)
    {
        colorArray = Enumerable.Repeat(color, penSize * penSize).ToArray();
    }
}
