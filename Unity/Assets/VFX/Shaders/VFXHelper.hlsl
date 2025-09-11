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

float3 ComputeLocalPositionfromRotation(float3 centerPosition, float3 rotation, float3 scale, float3 particlePosition)
{
    float4x4 trsMatrix = GetTRSMatrix(centerPosition, rotation, scale);
    float4x4 invTRS = VFXInverseTRSMatrix(trsMatrix);
    float4 worldPos = float4(particlePosition, 1.0);
    float4 localPos = mul(invTRS, worldPos);
    return localPos.xyz;
}

float3 GetScaffoldPosition(float theta, int wing)
{
    float A = wing;
    float B = 0.5;
    float N = 4;
    
    float r = A / (log(B * tan(theta / (2 * N))));
    float GX = r * cos(theta);
    float GY = r * sin(theta);
    float GZ = 0;
    return float3(GX, GY, GZ);
}

float HashSpace(uint n)
{
    n = (n << 13u) ^ n;
    return 1.0 - float((n * (n * n * 15731u + 789221u) + 1376312589u) & 0x7fffffffu) / 2147483648.0;
}

float3 GetClosestPointOnASpiral(float3 position, int wing, uint seed)
{
    float thetaMin = 0.0f;
    float thetaMax = 2*UNITY_PI;
    float minDist = 10000;
    float3 result = 0;
    int step = 100;
    
    for (int i = 10; i < step ; i++)
    {
        float interpolate = (float) i / (float) step;
        float t = lerp(thetaMin, thetaMax, pow(interpolate,1));
        float3 curveAtT = GetScaffoldPosition(t, wing);
        
        float distance = length(curveAtT - position);
        
        if(distance < minDist)
        {
            minDist = distance;
            
            float interpolateF = (float) (max(i-step*0.05, 0)) / (float) step;
            float tFurther = lerp(thetaMin, thetaMax, pow(interpolateF,1));
            result = GetScaffoldPosition(tFurther, wing);
        }
    }
    
    float randScaleOffset = 0.5;
    float randPositionOffset = 0.5;
    float3 randPosition = float3((HashSpace(seed) * 2.0f - 1.0f) * randPositionOffset, (HashSpace(seed + 1) * 2.0f - 1.0f) * randPositionOffset, 0);
    float randScale = 1 + (HashSpace(seed) * 2.0f - 1.0f) * randScaleOffset;
    
    return (randPosition + result) * randScale;
}




#endif // VFX_HELPER_H