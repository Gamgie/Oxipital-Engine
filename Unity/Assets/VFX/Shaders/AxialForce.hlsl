#include "VFXCommon.hlsl"

float3 AxialForce(in float4x4 parameters, in float3 pos, in float3 axis, in StructuredBuffer<float3> forceCenterBuffer)
{
	float3 totalForce = float3(0.0, 0.0, 0.0);
	int centerCount = forceCenterBuffer.Length;
	float intensity = parameters[0][0];
	float radius = parameters[1][0];
	float frequenceX = parameters[2][0];
	float frequenceY = parameters[3][0];
	float frequenceZ = parameters[0][1];

	// Axial force is attracting all particles towards axis
	// F = Intensity / ( R + 1 )²
	//Axial force (proportional to invert distance squared and pointed toward axis)

	// Raise intensity level to adjust to other force.
	intensity *= 5;
	
	for (int i = 0; i < centerCount; ++i)
    {
		float3 center = forceCenterBuffer[i];
        float3 toCenter = center - pos;
        float distanceToCenter = length(toCenter);
		
		float3 normalizedToCenter = toCenter / distanceToCenter;
		float normalizedDistance = distanceToCenter / radius;
		float3 axialForceX = float3(0.0,0.0,0.0);
		float3 axialForceY = float3(0.0,0.0,0.0);
		float3 axialForceZ = float3(0.0,0.0,0.0);


		// Compute X Axis
		if(axis.x != 0)
        {
			float3 axialHorizontalVector = ClosestPointOnALine(pos, float3(axis.x, 0, 0), center);
			float waveX = 0;

			if(frequenceX != 0)
            {
				waveX = sin(2*PI*frequenceX*pos.y) * cos(PI*frequenceX*pos.x);
			}
			else
            {
				waveX = 1;
			}

			axialForceX = ( intensity * axis.x / pow(normalizedDistance+1,2) ) * waveX * normalize(axialHorizontalVector);
		}
		
		// Compute Y Axis
		if(axis.y != 0)
        {
			float3 axialVerticalVector = ClosestPointOnALine(pos, float3(0, axis.y, 0), center);
			float waveY = 0;

			if(frequenceY != 0)
            {
				waveY = sin(2*PI*frequenceY*pos.x) * cos(PI*frequenceY*pos.y);
			}
			else
            {
				waveY = 1;
			}

			axialForceY = ( intensity * axis.y / pow(normalizedDistance+1,2) ) * waveY * normalize(axialVerticalVector);
		}
		// Compute Z Axis
		if(axis.z != 0)
        {
			float3 axialDepthVector = ClosestPointOnALine(pos, float3(0, 0, axis.z), center);
			float waveZ = 0;

			if(frequenceZ != 0)
            {
				waveZ = sin(2*PI*frequenceZ*pos.z) * cos(PI*frequenceZ*pos.y);
			}
			else
            {
				waveZ = 1;
			}
			axialForceZ = ( intensity * axis.z / pow(normalizedDistance+1,2) ) * waveZ * normalize(axialDepthVector);
		}

		if(distanceToCenter > radius)
        {
			axialForceX = float3(0,0,0);
			axialForceY = float3(0,0,0);
			axialForceZ = float3(0,0,0);
		}
		

        // Total force contribution from this center
        totalForce += axialForceX + axialForceY + axialForceZ;
	}
	
	return totalForce;
}


