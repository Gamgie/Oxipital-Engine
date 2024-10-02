float ComputeForceInfluence(in float3 particlePosition, in float forceRadius, in StructuredBuffer<float3> forceCenterBuffer, in VFXCurve curve)
{
    float totalForce = 0;
    int centerCount = forceCenterBuffer.Length;

    for (int i = 0; i < centerCount; ++i)
    {
        float distanceToCenter = length(forceCenterBuffer[i] - particlePosition);
		
        if (distanceToCenter > forceRadius)
            continue;
		
        totalForce += SampleCurve(curve, distanceToCenter / forceRadius);
    }
    totalForce = min(totalForce,1);

    return totalForce;
}
