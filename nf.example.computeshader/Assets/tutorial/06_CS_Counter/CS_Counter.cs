using UnityEngine;

[DisallowMultipleComponent]
public sealed class CS_Counter : MonoBehaviour
{
    private static readonly int NAME_ID_COUNTERBUFFER = Shader.PropertyToID("_CounterBuffer");

    [SerializeField]
    private ComputeShader _counterShader;

    void Awake()
    {
        int size = 4 * 4 * 1 * 1;

        using (GraphicsBuffer counterBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Counter, size, sizeof(int)))
        {
            counterBuffer.SetCounterValue(0);

            _counterShader.SetBuffer(0, NAME_ID_COUNTERBUFFER, counterBuffer);
            _counterShader.Dispatch(0, 1, 1, 1);

            int[] data = new int[size];
            counterBuffer.GetData(data);

            for (int i = 0; i < data.Length; ++i)
            {
                Debug.Log(data[i]);
            }
        }
    }
}
