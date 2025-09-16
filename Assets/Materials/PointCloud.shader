Shader "Custom/Point Cloud"
{
    Properties
    {
        _PointSize ("Point Size", Float) = 1.0
        _PerspectiveEnabled ("Perspective Enabled", Float) = 0.0
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _NearColor ("Near Color", Color) = (0, 1, 0, 1)
        _FarColor ("Far Color", Color) = (1, 0, 0, 1)
        _MaxDistance ("Max Distance", Float) = 100.0
    }

    SubShader
    {
        Cull Off
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            float _PointSize;
            fixed _PerspectiveEnabled;
            fixed4 _BaseColor;
            fixed4 _NearColor;
            fixed4 _FarColor;
            float _MaxDistance;

            struct Vertex
            {
                float3 vertex : POSITION;
            };

            struct VertexOut
            {
                float psize : PSIZE;
                float4 center : TEXCOORD0;
                half size : TEXCOORD1;
                float distanceToCamera : TEXCOORD2;
                UNITY_FOG_COORDS(0)
            };

            VertexOut vert(Vertex vertex, out float4 outpos : SV_POSITION)
            {
                VertexOut o;
                float3 worldPos = mul(unity_ObjectToWorld, float4(vertex.vertex, 1.0)).xyz;
                float3 cameraPos = _WorldSpaceCameraPos;

                outpos = UnityObjectToClipPos(vertex.vertex);

                o.psize = lerp(_PointSize, _PointSize / outpos.w * _ScreenParams.y, step(0.5, _PerspectiveEnabled));
                o.size = o.psize;

                o.center = ComputeScreenPos(outpos);
                o.distanceToCamera = distance(worldPos, cameraPos);

                UNITY_TRANSFER_FOG(o, o.position);
                return o;
            }

            fixed4 frag(VertexOut i, UNITY_VPOS_TYPE vpos : VPOS) : SV_Target
            {
                float t = saturate(i.distanceToCamera / _MaxDistance);
                fixed4 c = lerp(_NearColor, _FarColor, t);

                float4 center = i.center;
                center.xy /= center.w;
                center.xy *= _ScreenParams.xy;
                float d = distance(vpos.xy, center.xy);

                if (d > i.size * 0.5) {
                    discard;
                }

                UNITY_APPLY_FOG(i.fogCoord, c);
                return c;
            }
            ENDCG
        }
    }
}
