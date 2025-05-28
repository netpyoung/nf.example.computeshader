using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public sealed class CS_Append : MonoBehaviour
{
    private static readonly int NAME_ID_compute_AppendBuffer = Shader.PropertyToID("_AppendBuffer");
    private static readonly int NAME_ID_compute_BetweenSize = Shader.PropertyToID("_BetweenSize");
    private static readonly int NAME_ID_compute_Width = Shader.PropertyToID("_Width");
    private static readonly int NAME_ID_mat_Buffer = Shader.PropertyToID("_Buffer");
    private static readonly int NAME_ID_mat_Color = Shader.PropertyToID("_Color");

    [SerializeField]
    private Material _material;

    [SerializeField]
    private ComputeShader _appendBufferShader;

    private const int WIDTH = 32;
    private const float BETWEEN_SIZE = 0.5f;

    private GraphicsBuffer _appendBuffer;
    private GraphicsBuffer _argBuffer;

    private void Awake()
    {
        _appendBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append,  WIDTH * WIDTH, sizeof(float) * 3);
        _appendBuffer.SetCounterValue(0);

        _appendBufferShader.SetBuffer(kernelIndex: 0, NAME_ID_compute_AppendBuffer, _appendBuffer);
        _appendBufferShader.SetFloat(NAME_ID_compute_BetweenSize, BETWEEN_SIZE);
        _appendBufferShader.SetFloat(NAME_ID_compute_Width, WIDTH);

        // Dispatch(4, 4, 1)
        // [numthreads[8, 8, 1]
        // 1024 x 1024 Size
        // x, y짝수만 걸러내면 256개가 _appendBuffer에 누적된다.
        _appendBufferShader.Dispatch(0, WIDTH / 8, WIDTH / 8, 1);

        int[] args = new int[] { 0, 1, 0, 0 };
        _argBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 4, sizeof(int));
        _argBuffer.SetData(args);

        // _appendBuffer에 얼마나 들어있는지 _argBuffer[0]에 넣어주는 코드.
        GraphicsBuffer.CopyCount(_appendBuffer, _argBuffer, 0);
        _argBuffer.GetData(args);

        Assert.AreEqual(256, args[0], "Vertex Count");
        Assert.AreEqual(1, args[1], "Instance Count");
        Assert.AreEqual(0, args[2], "Start Vertex");
        Assert.AreEqual(0, args[3], "Start Instance");
    }

    private void OnRenderObject()
    {
        _material.SetPass(0);
        _material.SetBuffer(NAME_ID_mat_Buffer, _appendBuffer);
        _material.SetColor(NAME_ID_mat_Color, Color.red);

        // _argBuffer : [정점의 개수, 인스턴스의 개수, 시작 정점과 시작 인스턴스]
        Graphics.DrawProceduralIndirectNow(MeshTopology.Points, _argBuffer, 0);
    }

    private void OnDestroy()
    {
        _appendBuffer.Release();
        _argBuffer.Release();
    }
}