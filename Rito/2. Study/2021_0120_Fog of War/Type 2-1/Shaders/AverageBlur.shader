Shader "FogOfWar/AverageBlur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
		_LastTex("LastTexture", 2D) = "black" {}

		_BlurRadius("BlurRadius",float) = 1.3
		_LerpRate("LerpRate",float) = 0.05
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	struct v2f
	{
		float4 pos:SV_POSITION;
		float2 uv:TEXCOORD0;
	};

	sampler2D _MainTex;
	sampler2D _LastTex;
	float4 _MainTex_TexelSize;		// x = 1/width, y = 1/height, z = width, w = height
	float _BlurRadius;		        // 블러 범위
	float _LerpRate;

	v2f vert (appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	}

	fixed4 frag (v2f i) : SV_Target
	{
		fixed4 col = fixed4(0,0,0,0);
		float2 offset = _BlurRadius * _MainTex_TexelSize;
		half G[9] = // 3x3 가우시안 블러
		{
			1,2,1,
			2,4,2,
			1,2,1
		};
		for (int x = 0; x < 3; x++)
		{
			for (int y = 0; y < 3; y++)
			{
				col += tex2D(_MainTex,i.uv + fixed2(x - 1, y - 1) * offset) * G[x * 1 + y * 3];
			}
		}
		col = col / 16;
		return col;
	} 	

	fixed4 lerpFrag (v2f i) : SV_Target
	{
		half4 col1=tex2D(_MainTex,i.uv);
		half4 col2=tex2D(_LastTex,i.uv);

		return lerp(col2, col1, _LerpRate);
	} 
	ENDCG

	SubShader
	{
		Cull Off ZWrite Off ZTest Always 
		Pass // Pass 0 : Blur
		{
			CGPROGRAM
			#pragma vertex vert	
			#pragma fragment frag
			ENDCG
		}
		Pass // Pass 1 : Lerp Last -> Current
		{
			CGPROGRAM
			#pragma vertex vert	
			#pragma fragment lerpFrag
			ENDCG
		}
	}
}