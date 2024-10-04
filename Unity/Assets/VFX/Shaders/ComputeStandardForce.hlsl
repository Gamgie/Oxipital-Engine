#include "VFXCommon.hlsl"
#include "ComputeAxialForce.hlsl"
#include "OxipitalHelpers.hlsl"

int dancerStartIndex;

float ComputeForceInfluence(in float distanceToCenter, in StructuredBuffer<float> buffer, in VFXCurve curve, in int i)
{
    float forceFactorInside = GetFloat(0);
    float forceFactorOutside = GetFloat(1);
    float intensity = GetDFloat(i,6);
    float forceRadius = GetDFloat(i,7);
		
    float forceRel = SampleCurve(curve, distanceToCenter / forceRadius);
    float forceRemap = remapFloat(forceRel, 0, 1, forceFactorInside*intensity, forceFactorOutside*intensity);

    return forceRemap;
}

void StandardForce(inout VFXAttributes attributes, in StructuredBuffer<float> buffer, in VFXCurve forceInfluenceCurve)
{
    float3 totalForce = float3(0.0, 0.0, 0.0);
    int dancerCount = buffer[0];
    dancerStartIndex = buffer[1];
    
    float3 axis = float3(0.0,0.0,0.0);
    
    // Radial Force is attracting towards the center
    float radialIntensity = buffer[2];

    // Axial Parameters
    float axialIntensity = buffer[4];
    float axialFactor = buffer[5];
    float3 axialFrequency = float3(0.0,0.0,0.0);

    // Orthoradial Force is pushing particle on the orthogonal vector of the center vector
    float orthoIntensity = buffer[7];
    int orthoFactor = buffer[9];
    float innerRadius = buffer[8];
    bool clockWise = buffer[10];
    
    // Linear Force is attracting towards the same direction
    float linearForceIntensity = buffer[6];

    // Spiral Force
    float spiralForceIntensity = buffer[13];

    if (innerRadius == 0)
    {
        innerRadius = 0.01;
    }
	
    for (int i = 0; i < dancerCount; ++i)
    {
        float radius = GetDFloat(i, 7);
        float3 centerPosition = GetDVector(i,0);
        float3 toCenterVector = centerPosition - attributes.position;
        float distanceToCenter = length(toCenterVector);
		
        float3 normalizedToCenterVector = toCenterVector / distanceToCenter;
        float normalizedDistance = distanceToCenter / (radius * innerRadius);
        float clockWiseFactor = clockWise == true ? 1 : -1;

        // compute force influence linked to radius limit
        float forceInfluence = ComputeForceInfluence(distanceToCenter, buffer, forceInfluenceCurve, i);

		float3 radialForce = radialIntensity * (1/(distanceToCenter+1)) * normalizedToCenterVector;
        
        float3 axialForce = axialIntensity * ComputeAxialForce(attributes.position, axis, normalizedDistance, centerPosition, axialFrequency, axialFactor);

       	// Orthoradial force (inversely proportional to the distance)
        float3 orthogonalVector = normalize(cross(normalizedToCenterVector, axis) * clockWiseFactor);
        float3 orthoradialForce = (orthoIntensity * orthogonalVector) / (pow(abs(normalizedDistance), abs(orthoFactor)));

        float3 linearForce = linearForceIntensity * normalize(axis);

        // Total force contribution from this center
        totalForce += forceInfluence * (radialForce + axialForce + orthoradialForce + linearForce);
    }
	
    // Update velocity
    attributes.velocity += totalForce * unity_DeltaTime[2];
}


