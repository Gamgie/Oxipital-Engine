#include "VFXHelper.hlsl"

float2 RemapFunction(in float3 position, in float3 emitterPosition, in float3 emitterSize)
{
    float2 UVposition = float2(0.0,0.0);

    // X 
    UVposition.x = remapFloat(position.x, (emitterPosition.x - 0.5) * emitterSize.x*2, (emitterPosition.x + 0.5) * emitterSize.x*2, 0, 1);
    // Y 
    UVposition.y = remapFloat(position.y, (emitterPosition.y - 0.5) * emitterSize.y*2, (emitterPosition.y + 0.5) * emitterSize.y*2, 0, 1); 

  return UVposition;
}