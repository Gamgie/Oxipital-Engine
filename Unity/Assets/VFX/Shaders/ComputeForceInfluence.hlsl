float ComputeForceInfluence(in float3 particlePosition, in float forceRadius, in StructuredBuffer<float3> forceCenterBuffer)
{
    float totalForce = 0;
    int centerCount = forceCenterBuffer.Length;

    for (int i = 0; i < centerCount; ++i)
    {
        float distanceToCenter = length(forceCenterBuffer[i] - particlePosition);
		
        if (distanceToCenter > forceRadius)
            return 0;
		
        totalForce += max(1 - distanceToCenter / forceRadius, 0);
    }
    
    return totalForce;
}
