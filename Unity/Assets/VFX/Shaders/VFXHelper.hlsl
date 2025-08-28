/*
Vector AB is axis
P is current point.
I the point P projected on AB
A is center
B is center + axis

PI = PA + AI
AI = dot(AP,AB)/dot(AB,AB)xAB

for more info : 
https://gamedev.stackexchange.com/questions/72528/how-can-i-project-a-3d-point-onto-a-3d-line

Dot(AB,AB) is the lenght of the vector. Because AB is normalized, we can simplify the formula with

PI = PA + dot(AP,AB)xAB 

Example in python
from numpy import *
def ClosestPointOnLine(a, b, p):
    ap = p-a
    ab = b-a
    result = a + dot(ap,ab)/dot(ab,ab) * ab
    return result
*/
#ifndef VFX_HELPER_H
#define VFX_HELPER_H

float3 ClosestPointOnALine(float3 pos, float3 axis, float3 origin)
{
    float3 result;

    float3 PA = origin- pos;
    float3 AP = pos - origin;
    
    result = PA + dot(AP, axis) * axis;

    return result;
}

float remapFloat(in float value, in float min1, in float max1, in float min2, in float max2)
{
    float outgoing = min2 + (max2 - min2) * ((value - min1) / (max1 - min1));

    return outgoing;
}

float3 ComputeForwardVectorFromRotation(float3 rotationDegrees)
{
    float3 forwardVector;

    float3 rotationRadians = radians(rotationDegrees);
    float cosX = cos(rotationRadians.x);
    float sinX = sin(rotationRadians.x);
    float cosY = cos(rotationRadians.y);
    float sinY = sin(rotationRadians.y);
    float cosZ = cos(rotationRadians.z);
    float sinZ = sin(rotationRadians.z);

    forwardVector.x = sinY * cosX;
    forwardVector.y = -sinX;
    forwardVector.z = cosX * cosY;

    return normalize(forwardVector);
}

float3 ComputeRightVectorFromRotation(float3 rotationDegrees)
{
    float3 rightVector;

    float3 rotationRadians = radians(rotationDegrees);
    float cosX = cos(rotationRadians.x);
    float sinX = sin(rotationRadians.x);
    float cosY = cos(rotationRadians.y);
    float sinY = sin(rotationRadians.y);
    float cosZ = cos(rotationRadians.z);
    float sinZ = sin(rotationRadians.z);

    rightVector.x = cosY * cosZ + sinY * sinX * sinZ;
    rightVector.y = cosX * sinZ;
    rightVector.z = -sinY * cosZ + cosY * sinX * sinZ;

    return normalize(rightVector);
}

float3 ComputeUpVectorFromRotation(float3 rotationDegrees)
{
    float3 upVector;

    float3 rotationRadians = radians(rotationDegrees);
    float cosX = cos(rotationRadians.x);
    float sinX = sin(rotationRadians.x);
    float cosY = cos(rotationRadians.y);
    float sinY = sin(rotationRadians.y);
    float cosZ = cos(rotationRadians.z);
    float sinZ = sin(rotationRadians.z);

    upVector.x = -cosY * sinZ + sinY * sinX * cosZ;
    upVector.y = cosX * cosZ;
    upVector.z = sinY * sinZ + cosY * sinX * cosZ;

    return normalize(upVector);
}

float4 QuaternionFromEulerDegrees(float3 euler)
{
    float3 eulerRadians = radians(euler);

    // Extract the angles
    float yaw = eulerRadians.y;
    float pitch = eulerRadians.x;
    float roll = eulerRadians.z;

    // Compute half angles
    float cy = cos(yaw * 0.5);
    float sy = sin(yaw * 0.5);
    float cp = cos(pitch * 0.5);
    float sp = sin(pitch * 0.5);
    float cr = cos(roll * 0.5);
    float sr = sin(roll * 0.5);

    // Compute quaternion components
    float4 q;
    q.w = cy * cp * cr + sy * sp * sr;
    q.x = cy * cp * sr - sy * sp * cr;
    q.y = sy * cp * sr + cy * sp * cr;
    q.z = sy * cp * cr - cy * sp * sr;

    return q;
}

#endif // VFX_HELPER_H