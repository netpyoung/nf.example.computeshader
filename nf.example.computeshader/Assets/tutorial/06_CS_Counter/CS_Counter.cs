using UnityEngine;

[DisallowMultipleComponent]
public class CS_Counter : MonoBehaviour
{
    [SerializeField]
    ComputeShader _counterShader;

    void Awake()
    {
        int size = 4 * 4 * 1 * 1;

        ComputeBuffer counterBuffer = new ComputeBuffer(size, sizeof(int), ComputeBufferType.Counter);
        counterBuffer.SetCounterValue(0);

        _counterShader.SetBuffer(0, "_CounterBuffer", counterBuffer);
        _counterShader.Dispatch(0, 1, 1, 1);

        int[] data = new int[size];
        counterBuffer.GetData(data);

        for (int i = 0; i < data.Length; ++i)
        {
            Debug.Log(data[i]);
        }

        counterBuffer.Release();
    }
}
