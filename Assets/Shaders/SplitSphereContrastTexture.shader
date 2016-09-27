Shader "Custom/SplitSphereContrastTexture"
{
	//changed version: added Mask Midpoint X to enable gaze fixed mask stabilisation
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {} //shader conventions require one _MainTex texture: it's the rightside t.
		_MaskTex(" Texture", 2D) = "white" {}
		_Contrast("Overall Contrast", Float) = 1.0
		_PeripheralContrast("Pheripheral Texture Contrast", Float) = 1.0
		_IsMirroredPeriphery("Periphery Mirrored around Vertical", Float) = 0.0
		_CentralContrast("Central Texture Contrast", Float) = 1.0
		_IsMirroredCenter("Center Mirrored around Vertical", Float) = 0.0
		_MaskMidPointX("Mask Midpoint X", Float) = 0.0
		//_GazePosX("Gaze Position X", Float) = 0.0
		//_GazePosY("Gaze Position Y", Float) = 0.0
		_MaskScaleX("Mask Scaling Factor X", Float) = 1.0
		_IsGray("Gray Patch Enabled", Int) = 0
	}
	SubShader
	{

		Pass
		{
			ZTest Always Cull Off ZWrite Off			
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"


			sampler2D _MainTex;
			float4 _MainTex_ST; //Unity predefined variable with tiling scales and translations(offsets) for _MainTex
			sampler2D _MaskTex;
			float4 _MaskTex_ST;
			float _Contrast;	//attention: contrast is not corrected for display's non-linearity
			float _PeripheralContrast;
			float _IsMirroredPeriphery;
			float _CentralContrast;
			float _IsMirroredCenter;
			float _MaskMidPointX;
			//float _GazePosX; //gaze screen position (Range 0..1) used for stabilisation of mask
			//float _GazePosY;
			float _MaskScaleX;
			int _IsGray;

			
			//v2f vert (appdata v)
			//{
			//	v2f o;
			//	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			//	o.texCoord = v.texCoord;
			//	return o;
			//}
			
			//fixed4 frag (v2f i) : SV_Target
			fixed4 frag (v2f_img i) : SV_Target
			{
				// set tiling scales and offsets
 				float2 uv = i.uv * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uvm = i.uv * _MaskTex_ST.xy + _MaskTex_ST.zw;
				float2 uvc = uv;
				if (_IsMirroredCenter)
					uvc.x = 1.0 - uv.x;
				float2 uvp = uv;
				if (_IsMirroredPeriphery)
					uvp.x = 1.0 - uv.x;
				float2 uvms = uvm;
				uvms.x = (uvm.x - 0.5 - _MaskMidPointX) * _MaskScaleX + 0.5;
				//uvms.x = (uvm.x - 0.5  -  _MaskMidPointX - _GazePosX) * _MaskScaleX + 0.5;
				//uvms.y = uvm.y - _GazePosY;

				half4 peripheralColor = tex2D(_MainTex, uvp);
				peripheralColor.rgb = 0.5 + _PeripheralContrast * (peripheralColor.rgb - 0.5);
				half4 centralColor = tex2D(_MainTex, uvc);
				centralColor.rgb = 0.5 + _CentralContrast * (centralColor.rgb - 0.5);
				half4 maskColor = tex2D(_MaskTex, uvms);
				half4 destColor;
				if (_IsGray == 0)
				{
					destColor = lerp(peripheralColor, centralColor, maskColor.r);
				}
				else
				{
					if(maskColor.g <= 0.5)
					{
						destColor = lerp(peripheralColor, centralColor, maskColor.r);
					}
					else if (maskColor.g > 0.5)
					{
						destColor.rgb = 0.5; //gray
						destColor.a = 1.0;
					}
				}
				//set contrast (no contrast = gray)
				if (_Contrast <= 0.0)
					destColor.rgb = 0.0;
				else
					destColor.rgb = 0.5 + _Contrast * (destColor.rgb - 0.5);
				return destColor;
			}
			ENDCG
		}
	}
	Fallback off
	
} //shader
