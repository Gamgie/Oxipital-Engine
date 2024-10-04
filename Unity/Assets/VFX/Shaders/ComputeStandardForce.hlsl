#include "VFXCommon.hlsl"
#include "ComputeAxialForce.hlsl"
#include "OxipitalHelpers.hlsl"

void StandardForce(inout VFXAttributes attributes, in StructuredBuffer<float> buffer, in VFXCurve forceInfluenceCurve)
{
    float3 totalForce = float3(0.0, 0.0, 0.0);
    int dancerCount = buffer[0];
    int dancerStartIndex = buffer[1];
    
    // Radial Force is attracting towards the center
    float radialIntensity = GetFloat(2);
    float radialFrequency = GetFloat(3);

    // Axial Parameters
    float axialIntensity =GetFloat(4);
    float axialFactor = GetFloat(5);
    float3 axialFrequency = GetVector(6);

    // Linear Force is attracting towards the same direction
    float linearForceIntensity = GetFloat(9);

    // Orthoradial Force is pushing particle on the orthogonal vector of the center vector
    float orthoIntensity = GetFloat(10);
    float innerRadius = GetFloat(11);
    int orthoFactor = GetFloat(12);
    bool clockWise = GetFloat(13);
    
    // Spiral Force
    // float spiralForceIntensity = GetFloat(13);

    if (innerRadius == 0)
    {
        innerRadius = 0.01;
    }
	
    for (int i = 0; i < dancerCount; ++i)
    {
        float axis = GetDFloat(i, 3);
        float radius = GetDFloat(i, 7);

        if(radius <= 0) continue;
        
        float3 centerPosition = GetDVector(i,0);
        float3 toCenterVector = centerPosition - attributes.position;
        float distanceToCenter = length(toCenterVector);
		
        float3 normalizedToCenterVector = toCenterVector / distanceToCenter;
        float normalizedDistance = distanceToCenter / (radius * innerRadius);
        float clockWiseFactor = clockWise == true ? 1 : -1;

        // compute force influence linked to radius limit
        float forceInfluence = computeForceInfluence(distanceToCenter, buffer, forceInfluenceCurve, i);

		float3 radialForce = radialIntensity * (1/(distanceToCenter+1)) * 2.5 * normalizedToCenterVector;
        
        float3 axialForce = axialIntensity * 0.008 * ComputeAxialForce(attributes.position, axis, normalizedDistance, centerPosition, axialFrequency, axialFactor);

       	// Orthoradial force (inversely proportional to the distance)
        float3 orthogonalVector = normalize(cross(normalizedToCenterVector, axis) * clockWiseFactor);
        float3 orthoradialForce = (orthoIntensity * orthogonalVector) / (pow(abs(normalizedDistance), abs(orthoFactor)));

        float3 linearForce = linearForceIntensity * normalize(axis);

        // Total force contribution from this center
        totalForce += forceInfluence * (radialForce + axialForce + orthoradialForce + linearForce);
    }
	
    float deltaTime = 1.0/60.0;//unity_DeltaTime[2];

    // Update velocity
    attributes.velocity += totalForce * deltaTime;
}


