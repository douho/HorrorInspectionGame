Shader "UI/SpotlightHole"
{
    Properties
    {
        _Color ("Tint", Color) = (0,0,0,0.65)
        _HoleCenter ("Hole Center (0-1)", Vector) = (0.5, 0.5, 0, 0)
        _HoleRadius ("Hole Radius", Float) = 0.12
        _Softness ("Softness", Float) = 0.02
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { float4 vertex : POSITION; float2 texcoord : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };

            fixed4 _Color;
            float4 _HoleCenter;
            float _HoleRadius;
            float _Softness;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 p = i.uv;
                float2 c = _HoleCenter.xy;

                // 以中心為基準的向量
                float2 d2 = p - c;

                // 寬高比補正：避免圓形洞變橢圓
                float aspect = _ScreenParams.x / _ScreenParams.y;
                d2.x *= aspect;

                float d = length(d2);

                float edge0 = _HoleRadius;
                float edge1 = _HoleRadius + _Softness;
                float a = smoothstep(edge0, edge1, d); // 洞內 0，外面 1

                fixed4 col = _Color;
                col.a *= a;
                return col;
            }
            ENDCG
        }
    }
}
