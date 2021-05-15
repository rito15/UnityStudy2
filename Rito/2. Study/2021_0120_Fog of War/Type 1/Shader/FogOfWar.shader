// 설명 : 
Shader "FogOfWar/FogOfWarType1"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}

        //_EdgeLength("Edge length", Range(2,50)) = 5
        //_Phong("Phong Strengh", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert //tessellate:tessEdge tessphong:_Phong
            #pragma fragment frag nolights alpha:fade noshadow novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa noforwardadd nolppv noshadowmask interpolateview

            #include "UnityCG.cginc"
            //#include "Tessellation.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // Vertex Color
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // Vertex Color
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            /*float _Phong;
            float _EdgeLength;

            float4 tessEdge(appdata v0, appdata v1, appdata v2)
            {
                return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength);
            }*/

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.color = v.color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 col = i.color;

                return col;
            }

            ENDCG
        }
    }
}
