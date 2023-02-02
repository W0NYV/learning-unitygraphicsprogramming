Shader "Custom/NewSurfaceShader"
{
    Properties
    {
        _Height("Height", float) = 5.0
        _TopColor("Top Color", Color) = (0.0, 0.0, 0.0, 1.0)
        _BottomColor("Bottom Color", Color) = (1.0, 0.0, 0.0, 0.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform float _Height;
            uniform float4 _TopColor, _BottomColor;

            struct v2g
            {
                float4 pos : SV_POSITION;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float4 col : COLOR;
            };

            v2g vert(appdata_full v)
            {
                v2g o;
                o.pos = v.vertex;

                return o;
            }

            //triangle単位で実行、出力もtriangle、最大出力数12（4のtriangle、triangleの頂点は3つ）
            [maxvertexcount(12)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> outStream)
            {
                float4 p0 = input[0].pos;
                float4 p1 = input[1].pos;
                float4 p2 = input[2].pos;

                //三角形の中心座標を計算して、さらに上方向に移動させた新しい座標
                float4 c = float4(0.0f, _Height, 0.0f, 1.0f) + (p0 + p1 + p2) * 0.33333f;

                //頂点シェーダでMVPかけてないのでここでかける
                g2f out0;
                out0.pos = UnityObjectToClipPos(p0);
                out0.col = _BottomColor;

                g2f out1;
                out1.pos = UnityObjectToClipPos(p1);
                out1.col = _BottomColor;

                g2f out2;
                out2.pos = UnityObjectToClipPos(p2);
                out2.col = _BottomColor;

                g2f o;
                o.pos = UnityObjectToClipPos(c);
                o.col = _TopColor;
                
                //面を4つ作る
                // bottom
                outStream.Append(out0);
                outStream.Append(out1);
                outStream.Append(out2);
                outStream.RestartStrip();

                // sides
                outStream.Append(out0);
                outStream.Append(out1);
                outStream.Append(o);
                outStream.RestartStrip();

                outStream.Append(out1);
                outStream.Append(out2);
                outStream.Append(o);
                outStream.RestartStrip();

                outStream.Append(out2);
                outStream.Append(out0);
                outStream.Append(o);
                outStream.RestartStrip();

            }

            float4 frag(g2f i) : COLOR
            {
                return i.col;
            }
            ENDCG
        }

    }
}