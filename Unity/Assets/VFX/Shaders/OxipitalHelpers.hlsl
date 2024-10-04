#include "VFXCommon.hlsl"

#ifndef OXIPITAL_HELPERS
#define OXIPITAL_HELPERS

#define GetFloat(i) getBufferFloatProperty(i, buffer);
#define GetVector(i) getBufferVectorProperty(i, buffer);
#define GetDFloat(d,i) getDancerFloatProperty(d,i,buffer,dancerStartIndex);
#define GetDVector(d,i) getDancerVectorProperty(d, i, buffer, dancerStartIndex);
#define DANCER_DATA_SIZE 8

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
    float forceremap = remapFloat(forcerel, 0, 1, forceFactorInside*intensity, forceFactorOutside*intensity);

    return forceremap;
}


#endif 