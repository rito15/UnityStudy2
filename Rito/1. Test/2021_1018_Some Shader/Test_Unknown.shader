Shader "Unlit/Test_Unknown"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Speed ("Speed", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Speed;

            v2f vert (appdata v)
            {
                v2f o;
                v.vertex = mul(unity_WorldToObject, mul(unity_ObjectToWorld, v.vertex) - mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)));
                //v.vertex.z += sin(-1.7 * _Time.y * _Speed + v.vertex.x * 150.6) * .12 * v.vertex.y;
                //v.vertex.x += sin(-1.1 * _Time.y * _Speed + v.vertex.z * 400.6) * .1 * v.vertex.y;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
