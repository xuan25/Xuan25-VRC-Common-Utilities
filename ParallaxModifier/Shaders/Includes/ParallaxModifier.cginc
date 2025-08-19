// Functions
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

// Safe 4x4 matrix inverse for Unity HLSL
float4x4 Inverse4x4(float4x4 m)
{
    float4x4 r;
    float det;

    r[0][0] =  m[1][1] * m[2][2] * m[3][3] - 
            m[1][1] * m[2][3] * m[3][2] - 
            m[2][1] * m[1][2] * m[3][3] + 
            m[2][1] * m[1][3] * m[3][2] +
            m[3][1] * m[1][2] * m[2][3] - 
            m[3][1] * m[1][3] * m[2][2];

    r[0][1] = -m[0][1] * m[2][2] * m[3][3] + 
            m[0][1] * m[2][3] * m[3][2] + 
            m[2][1] * m[0][2] * m[3][3] - 
            m[2][1] * m[0][3] * m[3][2] - 
            m[3][1] * m[0][2] * m[2][3] + 
            m[3][1] * m[0][3] * m[2][2];

    r[0][2] =  m[0][1] * m[1][2] * m[3][3] - 
            m[0][1] * m[1][3] * m[3][2] - 
            m[1][1] * m[0][2] * m[3][3] + 
            m[1][1] * m[0][3] * m[3][2] + 
            m[3][1] * m[0][2] * m[1][3] - 
            m[3][1] * m[0][3] * m[1][2];

    r[0][3] = -m[0][1] * m[1][2] * m[2][3] + 
            m[0][1] * m[1][3] * m[2][2] + 
            m[1][1] * m[0][2] * m[2][3] - 
            m[1][1] * m[0][3] * m[2][2] - 
            m[2][1] * m[0][2] * m[1][3] + 
            m[2][1] * m[0][3] * m[1][2];

    r[1][0] = -m[1][0] * m[2][2] * m[3][3] + 
            m[1][0] * m[2][3] * m[3][2] + 
            m[2][0] * m[1][2] * m[3][3] - 
            m[2][0] * m[1][3] * m[3][2] - 
            m[3][0] * m[1][2] * m[2][3] + 
            m[3][0] * m[1][3] * m[2][2];

    r[1][1] =  m[0][0] * m[2][2] * m[3][3] - 
            m[0][0] * m[2][3] * m[3][2] - 
            m[2][0] * m[0][2] * m[3][3] + 
            m[2][0] * m[0][3] * m[3][2] + 
            m[3][0] * m[0][2] * m[2][3] - 
            m[3][0] * m[0][3] * m[2][2];

    r[1][2] = -m[0][0] * m[1][2] * m[3][3] + 
            m[0][0] * m[1][3] * m[3][2] + 
            m[1][0] * m[0][2] * m[3][3] - 
            m[1][0] * m[0][3] * m[3][2] - 
            m[3][0] * m[0][2] * m[1][3] + 
            m[3][0] * m[0][3] * m[1][2];

    r[1][3] =  m[0][0] * m[1][2] * m[2][3] - 
            m[0][0] * m[1][3] * m[2][2] - 
            m[1][0] * m[0][2] * m[2][3] + 
            m[1][0] * m[0][3] * m[2][2] + 
            m[2][0] * m[0][2] * m[1][3] - 
            m[2][0] * m[0][3] * m[1][2];

    r[2][0] =  m[1][0] * m[2][1] * m[3][3] - 
            m[1][0] * m[2][3] * m[3][1] - 
            m[2][0] * m[1][1] * m[3][3] + 
            m[2][0] * m[1][3] * m[3][1] + 
            m[3][0] * m[1][1] * m[2][3] - 
            m[3][0] * m[1][3] * m[2][1];

    r[2][1] = -m[0][0] * m[2][1] * m[3][3] + 
            m[0][0] * m[2][3] * m[3][1] + 
            m[2][0] * m[0][1] * m[3][3] - 
            m[2][0] * m[0][3] * m[3][1] - 
            m[3][0] * m[0][1] * m[2][3] + 
            m[3][0] * m[0][3] * m[2][1];

    r[2][2] =  m[0][0] * m[1][1] * m[3][3] - 
            m[0][0] * m[1][3] * m[3][1] - 
            m[1][0] * m[0][1] * m[3][3] + 
            m[1][0] * m[0][3] * m[3][1] + 
            m[3][0] * m[0][1] * m[1][3] - 
            m[3][0] * m[0][3] * m[1][1];

    r[2][3] = -m[0][0] * m[1][1] * m[2][3] + 
            m[0][0] * m[1][3] * m[2][1] + 
            m[1][0] * m[0][1] * m[2][3] - 
            m[1][0] * m[0][3] * m[2][1] - 
            m[2][0] * m[0][1] * m[1][3] + 
            m[2][0] * m[0][3] * m[1][1];

    r[3][0] = -m[1][0] * m[2][1] * m[3][2] + 
            m[1][0] * m[2][2] * m[3][1] + 
            m[2][0] * m[1][1] * m[3][2] - 
            m[2][0] * m[1][2] * m[3][1] - 
            m[3][0] * m[1][1] * m[2][2] + 
            m[3][0] * m[1][2] * m[2][1];

    r[3][1] =  m[0][0] * m[2][1] * m[3][2] - 
            m[0][0] * m[2][2] * m[3][1] - 
            m[2][0] * m[0][1] * m[3][2] + 
            m[2][0] * m[0][2] * m[3][1] + 
            m[3][0] * m[0][1] * m[2][2] - 
            m[3][0] * m[0][2] * m[2][1];

    r[3][2] = -m[0][0] * m[1][1] * m[3][2] + 
            m[0][0] * m[1][2] * m[3][1] + 
            m[1][0] * m[0][1] * m[3][2] - 
            m[1][0] * m[0][2] * m[3][1] - 
            m[3][0] * m[0][1] * m[1][2] + 
            m[3][0] * m[0][2] * m[1][1];

    r[3][3] =  m[0][0] * m[1][1] * m[2][2] - 
            m[0][0] * m[1][2] * m[2][1] - 
            m[1][0] * m[0][1] * m[2][2] + 
            m[1][0] * m[0][2] * m[2][1] + 
            m[2][0] * m[0][1] * m[1][2] - 
            m[2][0] * m[0][2] * m[1][1];

    det = m[0][0] * r[0][0] + m[0][1] * r[1][0] + m[0][2] * r[2][0] + m[0][3] * r[3][0];

    r = r / det;
    return r;
}

float4 ParallaxModifier(float4 vertex, float viewScaling, float viewTransformAnchorBlend)
{
    // Grab the standard matrices.
    // We will *not* touch the projection matrix (UNITY_MATRIX_P),
    // so near/far planes and FOV remain exactly as on the camera.
    float4x4 M = UNITY_MATRIX_M;
    float4x4 V = UNITY_MATRIX_V;
    float4x4 P = UNITY_MATRIX_P;

    // ─────────────────────────────────────────────────────────────────
    // 1) Map the artist control to a translation scale “s”
    //
    // Design goal: bigger viewScaling (u) should feel *farther*.
    // We realize that by using s = 1/u so:
    //   u ↑  →  s ↓  →  translations shrink more  →  motion/parallax looks smaller.
    //
    // EPS guards division-by-zero; for very extreme ranges (1e5+), you
    // can tighten to 1e-8. Keep float precision (don’t downcast to half).
    // ─────────────────────────────────────────────────────────────────
    const float EPS = 1e-8;
    float u = viewScaling;
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
    float t       = saturate(viewTransformAnchorBlend);
    float s_cam   = s;               // camera translation always scaled
    float s_model = lerp(1.0, s, t); // model translation: A=1 → B=s

    // ─────────────────────────────────────────────────────────────────
    // 3) Build the composite transform with *translation-only* scaling applied
    // ─────────────────────────────────────────────────────────────────
    float4x4 M2  = ScaleTranslationOnly(M, s_model);
    float4x4 V2  = ScaleTranslationOnly(V, s_cam);
    float4x4 VP  = mul(P, V2);       // keep P intact to preserve clip planes

    // One final multiply to clip space.
    // o.pos = mul(VP, mul(M2, v.vertex));

    // apply the transform to v instead, and apply an inverted original MVP, 
    // so the following pipeline which use the original MVP will be cancelled out and use the new one instead
    
    // Build original and new MVPs
    float4x4 MVP_orig = mul(P, mul(V,  M));
    float4x4 MVP_new  = mul(P, mul(V2, M2));

    // Invert the original MVP
    float4x4 invMVP_orig = Inverse4x4(MVP_orig);

    // Pre-transform the vertex so that downstream use of MVP_orig
    // yields the same as using MVP_new on the original vertex.
    float4 clipNew = mul(MVP_new, vertex);
    return mul(invMVP_orig, clipNew);

    // (Don't touch o.pos here; let the usual pipeline run with the original MVP.)
}