using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vol2.ch02
{
    public class ComputeShaderUtil
    {
        public static void Dispatch(ComputeShader cs, int kernel, Vector3 threadNum)
        {
            uint x, y, z;
            
            //カーネルのスレッドグループサイズを取ってくる
            cs.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
            cs.Dispatch(kernel, Mathf.CeilToInt(threadNum.x / x), Mathf.CeilToInt(threadNum.y / y), Mathf.CeilToInt(threadNum.z / z));
        }
    }
}
