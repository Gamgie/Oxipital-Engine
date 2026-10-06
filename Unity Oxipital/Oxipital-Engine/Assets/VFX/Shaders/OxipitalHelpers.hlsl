#include "VFXHelper.hlsl"

#ifndef OXIPITAL_HELPERS
#define OXIPITAL_HELPERS

#define GetFloat(i) getBufferFloatProperty(i, buffer);
#define GetVector(i) getBufferVectorProperty(i, buffer);
#define GetDFloat(d,i) getDancerFloatProperty(d,i,buffer,dancerStartIndex);
#define GetDVector(d,i) getDancerVectorProperty(d, i, buffer, dancerStartIndex);
#define DANCER_DATA_SIZE 8

// A Custom HLSL block reading a buffer that is also read by a Sample Buffer node (same context) must declare it
// StructuredBuffer<Single> : VFX Graph compares the template name and Sample Buffer registers "Single", not "float".
#define Single float

// Orb Buffer group fields (OrbGroup [InBuffer] indices) used by the infinite life mode
#define ORB_INFINITE_LIFE 1
#define ORB_INFINITE_COUNT 2
#define ORB_INFINITE_RANK_BASE 35
#define INFINITE_LIFE_HOLD 0.1 // fraction of its lifetime where an infinite particle stops aging (just after the size fade-in)

float getBufferFloatProperty(in int index, in StructuredBuffer<float> buffer)
{
     return buffer[index+2];
}

float3 getBufferVectorProperty(in int index, in StructuredBuffer<float> buffer)
{
    return float3(buffer[index+2], buffer[index+3], buffer[index+4]);
}

float getDancerFloatProperty(in int dancerIndex, in int dancerProperty, in StructuredBuffer<float> buffer, in int startIndex)
{
    return buffer[startIndex + dancerIndex*DANCER_DATA_SIZE + dancerProperty];
}

float3 getDancerVectorProperty(in int dancerIndex, in int dancerProperty, in StructuredBuffer<float> buffer, in int startIndex)
{
    int index = startIndex + dancerIndex* DANCER_DATA_SIZE + dancerProperty;
    return float3(buffer[index], buffer[index+1], buffer[index+2]);
}

float computeForceInfluence(in float distancetocenter, in StructuredBuffer<float> buffer, in VFXCurve curve, in int index)
{
    int dancerStartIndex = buffer[1]; 
    float forceFactorInside = GetFloat(0);
    float forceFactorOutside = GetFloat(1);
    float intensity = GetDFloat(index,6);
    float forceRadius = GetDFloat(index,7);
		
    float forcerel = SampleCurve(curve, distancetocenter / forceRadius);
    float forceremap = remapFloat(forcerel, 0, 1, forceFactorOutside*intensity, forceFactorInside*intensity);

    return forceremap;
}

#endif 