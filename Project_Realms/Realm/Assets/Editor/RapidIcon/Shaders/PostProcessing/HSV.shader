Shader "RapidIcon/PostProcessing/HSV"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Hue("Hue", range(0,1)) = 0
		_Saturation("Saturation", range(0,1)) = 1
		_Value("Value", range(0,1)) = 1
	}
	
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			float _Hue;
			float _Saturation;
			float _Value;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				float pi = 3.14159265359;
				float U = cos(2*_Hue*pi);
				float W = sin(2*_Hue*pi);

				float4 newCol;
				newCol.r = (.299 + .701*U + .168*W)*col.r
					+ (.587 - .587*U + .330*W)*col.g
					+ (.114 - .114*U - .497*W)*col.b;
				newCol.g = (.299 - .299*U - .328*W)*col.r
					+ (.587 + .413*U + .035*W)*col.g
					+ (.114 - .114*U + .292*W)*col.b;
				newCol.b = (.299 - .3*U + 1.25*W)*col.r
					+ (.587 - .588*U - 1.05*W)*col.g
					+ (.114 + .886*U - .203*W)*col.b;
				
				float grayscale;
				grayscale = dot(float3(0.222, 0.707, 0.071), newCol);

				float3 desatCol;
				desatCol = float3(grayscale, grayscale, grayscale);
				
				col.rgb = lerp(desatCol, newCol.rgb, _Saturation)*_Value;

				return col;
			}

			ENDCG
		}
	}
}



