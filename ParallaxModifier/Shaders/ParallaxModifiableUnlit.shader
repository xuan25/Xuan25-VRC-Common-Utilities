Shader "Unlit/ParallaxModifiableUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ViewScaling ("Distance Feel (bigger = farther)", Float) = 1.0
        _ViewTransformAnchorBlend ("Anchor Blend (0=Scale from World, 1=Scale from Model)", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Background+50" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            // Include the Parallax Modifier functions
            #include "Includes/ParallaxModifier.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            // Parallax modifier parameters
            float  _ViewScaling;
            float  _ViewTransformAnchorBlend;

            v2f vert (appdata v)
            {
                v2f o;
                
                // Transform the vertex position using the parallax modifier
                v.vertex = ParallaxModifier(v.vertex, _ViewScaling, _ViewTransformAnchorBlend);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
