using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vol2.ch02
{
    [RequireComponent(typeof(GPUTrails))]
    public class GPUTrailsRenderer : MonoBehaviour
    {

        [SerializeField] private Material _material;

        private GPUTrails _trails;

        private void Start() 
        {
            TryGetComponent<GPUTrails>(out _trails);
        }

        //通常のシーンのレンダリングが終わった後に呼ばれる
        private void OnRenderObject() 
        {
            _material.SetInt("_NodeNumPerTrail", _trails.NodeNum);
            _material.SetFloat("_Life", _trails.Life);
            _material.SetBuffer("_TrailBuffer", _trails.TrailBuffer);
            _material.SetBuffer("_NodeBuffer", _trails.NodeBuffer);

            //特定のパスを有効にする
            //シェーダファイルの最初に書かれているシェーダを有効にしているでいいのかな
            _material.SetPass(0);

            //GPU に完全なプロシージャルジオメトリを描画します。
            //_trails.NodeNum分の頂点を持つインスタンスを_trails.TrailNum分だけ描画する
            //Unity側がNowに変えてきたけどこっちのほうが良さげ？
            Graphics.DrawProceduralNow(MeshTopology.Points, _trails.NodeNum, _trails.TrailNum);

            //Graphics.DrawProcedural(MeshTopology.Points, _trails.NodeNum, _trails.TrailNum);

        }

    }
}
