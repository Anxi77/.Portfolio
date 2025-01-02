Shader "RapidIcon/PostProcessing/Underlay"
{
    Properties
    {
		[NoScaleOffset]
		_MainTex("Tex", 2D) = "black" {}
		[NoScaleOffset]
		_UnderlayTex("Underlay Image", 2D) = "black" {}
		
		[Toggle]
		_MatchAspect("Match Image Aspect Ratio", Int) = 1
		_Scale("Scale", Float) = 1
		_OffsetX("X Offset", Float) = 0
		_OffsetY("Y Offset", Float) = 0
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
                float2 uvMain : TEXCOORD0;
				float2 uvUnderlay : TEXCOORD1;
            };

            struct v2f
            {
				float2 uvMain : TEXCOORD0;
				float2 uvUnderlay : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

			float4 _MainTex_TexelSize;
			float4 _UnderlayTex_TexelSize;
			int _MatchAspect;
			float _Scale;
			float _OffsetX;
			float _OffsetY;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uvMain = v.uvMain;
				o.uvUnderlay = v.uvUnderlay;
				
				o.uvUnderlay.x -= _OffsetX;
				o.uvUnderlay.y -= _OffsetY;

				o.uvUnderlay = (o.uvUnderlay - 0.5) / _Scale + 0.5;
				
				if (_UnderlayTex_TexelSize.z > _UnderlayTex_TexelSize.w)
				{
					float aspectScale = _UnderlayTex_TexelSize.x / _UnderlayTex_TexelSize.y;
					o.uvUnderlay.y = (o.uvUnderlay.y - 0.5) / aspectScale + 0.5;
				}
				else
				{
					float aspectScale = _UnderlayTex_TexelSize.y / _UnderlayTex_TexelSize.x;
					o.uvUnderlay.x = (o.uvUnderlay.x - 0.5) / aspectScale + 0.5;
				}

				if (!_MatchAspect) {
					if (_MainTex_TexelSize.z > _MainTex_TexelSize.w)
					{
						float aspectScale = _MainTex_TexelSize.y / _MainTex_TexelSize.x;
						o.uvUnderlay.y = (o.uvUnderlay.y - 0.5) / aspectScale + 0.5;
					}
					else
					{
						float aspectScale = _MainTex_TexelSize.x / _MainTex_TexelSize.y;
						o.uvUnderlay.x = (o.uvUnderlay.x - 0.5) / aspectScale + 0.5;
					}
				}

                return o;
            }

            sampler2D _MainTex;
			sampler2D _UnderlayTex;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 main = tex2D(_MainTex, i.uvMain);
				fixed4 underlay = tex2D(_UnderlayTex, i.uvUnderlay);

				fixed4 output;
				if (i.uvUnderlay.x < 0 || i.uvUnderlay.x > 1 || i.uvUnderlay.y < 0 || i.uvUnderlay.y > 1)
					output = main;
				else
				{
					if (underlay.a > 0)
						output.rgb = lerp(underlay.rgb, main.rgb, main.a);
					else
						output.rgb = main;
					output.a = 1 - ((1 - main.a)*(1 - underlay.a));
				}
				return output;
            }
            ENDCG
        }
    }
}
