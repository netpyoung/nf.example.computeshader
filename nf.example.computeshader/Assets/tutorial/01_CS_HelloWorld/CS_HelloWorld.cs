using UnityEngine;

[DisallowMultipleComponent]
public sealed class CS_HelloWorld : MonoBehaviour
{
    private static readonly int NAME_ID_BUFFER = Shader.PropertyToID("_Buffer");

    [SerializeField]
    private ComputeShader _shader;

    private void Awake()
    {
        int[] recvData = new int[4];

        using (GraphicsBuffer graphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 4, sizeof(int)))
        {
            _shader.SetBuffer(kernelIndex: 0, NAME_ID_BUFFER, graphicsBuffer);
            graphicsBuffer.SetData(new int[] { 11, 22, 33, 44 });

            _shader.Dispatch(kernelIndex: 0, 1, 1, 1);

            graphicsBuffer.GetData(recvData);
            for (int i = 0; i < 4; i++)
            {
                Debug.Log(recvData[i]);
            }
        }
    }
}