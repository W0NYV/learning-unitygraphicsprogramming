using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Boids
{
    public class GPUBoids : MonoBehaviour
    {

        //Boidデータの構造体
        [System.Serializable] struct BoidData
        {
            public Vector3 velocity;
            public Vector3 position;
        }

        //スレッドグループのスレッドのサイズ
        const int SIMULATION_BLOCK_SIZE = 256;

        //最大オブジェクト数
        [Range(256, 32768)] private int _maxObjectNum = 16384;
        public int MaxObjectNum => _maxObjectNum; 

        //結合を適用する他の個体との半径
        [SerializeField] private float _cohesionNeighborhoodRadius = 2f;

        //整列を適用する他の個体との半径
        [SerializeField] private float _alignmentNeighborhoodRadius = 2f;

        //分離を適用する他の個体との半径
        [SerializeField] private float _separateNeighborhoodRadius = 1f;

        [SerializeField] private float _maxSpeed = 5f;
        [SerializeField] private float _maxSteerForce = 0.5f; //操舵力の最大値

        [SerializeField] private float _cohesionWeight = 1f; //結合する力の重み
        [SerializeField] private float _alignmentWeight = 1f; //整列する力の重み
        [SerializeField] private float _separateWeight = 3f; //分離する力の重み
        [SerializeField] private float _avoidWallWeight = 10f; //壁を避ける力の重み

        [SerializeField] private Vector3 _wallCenter = Vector3.zero;
        public Vector3 WallCenter => _wallCenter;

        [SerializeField] private Vector3 _wallSize = new Vector3(32f, 32f, 32f);
        public Vector3 WallSize => _wallSize;

        [SerializeField] private ComputeShader _boidsCS;
        
        private ComputeBuffer _boidForceBuffer; //Boidの操舵力を格納したバッファ

        private ComputeBuffer _boidDataBuffer; //Boidの基本データを格納したバッファ
        public ComputeBuffer BoidDataBuffer => _boidDataBuffer;

        //Boidの基本データを格納したバッファを取得
        public ComputeBuffer GetBoidDataBuffer()
        {
            return _boidDataBuffer != null ? _boidDataBuffer : null;
        }

        //オブジェクト数を取得
        public int GetMaxObjectNum()
        {
            return MaxObjectNum;
        }

        //シミュレーション領域の中心座標を返す
        public Vector3 GetSimulationAreaCenter()
        {
            return WallCenter;
        }

        //シミュレーション領域のボックスのサイズを返す
        public Vector3 GetSimulationAreaSize()
        {
            return WallSize;
        }

        private void Start() {
            InitBuffer();
        }

        private void Update() {
            Simulation();
        }

        private void OnDestroy() {
            ReleaseBuffer();
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(WallCenter, WallSize);
        }

        private void InitBuffer()
        {
            //バッファを初期化
            _boidDataBuffer = new ComputeBuffer(MaxObjectNum, Marshal.SizeOf(typeof(BoidData)));
            _boidForceBuffer = new ComputeBuffer(MaxObjectNum, Marshal.SizeOf(typeof(Vector3)));

            //Boidデータ、Forceバッファを初期化
            var forceArr = new Vector3[MaxObjectNum];
            var boidDataArr = new BoidData[MaxObjectNum];
            for(var i = 0; i < MaxObjectNum; i++)
            {
                forceArr[i] = Vector3.zero;
                boidDataArr[i].position = Random.insideUnitSphere * 1f;
                boidDataArr[i].velocity = Random.insideUnitSphere * 0.1f;
            }
            _boidForceBuffer.SetData(forceArr);
            _boidDataBuffer.SetData(boidDataArr);

            //メモリの開放的な
            forceArr = null;
            boidDataArr = null;
        }

        private void Simulation()
        {
            ComputeShader cs = _boidsCS;
            int id = -1;

            //スレッドグループ数を求める
            int threadGroupSize = Mathf.CeilToInt(MaxObjectNum / SIMULATION_BLOCK_SIZE);

            //操舵力を計算
            id = cs.FindKernel("ForceCS"); //カーネルIDを取得
            cs.SetInt("_MaxBoidObjectNum", MaxObjectNum);
            cs.SetFloat("_CohesionNeighborhoodRadius", _cohesionNeighborhoodRadius);
            cs.SetFloat("_AlignmentNeighborhoodRadius", _alignmentNeighborhoodRadius);
            cs.SetFloat("_SeparateNeighborhoodRadius", _separateNeighborhoodRadius);
            cs.SetFloat("_MaxSpeed", _maxSpeed);
            cs.SetFloat("_MaxSteerForce", _maxSteerForce);
            cs.SetFloat("_SeparateWeight", _separateWeight);
            cs.SetFloat("_CohesionWeight", _cohesionWeight);
            cs.SetFloat("_AlignmentWeight", _alignmentWeight);
            cs.SetVector("_WallCenter", _wallCenter);
            cs.SetVector("_WallSize", _wallSize);
            cs.SetFloat("_AvoidWallWeight", _avoidWallWeight);
            cs.SetBuffer(id, "_BoidDataBufferRead", _boidDataBuffer);
            cs.SetBuffer(id, "_BoidForceBufferWrite", _boidForceBuffer);
            cs.Dispatch(id, threadGroupSize, 1, 1); //CSを実行

            //操舵力から速度と位置を計算
            id = cs.FindKernel("IntegrateCS");
            cs.SetFloat("_DeltaTime", Time.deltaTime);
            cs.SetBuffer(id, "_BoidForceBufferRead", _boidForceBuffer);
            cs.SetBuffer(id, "_BoidDataBufferWrite", _boidDataBuffer);
            cs.Dispatch(id, threadGroupSize, 1, 1);
        }

        private void ReleaseBuffer()
        {
            if(_boidDataBuffer != null)
            {
                _boidDataBuffer.Release();
                _boidDataBuffer = null;
            }

            if(_boidForceBuffer != null)
            {
                _boidForceBuffer.Release();
                _boidForceBuffer = null;
            }
        }
    }
}
