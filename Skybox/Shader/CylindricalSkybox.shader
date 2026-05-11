Shader "Xuan25/CylindricalSkybox"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        _MainTex ("Cylindrical (HDR)", 2D) = "grey" {}

        _AltTint ("Alt Tint Color", Color) = (.5, .5, .5, .5)
        [Gamma] _AltExposure ("Alt Exposure", Range(0, 8)) = 1.0
        _AltRotation ("Alt Rotation", Range(0, 360)) = 0
        _AltTex ("Alt Cylindrical (HDR)", 2D) = "grey" {}

        [KeywordEnum(Lerp, Superposition, ArgMax, Map)] _BlendMode ("Blend Mode", Float) = 0
        _BlendFactor ("Blend Factor", Range(0,1)) = 0

        _ArgMaxFeather ("ArgMax Feather", Range(0, 1)) = 0.3

        _BlendMap ("Blend Map", 2D) = "white" {}
        _BlendMapFeather ("Blend Map Feather", Range(0, 1)) = 0.05

        [Toggle(_ENABLE_LOD)] _EnableLOD ("Enable LOD", Float) = 0
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
            #pragma target 3.0

            #include "UnityCG.cginc"

            #pragma shader_feature_local _BLENDMODE_LERP _BLENDMODE_SUPERPOSITION _BLENDMODE_ARGMAX _BLENDMODE_MAP
            #pragma shader_feature_local _ENABLE_LOD

            sampler2D _MainTex;
            float4 _MainTex_HDR;
            float4 _MainTex_ST;

            half4 _Tint;
            half _Exposure;
            float _Rotation;

            sampler2D _AltTex;
            float4 _AltTex_HDR;
            float4 _AltTex_ST;

            half4 _AltTint;
            half _AltExposure;
            float _AltRotation;

            float _BlendFactor;

            float _ArgMaxFeather;

            sampler2D _BlendMap;
            float4 _BlendMap_ST;

            float _BlendMapFeather;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 dir : TEXCOORD0;
                float3 altDir : TEXCOORD1;
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

            float2 ApplyOffsetTiling(float2 uv, float4 offsetTiling)
            {
                uv = uv * offsetTiling.xy + offsetTiling.zw;

                uv.x = frac(uv.x);
                uv.y = saturate(uv.y);

                return uv;
            }

#if _ENABLE_LOD

            void DirectionToSphericalUVWithGrad(
                float3 dir,
                out float2 uv,
                out float2 dUVdx,
                out float2 dUVdy)
            {
                float3 n = normalize(dir);

                // compute screen-space derivatives of the continuous normalized direction
                float3 dn_dx = ddx(n);
                float3 dn_dy = ddy(n);

                float x = n.x;
                float y = n.y;
                float z = n.z;

                float longitude = atan2(z, x);
                float latitude  = asin(clamp(y, -1.0, 1.0));

                uv.x = 0.5 - longitude / (2.0 * UNITY_PI);
                uv.y = 0.5 + latitude / UNITY_PI;

                // longitude = atan2(z, x)
                // d longitude = (x dz - z dx) / (x^2 + z^2)
                float denomLon = max(x * x + z * z, 1e-6);

                float dLongitude_dx = (x * dn_dx.z - z * dn_dx.x) / denomLon;
                float dLongitude_dy = (x * dn_dy.z - z * dn_dy.x) / denomLon;

                // latitude = asin(y)
                // d latitude = dy / sqrt(1 - y^2)
                float denomLat = sqrt(max(1.0 - y * y, 1e-6));

                float dLatitude_dx = dn_dx.y / denomLat;
                float dLatitude_dy = dn_dy.y / denomLat;

                dUVdx = float2(
                    -dLongitude_dx / (2.0 * UNITY_PI),
                    dLatitude_dx  / UNITY_PI
                );

                dUVdy = float2(
                    -dLongitude_dy / (2.0 * UNITY_PI),
                    dLatitude_dy  / UNITY_PI
                );
            }

            void ApplyOffsetTilingWithGrad(
                float2 baseUV,
                float2 baseDx,
                float2 baseDy,
                float4 offsetTiling,
                out float2 uv,
                out float2 uvDx,
                out float2 uvDy)
            {
                uv   = baseUV * offsetTiling.xy + offsetTiling.zw;
                uvDx = baseDx * offsetTiling.xy;
                uvDy = baseDy * offsetTiling.xy;

                // U direction is the wrap-around direction
                uv.x = frac(uv.x);

                // V direction is usually clamped
                uv.y = saturate(uv.y);
            }

#else


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

#endif

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
                o.altDir = RotateAroundY(viewRay, _AltRotation);

                return o;
            }

            float luminance(float3 color)
            {
                return dot(color, float3(0.2126, 0.7152, 0.0722));
            }

            half4 frag(v2f i) : SV_Target
            {

#if _ENABLE_LOD

                // --- Main UV + Grad ---
                float2 mainBaseUV;
                float2 mainBaseDx;
                float2 mainBaseDy;
                DirectionToSphericalUVWithGrad(i.dir, mainBaseUV, mainBaseDx, mainBaseDy);

                float2 mainUV;
                float2 mainDx;
                float2 mainDy;
                ApplyOffsetTilingWithGrad(
                    mainBaseUV,
                    mainBaseDx,
                    mainBaseDy,
                    _MainTex_ST,
                    mainUV,
                    mainDx,
                    mainDy
                );

                // --- Alt UV + Grad ---
                float2 altBaseUV;
                float2 altBaseDx;
                float2 altBaseDy;
                DirectionToSphericalUVWithGrad(i.altDir, altBaseUV, altBaseDx, altBaseDy);

                float2 altUV;
                float2 altDx;
                float2 altDy;
                ApplyOffsetTilingWithGrad(
                    altBaseUV,
                    altBaseDx,
                    altBaseDy,
                    _AltTex_ST,
                    altUV,
                    altDx,
                    altDy
                );

                // --- Sampling with explicit gradients ---
                half4 mainTex = tex2Dgrad(_MainTex, mainUV, mainDx, mainDy);
                float3 mainColor = DecodeHDR(mainTex, _MainTex_HDR);

                half4 altTex = tex2Dgrad(_AltTex, altUV, altDx, altDy);
                float3 altColor = DecodeHDR(altTex, _AltTex_HDR);

#else

                float2 mainUV = DirectionToSphericalUV(i.dir);
                float2 altUV = DirectionToSphericalUV(i.altDir);

                mainUV = mainUV * _MainTex_ST.xy + _MainTex_ST.zw;
                altUV = altUV * _AltTex_ST.xy + _AltTex_ST.zw;

                half4 mainTex = tex2Dlod(_MainTex, float4(mainUV, 0, 0));
                float3 mainColor = DecodeHDR(mainTex, _MainTex_HDR);

                half4 altTex = tex2Dlod(_AltTex, float4(altUV, 0, 0));
                float3 altColor = DecodeHDR(altTex, _AltTex_HDR);

#endif

                mainColor *= _Tint.rgb * unity_ColorSpaceDouble.rgb;
                mainColor *= _Exposure;

                altColor *= _AltTint.rgb * unity_ColorSpaceDouble.rgb;
                altColor *= _AltExposure;

                // Blend

#if _BLENDMODE_LERP
                    float3 color = lerp(mainColor, altColor, _BlendFactor);
#elif _BLENDMODE_SUPERPOSITION
                    float3 color;
                    if (_BlendFactor < 0.5)
                    {
                        // alt fade in 
                        color = max(mainColor, altColor * _BlendFactor * 2);
                    }
                    else
                    {
                        // main fade out
                        color = max(mainColor * (1 - (_BlendFactor - 0.5) * 2), altColor);
                    }
#elif _BLENDMODE_ARGMAX
                    // float3 color = (luminance(altColor * _BlendFactor) > luminance(mainColor * (1 - _BlendFactor))) ? altColor : mainColor;

                    // with feathering
                    float mainScore = luminance(mainColor * (1.0 - _BlendFactor));
                    float altScore  = luminance(altColor * _BlendFactor);

                    float scale = max(max(mainScore, altScore), 1e-4);
                    float diff = (altScore - mainScore) / scale;

                    float feather = _ArgMaxFeather;
                    float t = smoothstep(-feather, feather, diff);

                    float3 color = lerp(mainColor, altColor, t);
#elif _BLENDMODE_MAP
                    maskUV = ApplyOffsetTiling(mainUV, _BlendMap_ST);
                    
                    float blendThreshold = tex2Dlod(_BlendMap, float4(maskUV, 0, 0)).r;
                    float diff = _BlendFactor - blendThreshold;
                    float feather = _BlendMapFeather;
                    float t = smoothstep(-feather, feather, diff);
                    float3 color = lerp(mainColor, altColor, t);
                    
#else
                    float3 color = mainColor;
#endif

                return half4(color, 1.0);
            }

            ENDCG
        }
    }

    Fallback Off
}