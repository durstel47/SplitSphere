Shader "Unlit/UnlitContrastControlShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Contrast("Contrast", Float) = 1.0
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
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST; //Unity3D predefined variable with tiling scales and translations(offsets) for _MainTex
			float _Contrast;

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv.y = v.uv.y;
				o.uv.x = 1 - v.uv.x; //reverse text around yaw
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb = 0.5 + _Contrast * (col.rgb - 0.5);
				return col;
			}
			ENDCG
		}
	}
}
