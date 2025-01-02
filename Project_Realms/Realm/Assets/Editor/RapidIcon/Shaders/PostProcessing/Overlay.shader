Shader "RapidIcon/PostProcessing/Overlay"
{
    Properties
    {
		[NoScaleOffset]
		_MainTex("Tex", 2D) = "black" {}
		[NoScaleOffset]
		_OverlayTex("Overlay Image", 2D) = "black" {}
		
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
				float2 uvOverlay : TEXCOORD1;
            };

            struct v2f
            {
				float2 uvMain : TEXCOORD0;
				float2 uvOverlay : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

			float4 _MainTex_TexelSize;
			float4 _OverlayTex_TexelSize;
			int _MatchAspect;
			float _Scale;
			float _OffsetX;
			float _OffsetY;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uvMain = v.uvMain;
				o.uvOverlay = v.uvOverlay;
				
				o.uvOverlay.x -= _OffsetX;
				o.uvOverlay.y -= _OffsetY;

				o.uvOverlay = (o.uvOverlay - 0.5) / _Scale + 0.5;
				
				if (_OverlayTex_TexelSize.z > _OverlayTex_TexelSize.w)
				{
					float aspectScale = _OverlayTex_TexelSize.x / _OverlayTex_TexelSize.y;
					o.uvOverlay.y = (o.uvOverlay.y - 0.5) / aspectScale + 0.5;
				}
				else
				{
					float aspectScale = _OverlayTex_TexelSize.y / _OverlayTex_TexelSize.x;
					o.uvOverlay.x = (o.uvOverlay.x - 0.5) / aspectScale + 0.5;
				}

				if (!_MatchAspect) {
					if (_MainTex_TexelSize.z > _MainTex_TexelSize.w)
					{
						float aspectScale = _MainTex_TexelSize.y / _MainTex_TexelSize.x;
						o.uvOverlay.y = (o.uvOverlay.y - 0.5) / aspectScale + 0.5;
					}
					else
					{
						float aspectScale = _MainTex_TexelSize.x / _MainTex_TexelSize.y;
						o.uvOverlay.x = (o.uvOverlay.x - 0.5) / aspectScale + 0.5;
					}
				}

                return o;
            }

            sampler2D _MainTex;
			sampler2D _OverlayTex;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 main = tex2D(_MainTex, i.uvMain);
				fixed4 overlay = tex2D(_OverlayTex, i.uvOverlay);

				fixed4 output;
				if (i.uvOverlay.x < 0 || i.uvOverlay.x > 1 || i.uvOverlay.y < 0 || i.uvOverlay.y > 1)
					output = main;
				else
				{
					if (overlay.a > 0)
						output.rgb = lerp(main.rgb, overlay.rgb, overlay.a);
					else
						output.rgb = main;

					output.a = 1 - ((1 - main.a)*(1 - overlay.a));
				}
				return output;
            }
            ENDCG
        }
    }
}
