using UnityEngine;

[DisallowMultipleComponent]
public class CS_HelloWorld : MonoBehaviour
{
    [SerializeField]
    ComputeShader _shader;

    void Awake()
    {
        int[] recvData = new int[4];

        ComputeBuffer computeBuffer = new ComputeBuffer(4, sizeof(int));
        _shader.SetBuffer(kernelIndex: 0, "_Buffer", computeBuffer);
        computeBuffer.SetData(new int[] { 11, 22, 33, 44 });

        _shader.Dispatch(kernelIndex: 0, 1, 1, 1);
        
        computeBuffer.GetData(recvData);
        for (int i = 0; i < 4; i++)
        {
            Debug.Log(recvData[i]);
        }
        computeBuffer.Release();
    }
}
