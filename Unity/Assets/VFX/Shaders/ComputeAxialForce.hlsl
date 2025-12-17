#ifndef COMPUTE_AXIAL_FORCE_H
#define COMPUTE_AXIAL_FORCE_H

#include "VFXHelper.hlsl"


float3 ComputeAxialForce(in float3 pos, in float3 worldPosition, in float3 rotation, in float3 center, in float3 axialFrequency, in float axialFactor, in float3 axisMultiplier)
{
    // Axial force is attracting towards axis
    // F = Intensity / pow(( R + 1 ),axialFactor)

	float3 axialForceX = float3(0.0,0.0,0.0);
	float3 axialForceY = float3(0.0,0.0,0.0);
	float3 axialForceZ = float3(0.0,0.0,0.0);
	
    
    // Compute X Axis
    if (axisMultiplier.x != 0)
    {	
        float3 xAxis = ComputeRightVectorFromRotation(rotation);
        float3 axialHorizontalVector = ClosestPointOnALine(worldPosition, xAxis * axisMultiplier.x, center);
        float axialXDistance = length(axialHorizontalVector);
		float waveX = 0;

		if(axialFrequency.x != 0)
        {
			waveX = sin(2*PI*axialFrequency.x*pos.y) * cos(PI*axialFrequency.x*pos.x);
            //waveX = sin(2 * PI * axialFrequency.x * pos.x);
        }
		else
        {
			waveX = 1;
		}

        axialForceX = (axisMultiplier.x / pow(axialXDistance + 1, abs(axialFactor))) * waveX * normalize(axialHorizontalVector);
    }
		
	// Compute Y Axis
	if(axisMultiplier.y != 0)
    {
		float3 yAxis = ComputeUpVectorFromRotation(rotation);
        float3 axialVerticalVector = ClosestPointOnALine(worldPosition, yAxis * axisMultiplier.y, center);
		float waveY = 0;
        float axialYDistance = length(axialVerticalVector);

		if(axialFrequency.y != 0)
        {
            waveY = sin(2 * PI * axialFrequency.y * pos.x) * cos(PI * axialFrequency.y * pos.y);
            //waveY = sin(2 * PI * axialFrequency.y * pos.y);
        }
		else
        {
			waveY = 1;
		}

        axialForceY = (axisMultiplier.y / pow(axialYDistance + 1, abs(axialFactor))) * waveY * normalize(axialVerticalVector);
    }
	// Compute Z Axis
    if (axisMultiplier.z != 0)
    {
		float3 zAxis = ComputeForwardVectorFromRotation(rotation);
        float3 axialDepthVector = ClosestPointOnALine(worldPosition, zAxis * axisMultiplier.z, center);
        float axialZDistance = length(axialDepthVector);
		float waveZ = 0;

		if(axialFrequency.z != 0)
        {
            //waveZ = sin(2 * PI * axialFrequency.z * pos.z);
            waveZ = sin(2 * PI * axialFrequency.z * pos.z) * cos(PI * axialFrequency.z * pos.y);
        }
		else
        {
			waveZ = 1;
		}
        axialForceZ = (axisMultiplier.z / pow(axialZDistance + 1, abs(axialFactor))) * waveZ * normalize(axialDepthVector);
    }		

    // Total force contribution from this center

    return axialForceX + axialForceY + axialForceZ;
}



#endif // COMPUTE_AXIAL_FORCE_H