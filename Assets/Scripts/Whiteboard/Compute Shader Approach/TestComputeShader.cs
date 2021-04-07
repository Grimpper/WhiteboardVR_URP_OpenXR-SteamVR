using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestComputeShader : MonoBehaviour
{
    public ComputeShader computeShader;
    public Texture2D iniTex;
    [SerializeField] private RenderTexture renderTexture;

    private int resolution;
    private int _kernel;
    private Vector2Int dispatchCount;

    private void Start()
    {
        resolution = iniTex.width;
        _kernel = computeShader.FindKernel("CSMain");

        renderTexture = new RenderTexture(resolution, resolution, 0);
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        
        Graphics.Blit(iniTex, renderTexture);
        
        GetComponent<Renderer>().material.SetTexture("_BaseMap", iniTex);
        computeShader.SetTexture(_kernel, "Result", renderTexture);
        computeShader.SetFloat("_Resolution", resolution);

        uint threadX = 0;
        uint threadY = 0;
        uint threadZ = 0;
        computeShader.GetKernelThreadGroupSizes(_kernel, out threadX, out threadY, out threadZ);
        dispatchCount.x = Mathf.CeilToInt(resolution / threadX);
        dispatchCount.y = Mathf.CeilToInt(resolution / threadY);
        
        computeShader.Dispatch(_kernel, dispatchCount.x, dispatchCount.y, 1);
    }
}
