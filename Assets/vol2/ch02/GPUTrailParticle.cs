using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Vol2.ch02
{
    [RequireComponent(typeof(GPUTrails))]
    public class GPUTrailParticle : MonoBehaviour
    {

        public struct Particle
        {
            public Vector3 position;
        }

        [SerializeField] private ComputeShader _computeShader;
        [SerializeField] private float _initRadius = 20f;
        [SerializeField] private float _timeScale = 1f;
        [SerializeField] private float _positionScale = 1f;
        [SerializeField] private float _noiseScale = 1f;

        private ComputeBuffer _particleBuffer;
        private GPUTrails _trails;

        //こんな記法あったんすね
        private int _particleNum => _trails.TrailNum;

        private void Start()
        {
            TryGetComponent<GPUTrails>(out _trails);

            _particleBuffer = new ComputeBuffer(_particleNum, Marshal.SizeOf(typeof(Particle)));

            _particleBuffer.SetData(Enumerable.Range(0, _particleNum)
                .Select(_ => new Particle() { position = Random.insideUnitSphere * _initRadius})
                .ToArray());
        }

        private void Update() {
            
            _computeShader.SetInt("_ParticleNum", _particleNum);
            _computeShader.SetFloat("_Time", Time.time);
            _computeShader.SetFloat("_PositionScale", _positionScale);
            _computeShader.SetFloat("_NoiseScale", _noiseScale);

            var kernelUpdate = _computeShader.FindKernel("Update");
            _computeShader.SetBuffer(kernelUpdate, "_ParticleBufferWrite", _particleBuffer);

            uint x, y, z;
            //カーネルのスレッドグループサイズを取ってくる
            _computeShader.GetKernelThreadGroupSizes(kernelUpdate, out x, out y, out z);
            _computeShader.Dispatch(kernelUpdate, Mathf.CeilToInt(_particleNum / x), Mathf.CeilToInt(1f / y), Mathf.CeilToInt(1f / z));

            var kernelInput = _computeShader.FindKernel("WriteToInput");
            _computeShader.SetBuffer(kernelInput, "_ParticleBufferRead", _particleBuffer);
            _computeShader.SetBuffer(kernelInput, "InputBuffer", _trails.InputBuffer);

            uint x2, y2, z2;
            //カーネルのスレッドグループサイズを取ってくる
            _computeShader.GetKernelThreadGroupSizes(kernelInput, out x2, out y2, out z2);
            _computeShader.Dispatch(kernelInput, Mathf.CeilToInt(_particleNum / x2), Mathf.CeilToInt(1f / y2), Mathf.CeilToInt(1f / z2));

        }

        private void OnDestroy()
        {
            _particleBuffer.Release();
        }
    }
}
