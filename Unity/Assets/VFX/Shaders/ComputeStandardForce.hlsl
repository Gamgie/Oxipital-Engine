#include "VFXCommon.hlsl"
#include "ComputeAxialForce.hlsl"
#include "OxipitalHelpers.hlsl"

void StandardForce(inout VFXAttributes attributes, in StructuredBuffer<float> buffer, in VFXCurve forceInfluenceCurve, in float deltaTime, in float globalMultiplier)
{
    float3 totalForce = float3(0.0, 0.0, 0.0);
    int dancerCount = buffer[0];
    int dancerStartIndex = buffer[1];
    
    // Radial Force is attracting towards the center
    float radialIntensity = GetFloat(2);
    float radialFrequency = GetFloat(3);

    // Axial Parameters
    float axialIntensity = GetFloat(4);
    float axialFactor = GetFloat(5);
    float3 axisMultiplier = GetVector(6)
    float3 axialFrequency = GetVector(9);

    // Linear Force is attracting towards the same direction
    float linearForceIntensity = GetFloat(12);

    // Orthoradial Force is pushing particle on the orthogonal vector of the center vector
    float orthoIntensity = GetFloat(13);
    float innerRadius = GetFloat(14);
    int orthoFactor = GetFloat(15);
    float clockWise = GetFloat(16);
    
    // Spiral Force
    // float spiralForceIntensity = GetFloat(13);

    if (innerRadius == 0)
    {
        innerRadius = 0.01;
    }
	
    for (int i = 0; i < dancerCount; ++i)
    {
        float3 rotation = GetDVector(i, 3);
        
        float3 axis = ComputeForwardVectorFromRotation(rotation);
        
        float radius = GetDFloat(i, 7);

        if (radius <= 0)
            continue;
        
        float3 centerPosition = GetDVector(i, 0);
        float3 toCenterVector = centerPosition - attributes.position;
        float distanceToCenter = length(toCenterVector);
		
        float3 normalizedToCenterVector = toCenterVector / distanceToCenter;
        float normalizedDistance = distanceToCenter / (radius * innerRadius);

        // compute force influence linked to radius limit
        float forceInfluence = computeForceInfluence(distanceToCenter, buffer, forceInfluenceCurve, i);

        float3 radialForce = radialIntensity * (1 / (distanceToCenter + 1)) * 2.5 * normalizedToCenterVector;
        
        
        float3 axialForce = 0;
        float3 orthogonalVector = 0;
        float3 orthoradialForce = 0;
        float3 linearForce = 0;
        
        if (length(axis) > 0)
        {
            axialForce = axialIntensity * ComputeAxialForce(attributes.position, rotation, normalizedDistance, centerPosition, axialFrequency, axialFactor, axisMultiplier);
            axialForce *= 2; //to normalize strength feeling compared to other forces
            
       	    // Orthoradial force (inversely proportional to the distance)
            if (clockWise != 0)
            {
                orthogonalVector = normalize(cross(normalizedToCenterVector, axis) * clockWise);
                orthoradialForce = (abs(clockWise) * orthoIntensity * orthogonalVector) / (pow(abs(normalizedDistance), abs(orthoFactor)));
            }
            
            linearForce = linearForceIntensity * normalize(axis);
        }
        
            
        
        // Total force contribution from this center
        if (radialIntensity > 0)
            totalForce += radialForce * forceInfluence;
        if (axialIntensity > 0)
            totalForce += axialForce * forceInfluence;
        if (orthoIntensity > 0)
            totalForce += orthoradialForce * forceInfluence;
        if (linearForceIntensity > 0)
            totalForce += linearForce * forceInfluence;
        
    }
	

    // Update velocity
    attributes.velocity += totalForce * globalMultiplier * deltaTime; // * deltaTime;
}


