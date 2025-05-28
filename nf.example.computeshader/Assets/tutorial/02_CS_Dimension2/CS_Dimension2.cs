using System.Text;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CS_Dimension2 : MonoBehaviour
{
    private static readonly int NAME_ID_BUFFER = Shader.PropertyToID("_Buffer");

    [SerializeField]
    private ComputeShader _shader;

    private void Awake()
    {
        int countGroup = (3 * 2 * 1); // .Dispatch(kernelIndex, 3, 2, 1);
        int countThread = (2 * 2 * 1); //            [numthreads(2, 2, 1)]
        int countDispatched = countGroup * countThread;

        int[] recvData = new int[countDispatched];
        using (GraphicsBuffer graphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, countDispatched, sizeof(int)))
        {
            int kernelIndex = _shader.FindKernel("CS_Dimension2");

            _shader.SetBuffer(kernelIndex, NAME_ID_BUFFER, graphicsBuffer);
            graphicsBuffer.SetData(recvData);

            _shader.Dispatch(kernelIndex, 3, 2, 1);
            graphicsBuffer.GetData(recvData);

            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < 4; ++y)
            {
                for (int x = 0; x < 6; ++x)
                {
                    int index = y * 6 + x;
                    sb.Append($" {recvData[index],2}");
                }

                sb.AppendLine();
            }

            Debug.Log(sb.ToString());
        }
    }
}