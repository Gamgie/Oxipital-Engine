#include "OxipitalHelpers.hlsl"

#ifndef COMPUTE_ALL_FORCE_INFLUENCE_H
#define COMPUTE_ALL_FORCE_INFLUENCE_H

float ComputeAllForceInfluence(in StructuredBuffer<float> buffer, in VFXCurve curve, in float3 position)
{
    float totalForce = 0;
    int dancerCount = buffer[0];
    int dancerStartIndex = buffer[1];

    for (int i = 0; i < dancerCount; ++i)
    {
        float3 centerPosition = GetDVector(i,0);
        float3 toCenterVector = centerPosition - position;
        float distanceToCenter = length(toCenterVector);

        totalForce += computeForceInfluence(distanceToCenter, buffer, curve, i);
    }

    return totalForce;
}


#endif // COMPUTE_ALL_FORCE_INFLUENCE_H