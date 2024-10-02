#include "VFXCommon.hlsl"
#include "ComputeAxialForce.hlsl"

void StandardForce(inout VFXAttributes attributes, in StructuredBuffer<float> floatBuffer, in StructuredBuffer<float3> vector3Buffer, in StructuredBuffer<float3> forceCenterBuffer)
{
    float3 totalForce = float3(0.0, 0.0, 0.0);
    int centerCount = forceCenterBuffer.Length;
    float intensity = floatBuffer[0];
    
    float3 axis = vector3Buffer[0];
    float radius = floatBuffer[1];
    
    // Radial Force is attracting towards the center
    float radialIntensity = floatBuffer[2];

    // Axial Parameters
    float axialIntensity = floatBuffer[4];
    float axialFactor = floatBuffer[5];
    float3 axialFrequency = vector3Buffer[1];

    // Orthoradial Force is pushing particle on the orthogonal vector of the center vector
    float orthoIntensity = floatBuffer[7];
    int orthoFactor = floatBuffer[9];
    float innerRadius = floatBuffer[8];
    bool clockWise = floatBuffer[10];
    
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

		float3 radialForce = intensity * radialIntensity * (1/(distanceToCenter+1)) * normalizedToCenterVector;

        
        float3 axialForce = intensity * axialIntensity * ComputeAxialForce(attributes.position, axis, normalizedDistance, centerPosition, axialFrequency, axialFactor);

       	// Orthoradial force (inversely proportional to the distance)
        float3 orthogonalVector = normalize(cross(normalizedToCenterVector, axis) * clockWiseFactor);
        float3 orthoradialForce = (intensity * orthoIntensity * orthogonalVector) / (pow(abs(normalizedDistance), abs(orthoFactor)));

        // Total force contribution from this center
        totalForce += radialForce + axialForce + orthoradialForce;
    }
	
    // Update velocity
    attributes.velocity += totalForce * unity_DeltaTime[2];
}


