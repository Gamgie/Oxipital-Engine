#include "VFXHelper.hlsl"
#include "ComputeAxialForce.hlsl"
#include "ComputeSpiralForce.hlsl"
#include "ComputeOrthoAxialForce.hlsl"
#include "OxipitalHelpers.hlsl"


void StandardForce(inout VFXAttributes attributes, in StructuredBuffer<float> buffer, in VFXCurve forceInfluenceCurve, in float deltaTime, in float globalMultiplier,in float totalTime, in int randomPerParticle)
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
    float3 orthoAxisMultiplier = GetVector(9)
    float orthoAxialclockWise = GetFloat(38);
    float orthoAxialFrequency = GetVector(12);
    float orthoAxialSpeedWave = GetVector(15);
    float orthoAxialAmplitudeWave = GetVector(16);

    // Spiral Force
     float spiralForceIntensity = GetFloat(40);

    if (innerRadius == 0)
    {
        innerRadius = 0.01;
    }
	
    float totalInfluence = 0;
    
    for (int i = 0; i < dancerCount; ++i)
    {
        float3 rotation = GetDVector(i, 3);
        
        float radius = GetDFloat(i, 7);

        if (radius <= 0)
            continue;
        
        // Compute center vector. Center is the origin of the force. It is linked to the dancer ID.
        float3 centerPosition = GetDVector(i, 0);
        float3 toCenterVector = centerPosition - attributes.position;
        float distanceToCenter = length(toCenterVector);
        float3 normalizedToCenterVector = toCenterVector / distanceToCenter;
        float normalizedDistance = distanceToCenter / (radius * innerRadius);

        // compute force influence linked to radius limit
        float forceInfluence = computeForceInfluence(distanceToCenter, buffer, forceInfluenceCurve, i);
        totalInfluence += forceInfluence;
        
        // Forward vector (Z in unity world) is the default axis of a force
        float3 axis = ComputeForwardVectorFromRotation(rotation);
        
        // Compute local position of a particle
        float3 localPosition = ComputeLocalPositionfromRotation(centerPosition, rotation, float3(1, 1, 1), attributes.position);

        // Radial force - attracting or repeling particles towards center modualted by sin wave moving towards center.
        float radialInOutRemap = remapFloat(radialInOut,0,1,1,-1);
        float radialSinWave = radialAmplitudeWave * 3 * sin(2.0f * 3.14159f * (radialFrequency*6) * distanceToCenter + totalTime * radialSpeedWave * 5);
        if(radialAmplitudeWave == 0 || radialFrequency == 0) 
        {
            radialSinWave = 1;
        }
        float3 radialForce = radialInOutRemap * radialIntensity * (1 / (distanceToCenter + 1)) * 1.5f * normalizedToCenterVector * radialSinWave;
        
        float3 axialForce = 0;
        float3 orthoAxialForce = 0;
        float3 orthogonalVector = 0;
        float3 orthoradialForce = 0;
        float3 linearForce = 0;
        float3 spiralForce = 0;
        
        if (length(axis) > 0)
        {
            axialForce = axialIntensity * ComputeAxialForce(localPosition, attributes.position, rotation, normalizedDistance, centerPosition, axialFrequency, axialFactor, axisMultiplier);
            axialForce *= 1; //to normalize strength feeling compared to other forces
            
       	    // Orthoradial force (inversely proportional to the distance)
            if (clockWise != 0)
            {
                orthogonalVector = normalize(cross(normalizedToCenterVector, axis) * clockWise);
                orthoradialForce = 0.8f * (abs(clockWise) * orthoIntensity * orthogonalVector) / (pow(abs(normalizedDistance), abs(orthoFactor)) / 2);
            }
            
            linearForce = linearForceIntensity * normalize(axis);
            
            // Ortho Axial force (inversely proportional to the distance)
            if (orthoAxialclockWise != 0)
            {
                orthoAxialForce = orthoAxialIntensity * abs(orthoAxialclockWise) * ComputeOrthoAxialForce(attributes.position, rotation, centerPosition, orthoAxialFactor, radius * orthoAxialinnerRadius, orthoAxialclockWise);
                orthoAxialForce *= 15.0f;
            }
            
            //float r = RAND;
            spiralForceIntensity = 0.5f;
            spiralForce = spiralForceIntensity * ComputeSpiralForce(localPosition, rotation, centerPosition, orthoAxialFactor, radius, 1, axis, attributes.seed * (uint) randomPerParticle);
            //totalForce += spiralForce * forceInfluence;
            
        }
        
        // lorentz 
        //float sigma = 10 * 0.1;
        //float rho = 28 * 0.1;
        //float beta = 8.0 / 3.0 * 0.1;
        //float3 p = attributes.position;
        //float lorentzX = sigma * (p.y - p.x);
        //float lorentzY = (-p.x * p.z) + rho * p.x - p.y;
        //float lorentzZ = (p.x * p.y - beta * p.z);
        //totalForce += float3(lorentzX, lorentzY, lorentzZ) * 0.001 ;
        
        //float a = 2.07;
        //float b = 1.79;
        //float sprottX = 10 + attributes.position.y + a * attributes.position.x * attributes.position.y + attributes.position.x * attributes.position.z;
        //float sprottY = 1 - b * attributes.position.x * attributes.position.x + attributes.position.y * attributes.position.z;
        //float sprottZ = attributes.position.x - attributes.position.x * attributes.position.x - attributes.position.y * attributes.position.y;
        //totalForce += float3(sprottX, sprottY, sprottZ) * 0.01;

        
        // Total force contribution from this center
        if (radialIntensity > 0)
            totalForce += radialForce * forceInfluence;
        if (axialIntensity > 0)
            totalForce += axialForce * forceInfluence;
        if (orthoIntensity > 0)
            totalForce += orthoradialForce * forceInfluence;
        if (linearForceIntensity > 0)
            totalForce += linearForce * forceInfluence;
        if (orthoAxialIntensity > 0)
            totalForce += orthoAxialForce * forceInfluence;
    }
	   
    // Update velocity
    attributes.velocity += totalForce * globalMultiplier * deltaTime;
    attributes.forceInfluence = totalInfluence;
}


