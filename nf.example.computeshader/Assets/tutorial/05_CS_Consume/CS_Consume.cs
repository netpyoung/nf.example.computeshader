using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public class CS_Consume : MonoBehaviour
{
    [SerializeField]
    Material _material;

    [SerializeField]
    ComputeShader _consumeShader;

    const int WIDTH = 32;
    const float BETWEEN_SIZE = 0.5f;

    ComputeBuffer _appendBuffer;
    ComputeBuffer _argBuffer;

    void Awake()
    {
        _appendBuffer = new ComputeBuffer(WIDTH * WIDTH, sizeof(float) * 3, ComputeBufferType.Append);
        _appendBuffer.SetCounterValue(0);

        int kernelConsumedIndex = _consumeShader.FindKernel("CS_Consume");
        int kernelAppendIndex = _consumeShader.FindKernel("CS_Append");
        _consumeShader.SetBuffer(kernelConsumedIndex, "_ConsumeBuffer", _appendBuffer);
        _consumeShader.SetBuffer(kernelAppendIndex, "_AppendBuffer", _appendBuffer);
        _consumeShader.SetFloat("_BetweenSize", BETWEEN_SIZE);
        _consumeShader.SetFloat("_Width", WIDTH);

        // Dispatch(4, 4, 1)
        // [numthreads[8, 8, 1]
        // 1024 x 1024 Size
        // x, y짝수만 걸러내면 256개가 _appendBuffer에 누적된다.
        _consumeShader.Dispatch(kernelAppendIndex, WIDTH / 8, WIDTH / 8, 1);

        // Dispatch(1, 1, 1)
        // [numthreads[8, 8, 1]
        // 8 x 8 Size
        // 64(8 * 8)개만큼 _consumeBuffer로 소모시킨다.
        _consumeShader.Dispatch(kernelConsumedIndex, WIDTH / 8 / 4, WIDTH / 8 / 4, 1);

        int[] args = new int[] { 0, 1, 0, 0 };
        _argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        _argBuffer.SetData(args);

        // _appendBuffer에 얼마나 들어있는지 _argBuffer[0]에 넣어주는 코드.
        ComputeBuffer.CopyCount(_appendBuffer, _argBuffer, 0);
        _argBuffer.GetData(args);

        Assert.AreEqual(256 - 64, args[0], "Vertex Count");
        Assert.AreEqual(1, args[1], "Instance Count");
        Assert.AreEqual(0, args[2], "Start Vertex");
        Assert.AreEqual(0, args[3], "Start Instance");
    }

    void OnRenderObject()
    {
        _material.SetPass(0);
        _material.SetBuffer("_Buffer", _appendBuffer);
        _material.SetColor("_Color", Color.yellow);

        // _argBuffer : [정점의 개수, 인스턴스의 개수, 시작 정점과 시작 인스턴스]
        Graphics.DrawProceduralIndirectNow(MeshTopology.Points, _argBuffer, 0);
    }

    void OnDestroy()
    {
        _appendBuffer.Release();
        _argBuffer.Release();
    }
}
