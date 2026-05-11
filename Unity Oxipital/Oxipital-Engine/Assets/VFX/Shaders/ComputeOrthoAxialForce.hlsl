#ifndef COMPUTE_ORTHO_AXIAL_FORCE_H
#define COMPUTE_ORTHO_AXIAL_FORCE_H

#include "VFXHelper.hlsl"

float3 ComputeOrthoAxialForce(in float3 pos, in float3 rotation, in float3 center, in float axialFactor, in float normalizedRadius, in float clockwise)
{
    // ortho Axial force is rotating around axis
    // F = Intensity / pow(( R + 1 ),axialFactor)

	float3 orthoAxialForce = float3(0.0,0.0,0.0);	
    float3 zAxis = ComputeForwardVectorFromRotation(rotation);
    float3 xAxis = ComputeRightVectorFromRotation(rotation);
    float3 axialDepthVector = ClosestPointOnALine(pos, zAxis, center);
    float3 orthoAxialDepthVector = cross(normalize(zAxis), normalize(axialDepthVector)) * clockwise * -1;
    float axialZDistance = length(axialDepthVector) / normalizedRadius;
    float axialXDistance = length(ClosestPointOnALine(pos, xAxis, center)) / normalizedRadius;
    float curve = (axialXDistance);//+ sin(2 * 3.14 * axialXDistance);

    orthoAxialForce = (1 / pow(abs(axialZDistance + 1), abs(axialFactor))) * normalize(orthoAxialDepthVector);
	
    // Total force contribution from this center
    return orthoAxialForce;
}
#endif // COMPUTE_ORTHO_AXIAL_FORCE_H