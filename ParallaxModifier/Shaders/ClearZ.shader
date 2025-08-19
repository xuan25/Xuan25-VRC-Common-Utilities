Shader "Xuan25/ClearZ"
{
    Properties {
        _DepthOverrideValue ("Depth Override Value", Range(0, 1)) = 1.0
    }

	SubShader {
		Tags { "Queue" = "Background+100" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        LOD 100
        ZClip False     // Disable ZClip so that the object is not clipped by the near plane
        ZTest Always    // Disable Early-ZTest to always render regardless of depth, which allows us to render on top of everything

        Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

                float _DepthOverrideValue;

				struct appdata {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f {
					float4 position : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

                struct fout {
                    half4 color : SV_Target;
                    float depth : SV_Depth;
                };

                float4 _Color;
                float _ViewScalar;

				v2f vert (appdata IN) {
					v2f OUT;
                    
                    // ---- Transform the vertex position to clip space in a default way
					OUT.position = UnityObjectToClipPos(IN.vertex);
					OUT.uv = IN.uv;

					return OUT;
				}

				fout frag (v2f IN) {
                    fout OUT;
                    OUT.color = half4(0, 0, 0, 0);
                    OUT.depth = _DepthOverrideValue;
                    return OUT;
				}
			ENDCG
		}
	}
    FallBack "Diffuse"
}
