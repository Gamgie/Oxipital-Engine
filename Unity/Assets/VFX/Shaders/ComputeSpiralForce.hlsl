#ifndef COMPUTE_SPIRAL_FORCE_H
#define COMPUTE_SPIRAL_FORCE_H

#include "VFXHelper.hlsl"

float3 ComputeSpiralForce(in float3 pos, in float3 rotation, in float3 center, in float axialFactor, in float normalizedRadius, in float clockwise, in float3 axis, in uint seed)
{
    // ortho Axial force is rotating around axis
    // F = Intensity / pow(( R + 1 ),axialFactor)

	float3 spiralForce = float3(0.0,0.0,0.0);	

    float fff = 3;
    float sprottX = (normalizedRadius ) * cos(2 * 3.1415 * fff * (pos.z));
    float sprottY = (normalizedRadius) * sin(2 * 3.1415 * fff * (pos.z));
    float sprottZ = pos.z;
    //spiralForce = (float3(sprottX, sprottY, sprottZ)-pos);    
        
        
    float3 Moeb = float3(0.0, 0.0, 0.0);
    float u = pos.x;
    float v = pos.y;
    float MoebX = 2 * normalizedRadius * (cos(u) + v * cos( u / 2) * cos(u));
    float MoebY = 2 * normalizedRadius * (sin(u) + v * cos( u / 2) * sin(u));
    float MoebZ = 2 * normalizedRadius * (v - sin( u / 2));
    //spiralForce = (float3(MoebX, MoebY, MoebZ) - pos);
    
    //spiralForce = sin(2 * 3.1415 * fff * atan((pos.x / pos.z) / sqrt(pos.x * pos.x + pos.z * pos.z))) * axis;
    //spiralForce = sin(2 * 3.1415 * fff * atan(pos.x / pos.z) + sqrt(pos.x * pos.x + pos.z * pos.z)) * float3(0,0,1);
    
    
    float A = 3;
    float3 spiralForceTmp = GetClosestPointOnASpiral(pos, A, seed);
    spiralForceTmp += GetClosestPointOnASpiral(pos, -A, seed);
    
    if (length(spiralForceTmp) > 0.000001f)
    {
        spiralForce += spiralForceTmp - 2*pos;
    }
    else
    {
        spiralForce += float3(0,0,0) - pos;
    }
    
    
    //float3 spiralForce = spiralForceIntensity * sin(2 * 3.1415 * spiralFreq  *  atan(p.x / p.z) + alpha * sqrt(p.x * p.x + p.z * p.z)) * axis;

	
    return spiralForce;
}
#endif // COMPUTE_SPIRAL_FORCE_H