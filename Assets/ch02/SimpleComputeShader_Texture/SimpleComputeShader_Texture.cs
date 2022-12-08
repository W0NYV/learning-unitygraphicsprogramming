using UnityEngine;

namespace SimpleComputeShader
{

    public class SimpleComputeShader_Texture : MonoBehaviour
    {

        [SerializeField] private ComputeShader _computeShader;
        [SerializeField] private GameObject _planeA;
        [SerializeField] private GameObject _planeB;

        RenderTexture renderTexture_A;
        RenderTexture renderTexture_B;

        int kernelIndex_KernelFunction_A;
        int kernelIndex_KernelFunction_B;

        struct ThreadSize
        {
            public int x;
            public int y;
            public int z;

            public ThreadSize(uint x, uint y, uint z)
            {
                this.x = (int)x;
                this.y = (int)y;
                this.z = (int)z; 
            }
        }

        ThreadSize kernelThreadSize_KernelFunction_A;
        ThreadSize kernelThreadSize_KernelFunction_B;

        void Start()
        {
            renderTexture_A = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
            renderTexture_A.enableRandomWrite = true;
            renderTexture_A.Create();

            renderTexture_B = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
            renderTexture_B.enableRandomWrite = true;
            renderTexture_B.Create();

            kernelIndex_KernelFunction_A = _computeShader.FindKernel("KernelFunction_A");
            kernelIndex_KernelFunction_B = _computeShader.FindKernel("KernelFunction_B");

            uint threadSizeX, threadSizeY, threadSizeZ;

            _computeShader.GetKernelThreadGroupSizes(kernelIndex_KernelFunction_A, out threadSizeX, out threadSizeY, out threadSizeZ);
            kernelThreadSize_KernelFunction_A = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);

            _computeShader.GetKernelThreadGroupSizes(kernelIndex_KernelFunction_B, out threadSizeX, out threadSizeY, out threadSizeZ);
            kernelThreadSize_KernelFunction_B = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);

            _computeShader.SetTexture(kernelIndex_KernelFunction_A, "textureBuffer", renderTexture_A);
            _computeShader.SetTexture(kernelIndex_KernelFunction_B, "textureBuffer", renderTexture_B);

            _computeShader.Dispatch(kernelIndex_KernelFunction_A,
                                    renderTexture_A.width / kernelThreadSize_KernelFunction_A.x,
                                    renderTexture_A.height / kernelThreadSize_KernelFunction_A.y,
                                    kernelThreadSize_KernelFunction_A.z);
            
            _computeShader.Dispatch(kernelIndex_KernelFunction_B,
                                    renderTexture_B.width / kernelThreadSize_KernelFunction_B.x,
                                    renderTexture_B.width / kernelThreadSize_KernelFunction_B.y,
                                    kernelThreadSize_KernelFunction_B.z);

            _planeA.GetComponent<Renderer>().material.mainTexture = renderTexture_A;
            _planeB.GetComponent<Renderer>().material.mainTexture = renderTexture_B;

        }

    }

}
