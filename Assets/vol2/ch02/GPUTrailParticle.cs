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

            var updateThreadNum = new Vector3(_particleNum, 1f, 1f);
            ComputeShaderUtil.Dispatch(_computeShader, kernelUpdate, updateThreadNum);

            //Updateカーネルで位置を書き込んだバッファを、Inputバッファに書き込む
            var kernelInput = _computeShader.FindKernel("WriteToInput");
            _computeShader.SetBuffer(kernelInput, "_ParticleBufferRead", _particleBuffer);
            _computeShader.SetBuffer(kernelInput, "_InputBuffer", _trails.InputBuffer);

            var inputThreadNum = new Vector3(_particleNum, 1f, 1f);
            ComputeShaderUtil.Dispatch(_computeShader, kernelInput, inputThreadNum);
        }

        private void OnDestroy()
        {
            _particleBuffer.Release();
        }
    }
}
