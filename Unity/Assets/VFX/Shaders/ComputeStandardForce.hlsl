#include "VFXCommon.hlsl"
#include "ComputeAxialForce.hlsl"

void StandardForce(inout VFXAttributes attributes, in StructuredBuffer<float> floatBuffer, in StructuredBuffer<float3> vector3Buffer, in float3 pos, in StructuredBuffer<float3> forceCenterBuffer)
{
    float3 totalForce = float3(0.0, 0.0, 0.0);
    int centerCount = forceCenterBuffer.Length;
    float intensity = floatBuffer[0];
    
    float3 axis = vector3Buffer[0];
    float radius = floatBuffer[1];
    
    // Radial Force is attracting towards the center
    float radialForceIntensity = floatBuffer[2];;

    // Orthoradial Force is pushing particle on the orthogonal vector of the center vector
    float orthoForceIntensity = floatBuffer[3];
    int orthoFactor = floatBuffer[4];
    float innerRadius = floatBuffer[5];
    bool clockWise = floatBuffer[6];
    
    // Linear Force is attracting towards the same direction
    float linearForceIntensity = floatBuffer[7];

    // Spiral Force
    float spiralForceIntensity = floatBuffer[13];

    if (innerRadius == 0)
    {
        innerRadius = 0.01;
    }
	
    for (int i = 0; i < centerCount; ++i)
    {
        float3 centerPosition = forceCenterBuffer[i];
        float3 toCenterVector = centerPosition - attributes.position;
        float distanceToCenter = length(toCenterVector);
		
        float3 normalizedToCenterVector = toCenterVector / distanceToCenter;
        float normalizedDistance = distanceToCenter / (radius * innerRadius);
        float clockWiseFactor = clockWise == true ? 1 : -1;

		float3 radialForce = intensity * radialForceIntensity * (1/(distanceToCenter+1)) * normalizedToCenterVector;

        float3 axialForce = ComputeAxialForce(floatBuffer, vector3Buffer, normalizedDistance, attributes.position, centerPosition);

       	// Orthoradial force (inversely proportional to the square of the distance)
        float3 orthogonalVector = normalize(cross(normalizedToCenterVector, axis) * clockWiseFactor);
        float3 orthoradialForce = (intensity * orthoForceIntensity * orthogonalVector) / (pow(abs(normalizedDistance), abs(orthoFactor)));

        // Total force contribution from this center
        totalForce += axialForce + orthoradialForce + radialForce;
    }
	
    return;
}


