using System.Text;
using UnityEngine;

[DisallowMultipleComponent]
public class CS_Dimension2 : MonoBehaviour
{
    [SerializeField]
    ComputeShader _shader;

    void Awake()
    {
        int countGroup = (3 * 2 * 1);  // .Dispatch(kernelIndex, 3, 2, 1);
        int countThread = (2 * 2 * 1); //            [numthreads(2, 2, 1)]
        int countDispatched = countGroup * countThread;

        int[] recvData = new int[countDispatched];

        ComputeBuffer computeBuffer = new ComputeBuffer(countDispatched, sizeof(int));
        int kernelIndex = _shader.FindKernel("CS_Dimension2");

        _shader.SetBuffer(kernelIndex, "_Buffer", computeBuffer);

        computeBuffer.SetData(recvData);

        _shader.Dispatch(kernelIndex, 3, 2, 1);


        computeBuffer.GetData(recvData);

        StringBuilder sb = new StringBuilder();
        for (int y = 0; y < 4; ++y)
        {
            for (int x = 0; x < 6; ++x)
            {
                int index = y * 6 + x;
                sb.Append($" {recvData[index], 2}");
            }
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());

        computeBuffer.Release();
    }
}
