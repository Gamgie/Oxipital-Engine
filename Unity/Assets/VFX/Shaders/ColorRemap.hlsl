#include "VFXHelper.hlsl"

float2 RemapFunction(in float3 position, in float3 emitterPosition, in float3 emitterSize)
{
    float2 UVposition = float2(0.0,0.0);
    
    emitterSize *= 2;
    
    // Compute position relative to emitter center
    float3 localPos = position - emitterPosition;

    // Convert from [-0.5 * size, +0.5 * size] → [0,1]
    UVposition.x = (position.x - (emitterPosition.x - 0.5 * emitterSize.x)) / emitterSize.x;
    UVposition.y = (position.y - (emitterPosition.y - 0.5 * emitterSize.y)) / emitterSize.y;
    
    
    // clamp to avoid sampling outside texture
    UVposition = saturate(UVposition);
    

  return UVposition;
}