using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[RequireComponent(typeof(BoardPointCollectorCompute))]
public class BoardComputeRenderer : MonoBehaviour
{
    public ComputeShader computeShader;
    public enum InterpolationMethod { NoInterpolation, Interpolate }
    
    [Header("Point Interpolation Method")]
    public InterpolationMethod interpolationMethod = InterpolationMethod.NoInterpolation;
    
    [Header("Canvas Parameters")]
    [SerializeField, Min(1)] private int resolution = 1080;
    [SerializeField, Min(1)] private float penSize = 10;
    [SerializeField, Min(1)] private float eraserSize = 10;
    [SerializeField, Min(0.1f)] private float feather = 10;
    public Texture2D backgroundTex;
    private RenderTexture backgroundTexGamma;

    [Header("Result Text")]
    [SerializeField] private RenderTexture previousRenderTex;
    [SerializeField] private RenderTexture liveRenderTex;
    [SerializeField] private RenderTexture resultRenderTex;

    private RenderTexture transparentRenderTex;
    private bool wasPainting = false;
    
    
    [Header("Developer Options")]
    [SerializeField] private bool showDebug;

    [SerializeField, Min(1)] private int undoTexBufferSize = 10;
    private RenderTexture[] texBuffer;
    private int currentTexIndex = 0;
    private int maxTexIndex = 0;
    private int minTexIndex = 0;

    private static readonly Vector2 OUT_OF_RANGE = new Vector2(9, 9);

    private bool painting = false;

    public bool Painting
    {
        set
        {
            painting = value;
            if (!painting && wasPainting)
            {
                Graphics.Blit(transparentRenderTex, liveRenderTex);
                
                IncreaseIndex(ref currentTexIndex);
                maxTexIndex = currentTexIndex;
                if (maxTexIndex == minTexIndex) IncreaseIndex(ref minTexIndex);
                
                Graphics.Blit(previousRenderTex, texBuffer[currentTexIndex]);

                wasPainting = false;
                
                if (showDebug) Debug.Log("CurrentTexIndex: " + currentTexIndex);
            }

            if (painting) wasPainting = true;
        }
    }

    private bool eraseMode = false;

    public bool EraseMode
    {
        set => eraseMode = value;
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

    private Color color = Color.red;

    public Color Color
    {
        set => color = value;
    }
    
    private Vector2 lastPixelUV;

    private int kernel;
    private Vector2Int dispatchCount;
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    private void IncreaseIndex(ref int index)
    {
        index = ++index % undoTexBufferSize;
    }
    
    private void IncreaseIndex(ref int index, int max)
    {
        if (index != max) index = ++index % undoTexBufferSize;
    }
    
    private void DecreaseIndex(ref int index, int min)
    {
        if (index != min && --index < 0) index = undoTexBufferSize + index;
    }
    
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 50, 50), "Undo"))
        {
            DecreaseIndex(ref currentTexIndex, minTexIndex);
            Graphics.Blit(texBuffer[currentTexIndex], previousRenderTex);
            
            computeShader.SetVector("_PixelUV", OUT_OF_RANGE);
            computeShader.SetVector("_LastPixelUV", OUT_OF_RANGE);
            computeShader.Dispatch(kernel, dispatchCount.x, dispatchCount.y, 1);
            
            if (showDebug) Debug.Log("CurrentTexIndex: " + currentTexIndex);
        }
        
        if (GUI.Button(new Rect(70, 10, 50, 50), "Redo"))
        {
            IncreaseIndex(ref currentTexIndex, maxTexIndex);
            Graphics.Blit(texBuffer[currentTexIndex], previousRenderTex);
            
            computeShader.SetVector("_PixelUV", OUT_OF_RANGE);
            computeShader.SetVector("_LastPixelUV", OUT_OF_RANGE);
            computeShader.Dispatch(kernel, dispatchCount.x, dispatchCount.y, 1);
            
            if (showDebug) Debug.Log("CurrentTexIndex: " + currentTexIndex);
        }
    }

    private void Start()
    {
        CreateBoardTextures();

        Graphics.Blit(backgroundTex, backgroundTexGamma);
        
        GetComponent<Renderer>().material.SetTexture(BaseMap, resultRenderTex);
        
        InitShader();
    }

    void InitShader()
    {
        kernel = computeShader.FindKernel(interpolationMethod.ToString());
        
        computeShader.SetTexture(kernel, "_BackgroundTex", backgroundTexGamma);
        computeShader.SetTexture(kernel, "_PreviousTex", previousRenderTex);
        computeShader.SetTexture(kernel, "_LiveTex", liveRenderTex);
        computeShader.SetTexture(kernel, "_ResultTex", resultRenderTex);
        
        computeShader.SetFloat("_ResolutionInitTex", backgroundTexGamma.width);
        computeShader.SetFloat("_ResolutionResult", resolution);
        computeShader.SetVector("_Color", color);
        computeShader.SetFloat("_PenSize", penSize);
        computeShader.SetFloat("_EraserSize", eraserSize);
        computeShader.SetFloat("_Feather", feather);
        computeShader.SetBool("_Painting", painting);
        computeShader.SetBool("_EraseMode", eraseMode);
        
        computeShader.SetVector("_PixelUV", OUT_OF_RANGE);
        computeShader.SetVector("_LastPixelUV", OUT_OF_RANGE);

        computeShader.GetKernelThreadGroupSizes(kernel, out var threadX, out var threadY, out _);
        dispatchCount.x = Mathf.CeilToInt((float)resolution / threadX);
        dispatchCount.y = Mathf.CeilToInt((float)resolution / threadY);
        
        
        computeShader.Dispatch(kernel, dispatchCount.x, dispatchCount.y, 1);  
    }

    void CreateBoardTextures()
    {
        texBuffer = new RenderTexture[undoTexBufferSize];
         for (int i = 0; i < undoTexBufferSize; i++)
         {
             texBuffer[i] = new RenderTexture(resolution, resolution, 0)
             {
                 wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Point, enableRandomWrite = true
             };
             texBuffer[i].Create();
         }

        backgroundTexGamma = new RenderTexture(resolution, resolution, 0)
        {
            wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Point, enableRandomWrite = true
        };
        backgroundTexGamma.Create();

        previousRenderTex = new RenderTexture(resolution, resolution, 0)
        {
            wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Point, enableRandomWrite = true
        };
        previousRenderTex.Create();
        
        liveRenderTex = new RenderTexture(resolution, resolution, 0)
        {
            wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Point, enableRandomWrite = true
        };
        liveRenderTex.Create();
        
        resultRenderTex = new RenderTexture(resolution, resolution, 0)
        {
            wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Point, enableRandomWrite = true
        };
        resultRenderTex.Create();
        
        transparentRenderTex = new RenderTexture(resolution, resolution, 0)
        {
            wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Point, enableRandomWrite = true
        };
        transparentRenderTex.Create();
    }
    
    private void RenderCollisionHit()
    {
        Vector2 pixelUV = collisionHit.textureCoord;
        
        computeShader.SetVector("_Color", color);
        computeShader.SetFloat("_PenSize", penSize);
        computeShader.SetFloat("_EraserSize", eraserSize);
        computeShader.SetFloat("_Feather", feather);
        computeShader.SetBool("_Painting", painting);
        computeShader.SetBool("_EraseMode", eraseMode);
        computeShader.SetVector("_PixelUV", pixelUV);
        computeShader.SetVector("_LastPixelUV", lastPixelUV);
        computeShader.Dispatch(kernel, dispatchCount.x, dispatchCount.y, 1);
        
        lastPixelUV = pixelUV;
        
        if(showDebug) Debug.Log("pixelUV TEXCOORD: " + pixelUV);
    }
}
