using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomProperties;

public class BoardComputeRenderer : MonoBehaviour
{
    public ComputeShader computeShader;
    public enum InterpolationMethod { NoInterpolation, Interpolate }
    
    [Header("Canvas Parameters")]
    [SerializeField, Min(1)] private int resolution = 1080;
    [SerializeField] private int penSize = 10;
    public Texture2D iniTex;

    [Header("Point Interpolation Method")]
    public InterpolationMethod interpolationMethod = InterpolationMethod.NoInterpolation;
    
    [Header("Result Text")]
    [SerializeField, ReadOnly] private RenderTexture renderTexture;

    private bool strokeCleared = true;

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

    private Color color;

    public Color Color
    {
        set => color = value;
    }
    
    private Vector2 lastPixelUV;

    private int kernel;
    private Vector2Int dispatchCount;
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
    
    
    [Header("Developer Options")]
    [SerializeField] private bool showDebug = false;
    
    void Start()
    {
        kernel = computeShader.FindKernel(interpolationMethod.ToString());

        renderTexture = new RenderTexture(resolution, resolution, 0);
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        
        Graphics.Blit(iniTex, renderTexture);
        
        GetComponent<Renderer>().material.SetTexture(BaseMap, renderTexture);
        computeShader.SetTexture(kernel, "Result", renderTexture);
        computeShader.SetFloat("_Resolution", resolution);
        computeShader.SetFloat("_PenSize", penSize);

        computeShader.GetKernelThreadGroupSizes(kernel, out var threadX, out var threadY, out _);
        dispatchCount.x = Mathf.CeilToInt((float)resolution / threadX);
        dispatchCount.y = Mathf.CeilToInt((float)resolution / threadY);
        
        Vector2 startPos = Vector2.negativeInfinity;
        computeShader.SetBool("_StrokeCleared", strokeCleared);
        computeShader.SetVector("_PixelUV", startPos);
        computeShader.SetVector("_LastPixelUV", startPos);
        computeShader.Dispatch(kernel, dispatchCount.x, dispatchCount.y, 1);
    }
    
    private void RenderCollisionHit()
    {
        Vector2 pixelUV = collisionHit.textureCoord;
        
        computeShader.SetBool("_StrokeCleared", strokeCleared);
        computeShader.SetVector("_PixelUV", pixelUV);
        computeShader.SetVector("_LastPixelUV", lastPixelUV);
        computeShader.Dispatch(kernel, dispatchCount.x, dispatchCount.y, 1);
        
        lastPixelUV = pixelUV;
        strokeCleared = false;
        
        if(showDebug) Debug.Log("pixelUV TEXCOORD: " + pixelUV);
    }
}
