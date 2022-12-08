using UnityEngine;

namespace SimpleComputeShader
{
    public class SimpleComputeShader_Array : MonoBehaviour
    {

        [SerializeField] private ComputeShader _computeShader;
        int kernelIndex_KernelFunction_A;
        int kernelIndex_KernelFunction_B;
        ComputeBuffer intComputeBuffer;


        void Start()
        {
            //カーネルを指定するためのインデックスが必要、FindKernelメソッドで見つける
            kernelIndex_KernelFunction_A = _computeShader.FindKernel("KernelFunction_A");
            kernelIndex_KernelFunction_B = _computeShader.FindKernel("KernelFunction_B");

            //GPUによる演算結果をCPUに保存するためのバッファ
            intComputeBuffer = new ComputeBuffer(4, sizeof(int));
            _computeShader.SetBuffer(kernelIndex_KernelFunction_A, "intBuffer", intComputeBuffer);
            
            this._computeShader.SetInt("intValue", 2);

            //今回のグループ数は、1*1*1=1なので1グループ
            _computeShader.Dispatch(kernelIndex_KernelFunction_A, 1, 1, 1);

            int[] result = new int[4];

            intComputeBuffer.GetData(result);

            for(int i = 0; i < 4; i++)
            {
                Debug.Log(result[i]);
            }

            //異なるカーネルの実行
            _computeShader.SetBuffer(kernelIndex_KernelFunction_B, "intBuffer", intComputeBuffer);

            _computeShader.Dispatch(kernelIndex_KernelFunction_B, 1, 1, 1);

            intComputeBuffer.GetData(result);

            for(int i = 0; i < 4; i++) {
                Debug.Log(result[i]);
            }

            //使い終わったバッファは明示的に破棄する
            intComputeBuffer.Release();
        }


    }
}

