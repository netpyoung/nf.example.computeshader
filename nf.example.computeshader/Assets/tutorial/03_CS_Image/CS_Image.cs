using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshRenderer))]
public sealed class CS_Image : MonoBehaviour
{
    private static readonly int NAME_ID_BASEMAP = Shader.PropertyToID("_BaseMap");
    private static readonly int NAME_ID_RENDERTEX = Shader.PropertyToID("_RenderTex");

    [SerializeField]
    private ComputeShader _shader;

    private RenderTexture _RenderTex;

    private void Awake()
    {
        _RenderTex = new RenderTexture(
            width: 64,
            height: 64,
            depth: 0,
            format: RenderTextureFormat.ARGB32,
            readWrite: RenderTextureReadWrite.Linear)
        {
            enableRandomWrite = true
        };

        _RenderTex.Create();

        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material.SetTexture(NAME_ID_BASEMAP, _RenderTex);
    }

    private void Start()
    {
        int kernelIndex = _shader.FindKernel("CS_Image");

        _shader.SetTexture(kernelIndex, NAME_ID_RENDERTEX, _RenderTex);
        _shader.Dispatch(kernelIndex, _RenderTex.width / 8, _RenderTex.height / 8, 1);
    }

    private void OnDestroy()
    {
        _RenderTex.Release();
    }
}