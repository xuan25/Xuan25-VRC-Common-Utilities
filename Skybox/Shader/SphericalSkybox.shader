Shader "Xuan25/SphericalSkybox"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _MainTex ("Spherical  (HDR)", 2D) = "grey" {}
        _OffsetAndTiling ("Offset and Tiling", Vector) = (0, 0, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "PreviewType" = "Skybox"
        }

        Cull Front
        ZWrite On

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_HDR;

            half4 _Tint;
            half _Exposure;
            float _Rotation;
            float4 _OffsetAndTiling;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            float3 RotateAroundY(float3 dir, float degrees)
            {
                float rad = degrees * UNITY_PI / 180.0;

                float s;
                float c;
                sincos(rad, s, c);

                return float3(
                    dir.x * c - dir.z * s,
                    dir.y,
                    dir.x * s + dir.z * c
                );
            }

            float2 DirectionToSphericalUV(float3 dir)
            {
                dir = normalize(dir);

                float longitude = atan2(dir.z, dir.x);
                float latitude = asin(dir.y);

                float2 uv;
                uv.x = 0.5 - longitude / (2.0 * UNITY_PI);
                uv.y = 0.5 + latitude / UNITY_PI;

                return uv;
            }

            v2f vert(appdata v)
            {
                v2f o;

                // Mesh is projected as normal, 
                // therefore, the part of the mesh that covers the screen changes with the camera.
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Texture sampling direction is not based on the mesh's local coordinates,
                // but rather the world-space direction from the camera to the vertex.
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 viewRay = worldPos - _WorldSpaceCameraPos.xyz;

                o.dir = RotateAroundY(viewRay, _Rotation);

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 uv = DirectionToSphericalUV(i.dir);

                uv = uv * _OffsetAndTiling.zw + _OffsetAndTiling.xy;

                half4 tex = tex2D(_MainTex, uv);
                half3 color = DecodeHDR(tex, _MainTex_HDR);

                color *= _Tint.rgb * unity_ColorSpaceDouble.rgb;
                color *= _Exposure;

                return half4(color, 1.0);
            }

            ENDCG
        }
    }

    Fallback Off
}