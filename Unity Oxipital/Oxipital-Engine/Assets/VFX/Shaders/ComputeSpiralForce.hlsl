#ifndef COMPUTE_SPIRAL_FORCE_H
#define COMPUTE_SPIRAL_FORCE_H

#include "VFXHelper.hlsl"

float3 ComputeSpiralForce(in float3 pos, in float3 worldPos, in float3 rotation, in float3 center, in float3 axis, in float spiralFrequency, in float spiralVerticalForce, in float spiralDirection)
{

	float3 spiralForce = float3(0.0,0.0,0.0);	
    
    spiralVerticalForce *= 3;
    
    //spiralForce = sin(2 * 3.1415 * fff * atan((pos.x / pos.z) / sqrt(pos.x * pos.x + pos.z * pos.z))) * axis;
    //spiralForce = sin(2 * 3.1415 * spiralFrequency * pos.z * 2 * atan(pos.x / pos.y) + sqrt(pos.x * pos.x + pos.y * pos.y)) * axis;
    spiralForce = sin(2 * 3.1415 * spiralFrequency * pos.z * 2 * atan((pos.x / pos.y) + sqrt(pos.x * pos.x + pos.y * pos.y))) * axis;
    float distanceToAxis = length(ClosestPointOnALine(worldPos, axis, center));
    float3 direction = 1;
    if(pos.z < 0 && spiralDirection > 0)
    {
        direction = -1 * spiralDirection;
    }

    spiralForce += direction * (spiralVerticalForce / pow(distanceToAxis, 0.6f)) * normalize(axis);
   
   
    //float fff = 3;
    //float sprottX = (normalizedRadius ) * cos(2 * 3.1415 * fff * (pos.z));
    //float sprottY = (normalizedRadius) * sin(2 * 3.1415 * fff * (pos.z));
    //float sprottZ = pos.z;
    //spiralForce = (float3(sprottX, sprottY, sprottZ)-pos);    
        
        
    //float3 Moeb = float3(0.0, 0.0, 0.0);
    //float u = pos.x;
    //float v = pos.y;
    //float MoebX = 2 * normalizedRadius * (cos(u) + v * cos( u / 2) * cos(u));
    //float MoebY = 2 * normalizedRadius * (sin(u) + v * cos( u / 2) * sin(u));
    //float MoebZ = 2 * normalizedRadius * (v - sin( u / 2));
    //spiralForce = (float3(MoebX, MoebY, MoebZ) - pos);
    
    //float A = 3;
    //float3 spiralForceTmp = GetClosestPointOnASpiral(pos, A, seed);
    //spiralForceTmp += GetClosestPointOnASpiral(pos, -A, seed);
    
    //if (length(spiralForceTmp) > 0.000001f)
    //{
    //    spiralForce += spiralForceTmp - 2*pos;
    //}
    //else
    //{
    //    spiralForce += float3(0,0,0) - pos;
    //}
    
    //float A = 3;
    //float3 spiralForceTmp = GetClosestPointOnASpiral(pos, A, seed);
    //spiralForceTmp += GetClosestPointOnASpiral(pos, -A, seed);
    
    //if (length(spiralForceTmp) > 0.000001f)
    //{
    //    spiralForce += spiralForceTmp - 2*pos;
    //}
    //else
    //{
    //    spiralForce += float3(0,0,0) - pos;
    //}
    
	
    return spiralForce;
}
#endif // COMPUTE_SPIRAL_FORCE_H