/// MSDF.cginc
/// Multi-channel signed distance field helper functions

float _PixelRange;

float median(float3 col) {
    return max(min(col.r, col.g), min(max(col.r, col.g), col.b));
}

float4 decodeMSDF(float3 msdfData, float2 texelSize, float2 uv) {
    float2 msdfUnit = _PixelRange / texelSize;
    float sigDist = median(msdfData) - 0.5;
    sigDist *= max(dot(msdfUnit, 0.5 / fwidth(uv)), 1); // Max to handle fading out to quads in the distance
    float opacity = clamp(sigDist + 0.5, 0.0, 1.0);
    return float4(1, 1, 1, opacity);
}
