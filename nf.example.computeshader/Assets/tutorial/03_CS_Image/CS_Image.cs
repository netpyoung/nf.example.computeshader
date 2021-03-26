using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshRenderer))]
public class CS_Image : MonoBehaviour
{
    [SerializeField]
    ComputeShader _shader;

    RenderTexture _RenderTex;

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

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material.SetTexture("_BaseMap", _RenderTex);

    }
    void Start()
    {
        int kernelIndex = _shader.FindKernel("CS_Image");

        _shader.SetTexture(kernelIndex, "_RenderTex", _RenderTex);
        _shader.Dispatch(kernelIndex, _RenderTex.width / 8, _RenderTex.height / 8, 1);
    }

    private void OnDestroy()
    {
        _RenderTex.Release();
    }
}
