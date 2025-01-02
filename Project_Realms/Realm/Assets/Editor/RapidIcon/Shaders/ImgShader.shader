Shader "RapidIcon/ImgShader" 
{
	Properties
	{
		_MainTex("Tex", 2D) = "white" {}
		_UseDepthTexture("UseDepthTexture", int) = 0
	}
		SubShader
	{
		Tags { "Queue" = "Transparent" "PreviewType" = "Plane" }

		//LOD 100

		ZWrite Off
		Blend One OneMinusSrcAlpha

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
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			sampler2D _LastCameraDepthTexture;
			int _UseDepthTexture;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				half4 col = tex2D(_MainTex, i.uv);

				if (col.a > 0 && col.a < 1)
				{
					col.r /= col.a;
					col.g /= col.a;
					col.b /= col.a;
				}

				float depth = tex2D(_LastCameraDepthTexture, i.uv).r;
				depth = Linear01Depth(depth);
				depth = 1-depth;
				if(depth > 0.01)
					depth = 1;
				
				if(_UseDepthTexture == 1)
				{
					col.a*=depth;
				}

				return col;
			}
			ENDCG
		}
	}
}

