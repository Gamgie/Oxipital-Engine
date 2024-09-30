#include "VFXCommon.hlsl"

float2 RemapFunction(in float3 position, in float3 emitterPosition)
{
    float2 UVposition = float2(0.0,0.0);

    // X 
    UVposition.x = remapFloat(position.x, emitterPosition.x - 0.5, emitterPosition.x + 0.5, 0, 1);
    // Y 
    UVposition.y = remapFloat(position.z, emitterPosition.z - 0.5, emitterPosition.z + 0.5, 0, 1);

  return UVposition;
}