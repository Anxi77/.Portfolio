Shader "RapidIcon/ObjectRender"
{
	Properties
	{
		[NoScaleOffset]
		_MainTex("Tex", 2D) = "black" {}

		[NoScaleOffset]
		_Render("Object Render", 2D) = "black" {}
	}
	SubShader
	{
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
			sampler2D _Render;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 render = tex2D(_Render, i.uv);

				fixed4 output;
				output.rgb = lerp(col.rgb, render.rgb, render.a);
				output.a = 1 - ((1 - col.a)*(1 - render.a));
				return output;
			}
			ENDCG
		}
	}
}
