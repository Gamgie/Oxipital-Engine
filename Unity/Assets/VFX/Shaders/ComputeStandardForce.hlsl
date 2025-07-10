#include "VFXCommon.hlsl"
#include "ComputeAxialForce.hlsl"
#include "OxipitalHelpers.hlsl"

void StandardForce(inout VFXAttributes attributes, in StructuredBuffer<float> buffer, in VFXCurve forceInfluenceCurve, in float deltaTime, in float globalMultiplier,in float totalTime)
{
    float3 totalForce = float3(0.0, 0.0, 0.0);
    int dancerCount = buffer[0];
    int dancerStartIndex = buffer[1];
    
    // Radial Force is attracting towards the center
    float radialIntensity = GetFloat(2);
    float radialFrequency = GetFloat(3);
    float radialInOut = GetFloat(4);
    float radialSpeedWave = GetFloat(5);
    float radialAmplitudeWave = GetFloat(6);

    // Axial Parameters
    float axialIntensity = GetFloat(7);
    float axialFactor = GetFloat(8);
    float3 axisMultiplier = GetVector(9)
    float3 axialFrequency = GetVector(12);
    float axialSpeedWave = GetVector(15);
    float axialAmplitudeWave = GetVector(16);

    // Linear Force is attracting towards the same direction
    float linearForceIntensity = GetFloat(17);

    // Orthoradial Force is pushing particle on the orthogonal vector of the center vector
    float orthoIntensity = GetFloat(18);
    float innerRadius = GetFloat(19);
    int orthoFactor = GetFloat(20);
    float clockWise = GetFloat(21);
    
    // Ortho Axial
    float orthoAxialIntensity = GetFloat(35);
    float orthoAxialinnerRadius = GetFloat(36);
    int orthoAxialFactor = GetFloat(37);
    float orthoAxialclockWise = GetFloat(38);

    // Spiral Force
    // float spiralForceIntensity = GetFloat(13);

    if (innerRadius == 0)
    {
        innerRadius = 0.01;
    }
	
    float totalInfluence = 0;
    
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
        totalInfluence += forceInfluence;

        float radialInOutRemap = remapFloat(radialInOut,0,1,1,-1);
        float radialSinWave = radialAmplitudeWave * sin(2.0f * 3.14159f * (radialFrequency*3) * distanceToCenter + totalTime * radialSpeedWave * 5);
        if(radialAmplitudeWave == 0 || radialFrequency == 0) 
        {
            radialSinWave = 1;
        }
        float3 radialForce = radialInOutRemap * radialIntensity * (1 / (distanceToCenter + 1)) * 0.8f * normalizedToCenterVector * radialSinWave;
        
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
    attributes.velocity += totalForce * globalMultiplier * deltaTime;
    attributes.forceInfluence = totalInfluence;
}


