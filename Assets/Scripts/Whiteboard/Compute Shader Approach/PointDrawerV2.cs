using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointDrawerV2 : MonoBehaviour
{
    public ComputeShader computeShader;
    public Texture2D iniTex;
    [SerializeField] private RenderTexture renderTexture;
    public Material _material;

    private int resolution;
    private int _kernel;
    private Vector2Int dispatchCount;

    private Camera cam;
    private RaycastHit hit;
    private Vector2 mousePos;
    private Vector2 defaultPos = new Vector2(-9, 9);

    private float holdTimer = 0;

    private void Start()
    {
        cam = Camera.main;

        resolution = iniTex.width;
        _kernel = computeShader.FindKernel("CSMain");

        renderTexture = new RenderTexture(resolution, resolution, 0);
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        
        Graphics.Blit(iniTex, renderTexture);
        
        _material.SetTexture("_MainTex", renderTexture);
        computeShader.SetTexture(_kernel, "Result", renderTexture);
        computeShader.SetFloat("_Resolution", resolution);

        uint threadX = 0;
        uint threadY = 0;
        uint threadZ = 0;
        computeShader.GetKernelThreadGroupSizes(_kernel, out threadX, out threadY, out threadZ);
        dispatchCount.x = Mathf.CeilToInt(resolution / threadX);
        dispatchCount.y = Mathf.CeilToInt(resolution / threadY);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
        {
            if (mousePos != hit.textureCoord) mousePos = hit.textureCoord;
            holdTimer += Time.deltaTime;
        }
        else if (mousePos != defaultPos)
        {
            mousePos = defaultPos;
            holdTimer = 0;
        }
        
        computeShader.SetVector("_MousePosUV", mousePos);
        computeShader.SetFloat("_Time", holdTimer);
        computeShader.Dispatch(_kernel, dispatchCount.x, dispatchCount.y, 1);
    }
}
