Shader "Xuan25/ParallaxModifierSkybox"
{
    Properties {
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _MainTex ("Spherical  (HDR)", 2D) = "grey" {}
        _OffsetAndTiling ("Offset and Tiling", Vector) = (0, 0, 1, 1)

        // Artist-facing control:
        //   Larger value  → object *feels farther* (camera/model translations shrink more)
        //   Smaller value → object *feels closer*  (camera/model translations shrink less)
        //
        // NOTE: We *invert* this in shader (s = 1/u) so the “feels farther when bigger”
        // mapping stays intuitive across modes.
        _ViewScaling ("Distance Feel (bigger = farther)", Float) = 1.0

        // Blend between two translation-only modes:
        //   0.0 → Mode A (camera-only translation scaling)
        //   1.0 → Mode B (camera + model translation scaling)
        // Values in between linearly interpolate the *translation scaling factors*
        // (not the clip-space positions!), which is more stable than lerping positions.
        _ViewTransformModeBlend ("Mode Blend (0=A camera only, 1=B camera+model)", Range(0,1)) = 1.0
    }

    SubShader {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Front
        ZWrite Off

        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            half4 _MainTex_HDR;
            half4 _Tint;
            half _Exposure;
            float _Rotation;
            float4 _OffsetAndTiling;

            float  _ViewScaling;            // “bigger = farther” (we’ll invert in code)
            float  _ViewTransformModeBlend; // t in [0,1], blend A→B
            

            inline float2 ToRadialCoords(float3 coords)
            {
                float3 normalizedCoords = normalize(coords);
                float latitude = acos(normalizedCoords.y);
                float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
                float2 sphereCoords = float2(longitude, latitude) * float2(0.5/UNITY_PI, 1.0/UNITY_PI);
                return float2(0.5,1.0) - sphereCoords;
            }

            float3 RotateAroundYInDegrees (float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            struct appdata_t {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 position : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // ─────────────────────────────────────────────────────────────────────
            // Helper: scale only the *translation column* of a 4x4 matrix.
            //
            // Why only the translation column?
            //   Let V*M = A x + b, where A is the 3x3 linear block (rotation+scale),
            //   and b is the 3x1 translation. This function scales b ← s·b while
            //   leaving A unchanged. That gives the “distance/parallax feel” without
            //   changing geometry, FOV, or clip planes.
            //
            // Unity matrices are column-major in shaders:
            //   translation lives in the 4th column → (_m03, _m13, _m23).
            // ─────────────────────────────────────────────────────────────────────
            inline float4x4 ScaleTranslationOnly(float4x4 M, float s)
            {
                M._m03 *= s;   // tx
                M._m13 *= s;   // ty
                M._m23 *= s;   // tz
                return M;
            }

            v2f vert (appdata_t input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float3 rotated = RotateAroundYInDegrees(input.vertex, _Rotation);
                // output.position = UnityObjectToClipPos(rotated);

                // Grab the standard matrices.
                // We will *not* touch the projection matrix (UNITY_MATRIX_P),
                // so near/far planes and FOV remain exactly as on the camera.
                float4x4 M = UNITY_MATRIX_M;
                float4x4 V = UNITY_MATRIX_V;
                float4x4 P = UNITY_MATRIX_P;

                // ─────────────────────────────────────────────────────────────────
                // 1) Map the artist control to a translation scale “s”
                //
                // Design goal: bigger _ViewScaling (u) should feel *farther*.
                // We realize that by using s = 1/u so:
                //   u ↑  →  s ↓  →  translations shrink more  →  motion/parallax looks smaller.
                //
                // EPS guards division-by-zero; for very extreme ranges (1e5+), you
                // can tighten to 1e-8. Keep float precision (don’t downcast to half).
                // ─────────────────────────────────────────────────────────────────
                const float EPS = 1e-8;
                float u = _ViewScaling;
                float s = rcp(max(u, EPS));      // s = 1/u

                // ─────────────────────────────────────────────────────────────────
                // 2) Choose per-mode translation scaling factors
                //
                // Mode A (camera-only):
                //   - Camera translation scaled by s
                //   - Model translation unchanged (1.0)
                //
                // Mode B (camera + model):
                //   - Both camera and model translations scaled by s
                //
                // Blend t in [0,1] mixes the *factors*, not positions:
                //   s_cam   = lerp(sA, sB, t) with sA=s, sB=s  → equals s (camera always scaled)
                //   s_model = lerp(1,  sB, t) with sB=s       → 1→s across the blend
                //
                // This “single-path MVP” avoids lerping two clip-space results,
                // which would introduce perspective nonlinearity.
                // ─────────────────────────────────────────────────────────────────
                float t       = saturate(_ViewTransformModeBlend);
                float s_cam   = s;               // camera translation always scaled
                float s_model = lerp(1.0, s, t); // model translation: A=1 → B=s

                // ─────────────────────────────────────────────────────────────────
                // 3) Build the composite transform with *translation-only* scaling applied
                // ─────────────────────────────────────────────────────────────────
                float4x4 M2  = ScaleTranslationOnly(M, s_model);
                float4x4 V2  = ScaleTranslationOnly(V, s_cam);
                float4x4 VP  = mul(P, V2);       // keep P intact to preserve clip planes

                // One final multiply to clip space.
                output.position = mul(VP, mul(M2, input.vertex));
                

                output.texcoord = input.vertex.xyz;

                return output;
            }

            half4 frag (v2f i) : SV_Target
            {
                // float2 tc = ToRadialCoords(i.texcoord);
                float2 tc = ToRadialCoords(RotateAroundYInDegrees(i.texcoord, _Rotation));
                tc = (tc + _OffsetAndTiling.xy) * _OffsetAndTiling.zw;

                half4 tex = tex2D(_MainTex, tc);
                half3 c = DecodeHDR(tex, _MainTex_HDR);
                c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                c *= _Exposure;
                return half4(c, 1);
            }
            ENDCG
        }
    }

    Fallback Off
}
