// 설명 : 
Shader "Custom/MPBTest"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _Glossiness ("Smoothness", Range(0,1)) = 0.5

        [HideInInspector] _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        
        //fixed4 _Color;
        //half _Metallic;

        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_DEFINE_INSTANCED_PROP(half, _Metallic)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //fixed4 c = _Color;
            //o.Metallic = _Metallic;
            fixed4 c = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            o.Metallic = UNITY_ACCESS_INSTANCED_PROP(Props, _Metallic);

            o.Albedo = c.rgb;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
