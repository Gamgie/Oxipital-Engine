#include "VFXCommon.hlsl"

float3 VortexForce(in float4x4 parameters, in float3 pos, in float3 axis, in StructuredBuffer<float3> forceCenterBuffer)
{
	float3 totalForce = float3(0.0, 0.0, 0.0);
	int centerCount = forceCenterBuffer.Length;
	float intensity = parameters[0][0];
	float radius = parameters[1][0];
	float orthoScale = parameters[2][0];
	float axialScale = parameters[3][0];

	bool clockWise = parameters[0][1];
	bool squaredOrtho = parameters[1][1];
	float innerRadius = parameters[2][1];

	if (innerRadius == 0)
    {
		innerRadius = 0.01;
	}
	
	for (int i = 0; i < centerCount; ++i)
    {
		float3 center = forceCenterBuffer[i];
        float3 toCenter = center - pos;
        float distanceToCenter = length(toCenter);
		
		float3 normalizedToCenter = toCenter / distanceToCenter;
		float normalizedDistance = distanceToCenter / (radius*innerRadius);
		float clockWiseFactor = clockWise == true ? 1 : -1;

		// Radial force (proportional to distance)
		//float3 radialForce = intensity * (1/(distance+1)) * normalizedToCenter;

		//Axial force (proportional to distance and pointed toward axis)
		float3 axialVector = ClosestPointOnALine(pos, axis, center);
		float3 axialForce = (normalizedDistance * intensity * axialScale) * normalize(axialVector);

       	// Orthoradial force (inversely proportional to the square of the distance)
		float orthoFactor = squaredOrtho == true ? normalizedDistance*normalizedDistance : normalizedDistance;
        float3 orthogonalVector = normalize(cross(normalizedToCenter, axis) * clockWiseFactor);
        float3 orthoradialForce = (intensity * orthoScale * orthogonalVector) / (orthoFactor);

		if(distanceToCenter > radius)
        {
			axialForce = float3(0,0,0);
			orthoradialForce = float3(0,0,0);
		}

        // Total force contribution from this center
        totalForce += axialForce + orthoradialForce;
		
	}
	
	return totalForce;
}


