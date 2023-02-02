using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    [RequireComponent(typeof(GPUBoids))]
    public class BoidsRender : MonoBehaviour
    {

        //描画するBoidsオブジェクトのスケール
        public Vector3 ObjectScale = new Vector3(0.1f, 0.2f, 0.5f);

        public GPUBoids GPUBoidsScript;

        public Mesh InstanceMesh;

        public Material InstanceRenderMaterial;

        //GPUインスタシングのための引数(ComputeBufferへの転送用)
        //インスタンスあたりのインデックス数、インスタンス数
        //開始インデックス位置、ベース頂点位置、インスタンス開始位置
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

        //GPUインスタシングのための引数バッファ
        ComputeBuffer argsBuffer;

        // Start is called before the first frame update
        void Start()
        {
            //引数バッファを初期化
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        }

        // Update is called once per frame
        void Update()
        {
            RenderInstancedMesh();
        }

        private void OnDisable() {
            if(argsBuffer != null) argsBuffer.Release();
            argsBuffer = null;
        }

        private void RenderInstancedMesh()
        {
            //描画用マテリアルがNull、またはGPUBoidsがNull、またはGPUインスタシングがサポートしなければ処理しない
            if(InstanceRenderMaterial == null || GPUBoidsScript == null || !SystemInfo.supportsInstancing) return;

            //指定したメッシュのインデックス数を取得
            uint numIndices = (InstanceMesh != null) ? (uint)InstanceMesh.GetIndexCount(0) : 0;

            //メッシュのインデックス数をセット
            args[0] = numIndices;
            
            //インスタンス数をセット
            args[1] = (uint)GPUBoidsScript.GetMaxObjectNum();

            //バッファにセット
            argsBuffer.SetData(args);

            //Boidデータを格納したバッファをマテリアルにセット
            InstanceRenderMaterial.SetBuffer("_BoidDataBuffer", GPUBoidsScript.GetBoidDataBuffer());

            //Boidオブジェクトスケールをセット
            InstanceRenderMaterial.SetVector("_ObjectScale", ObjectScale);

            //境界領域を定義
            var bounds = new Bounds
            (
                GPUBoidsScript.GetSimulationAreaCenter(),
                GPUBoidsScript.GetSimulationAreaSize()
            );

            //メッシュをGPUインスタシングして描画
            Graphics.DrawMeshInstancedIndirect(InstanceMesh, 0, InstanceRenderMaterial, bounds, argsBuffer);
        }
    }
}
