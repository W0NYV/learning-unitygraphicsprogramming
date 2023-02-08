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
    
        private ComputeBuffer _trailBuffer;
        private ComputeBuffer _nodeBuffer;
        private ComputeBuffer _inputBuffer;

        [SerializeField] private int _trailNum = 10;
        [SerializeField] private int _totalNodeNum = 5;

        private void Start() {
            
            _trailBuffer = new ComputeBuffer(_trailNum, Marshal.SizeOf(typeof(Trail)));
            _nodeBuffer = new ComputeBuffer(_totalNodeNum, Marshal.SizeOf(typeof(Node)));
            _inputBuffer = new ComputeBuffer(_trailNum, Marshal.SizeOf(typeof(Input)));

            var initTrail = new Trail() { currentNodeIdx = -1 };
            var initNode = new Node() { time = -1 };

            //とりあえず -1 をセットしておく
            _trailBuffer.SetData(Enumerable.Repeat(initTrail, _trailNum).ToArray());
            _nodeBuffer.SetData(Enumerable.Repeat(initNode, _totalNodeNum).ToArray());

        }

    }
}
