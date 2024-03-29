Shader "GPUTrails/GPUTrails" 
{

    Properties
    {
        _Width("Width", Float) = 0.1
        _StartColor("StartColor", Color) = (1, 1, 1, 1)
        _EndColor("EndColor", Color) = (0, 0, 0, 1)
    }

    SubShader
    {

        Pass
        {

            Cull Off 
            Fog { Mode Off }
            ZWrite Off
            Blend One One

            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "GPUTrails.cginc"

            float _Width;
            float _Life;
            float4 _StartColor;
            float4 _EndColor;
            StructuredBuffer<Trail> _TrailBuffer;
            StructuredBuffer<Node> _NodeBuffer;

            Node GetNode(int trailIdx, int nodeIdx)
            {
                return _NodeBuffer[toNodeBufIdx(trailIdx, nodeIdx)];
            } 

            struct v2g
            {
                float4 pos : POSITION0;
                float3 dir : TANGENT0;
                float4 col : COLOR0;
                float4 posNext : POSITION1;
                float3 dirNext : TANGENT1;
                float4 colNext : COLOR1;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float4 col : COLOR;
            };

            v2g vert(uint id : SV_VertexID, uint instanceId : SV_InstanceID)
            {
                v2g o;
                Trail trail = _TrailBuffer[instanceId];
                int currentNodeIdx = trail.currentNodeIdx;

                Node node0 = GetNode(instanceId, id - 1); //1つ前のNode
                Node node1 = GetNode(instanceId, id); //現在のNode
                Node node2 = GetNode(instanceId, id + 1); //1つ後のNode
                Node node3 = GetNode(instanceId, id + 2); //2つ後のNode

                //現在のNodeが末端か未入力の場合、全てのNodeを現在のNodeのコピーとする
                bool isLastNode = (currentNodeIdx == (int)id);
                if(isLastNode || !isValid(node1))
                {
                    node0 = node1 = node2 = node3 = GetNode(instanceId, currentNodeIdx);
                }

                //4つのNodeから位置情報を取り出す
                //未入力の場合、1個前のNodeをコピー
                float3 pos1 = node1.position;
                float3 pos0 = isValid(node0) ? node0.position : pos1;
                float3 pos2 = isValid(node2) ? node2.position : pos1;
                float3 pos3 = isValid(node3) ? node3.position : pos2;

                o.pos = float4(pos1, 1);
                o.posNext = float4(pos2, 1);

                //Pos0 → Pos2の方向ベクトルをPos1における接線(tangent)
                //Pos1 → Pos3の方向ベクトルをPos2における接線(tangent)
                //として出力
                //???
                o.dir = normalize(pos2 - pos0);
                o.dirNext = normalize(pos3 - pos1);

                float ageRate = saturate((_Time.y - node1.time) / _Life);
                float ageRateNext = saturate((_Time.y - node2.time) / _Life);
                o.col = lerp(_StartColor, _EndColor, ageRate);
                o.colNext = lerp(_StartColor, _EndColor, ageRateNext);

                return o;

            }

            //VSから渡ってきた2つNode情報からポリゴン生成
            //2つのposとdirから4つの位置を求め、TriangleStreamとして出力
            [maxvertexcount(4)]
            void geom(point v2g input[1], inout TriangleStream<g2f> outStream)
            {
                g2f output0, output1, output2, output3;

                float3 pos = input[0].pos;
                float3 dir = input[0].dir;
                float3 posNext = input[0].posNext;
                float3 dirNext = input[0].dirNext;

                float3 camPos = _WorldSpaceCameraPos;
                float3 toCamDir = normalize(camPos - pos); //posからカメラへの方向ベクトル
                float3 sideDir = normalize(cross(toCamDir, dir)); //接線ベクトルとの外積でラインの幅を広げる方向を取得
                float3 toCamDirNext = normalize(camPos - posNext);
                float3 sideDirNext = normalize(cross(toCamDirNext, dirNext));
                
                float width = _Width * 0.5;

                //posからsideDir * width分ずらした位置を新たな位置とする
                //フラグメントシェーダに渡すために、座標変換も済ませておく
                output0.pos = UnityWorldToClipPos(pos + (sideDir * width));
                output1.pos = UnityWorldToClipPos(pos - (sideDir * width));

                output2.pos = UnityWorldToClipPos(posNext + (sideDirNext * width));
                output3.pos = UnityWorldToClipPos(posNext - (sideDirNext * width));

                output0.col = output1.col = input[0].col;
                output2.col = output3.col = input[0].colNext;

                outStream.Append(output0);
                outStream.Append(output1);
                outStream.Append(output2);
                outStream.Append(output3);

                outStream.RestartStrip();
            }

            fixed4 frag(g2f In) : COLOR
            {
                return In.col;
            }

            ENDCG

        }

    }

}