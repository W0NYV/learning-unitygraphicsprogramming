using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Vol2.ch02
{
    public class GPUTrails : MonoBehaviour
    {
        public struct Trail
        {
            public int currentNodeIdx;
        }

        //Trail内の制御点
        public struct Node
        {
            public float time;
            public Vector3 pos;
        }

        //軌跡をの残すものからの1フレーム分の入力
        public struct Input
        {
            public Vector3 pos;
        }

        [SerializeField] private ComputeShader _computeShader;
    
        private ComputeBuffer _trailBuffer;
        public ComputeBuffer TrailBuffer { get => _trailBuffer; }

        private ComputeBuffer _nodeBuffer;
        public ComputeBuffer NodeBuffer { get => _nodeBuffer; }

        private ComputeBuffer _inputBuffer;
        public ComputeBuffer InputBuffer { get => _inputBuffer; }

        [SerializeField] private int _trailNum = 10;
        public int TrailNum { get => _trailNum; }

        [SerializeField] private int _totalNodeNum = 5;
        [SerializeField] private float _updateDistanceMin = 0.01f;

        [SerializeField] private float _life = 10f;
        public float Life { get => _life; }

        private int _nodeNum;
        public int NodeNum { get => _nodeNum; }

        private void Start() {

            const float MAX_FPS = 60f;
            _nodeNum = Mathf.CeilToInt(_life * MAX_FPS);
            
            //ComputeBufferの初期化
            _trailBuffer = new ComputeBuffer(_trailNum, Marshal.SizeOf(typeof(Trail)));
            _nodeBuffer = new ComputeBuffer(_totalNodeNum, Marshal.SizeOf(typeof(Node)));
            _inputBuffer = new ComputeBuffer(_trailNum, Marshal.SizeOf(typeof(Input)));

            var initTrail = new Trail() { currentNodeIdx = -1 };
            var initNode = new Node() { time = -1 };

            //とりあえず -1 をセットしておく
            _trailBuffer.SetData(Enumerable.Repeat(initTrail, _trailNum).ToArray());
            _nodeBuffer.SetData(Enumerable.Repeat(initNode, _totalNodeNum).ToArray());

        }

        private void LateUpdate() 
        {
            //CSに値をセット
            _computeShader.SetFloat("_Time", Time.time);
            _computeShader.SetFloat("_UpdateDistanceMin", _updateDistanceMin);
            _computeShader.SetInt("_TrailNum", _trailNum);
            _computeShader.SetInt("_NodeNumPerTrail", _nodeNum);

            var kernel = _computeShader.FindKernel("CalcInput");

            //バッファのセット
            _computeShader.SetBuffer(kernel, "_TrailBuffer", _trailBuffer);
            _computeShader.SetBuffer(kernel, "_NodeBuffer", _nodeBuffer);
            _computeShader.SetBuffer(kernel, "_InputBuffer", _inputBuffer);

            uint x, y, z;
            //カーネルのスレッドグループサイズを取ってくる
            _computeShader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
            _computeShader.Dispatch(kernel, Mathf.CeilToInt(_trailNum / x), Mathf.CeilToInt(1f / y), Mathf.CeilToInt(1f / z));
        }

        private void OnDestroy() 
        {
            _trailBuffer.Release();
            _nodeBuffer.Release();
            _inputBuffer.Release();    
        }

        public void InputPoint(List<Input> inputs)
        {
            _inputBuffer.SetData(inputs);
        }

    }
}
