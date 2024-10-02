float3 ComputeAxialForce(in float3 pos, in float3 axis, in float normalizedDistance, in float3 center, in float3 axialFrequency, in float axialFactor)
{
    // Axial force is attracting towards axis
    // F = Intensity / pow(( R + 1 ),axialFactor)

	float3 axialForceX = float3(0.0,0.0,0.0);
	float3 axialForceY = float3(0.0,0.0,0.0);
	float3 axialForceZ = float3(0.0,0.0,0.0);

    // Compute X Axis
	if(axis.x != 0)
    {
		float3 axialHorizontalVector = ClosestPointOnALine(pos, float3(axis.x, 0, 0), center);
		float waveX = 0;

		if(axialFrequency.x != 0)
        {
			waveX = sin(2*PI*axialFrequency.x*pos.y) * cos(PI*axialFrequency.x*pos.x);
		}
		else
        {
			waveX = 1;
		}

		axialForceX = ( axis.x / pow(abs(normalizedDistance+1),abs(axialFactor)) ) * waveX * normalize(axialHorizontalVector);
	}
		
	// Compute Y Axis
	if(axis.y != 0)
    {
		float3 axialVerticalVector = ClosestPointOnALine(pos, float3(0, axis.y, 0), center);
		float waveY = 0;

		if(axialFrequency.y != 0)
        {
			waveY = sin(2*PI*axialFrequency.y*pos.x) * cos(PI*axialFrequency.y*pos.y);
		}
		else
        {
			waveY = 1;
		}

		axialForceY = ( axis.y / pow(abs(normalizedDistance+1),abs(axialFactor)) ) * waveY * normalize(axialVerticalVector);
	}
	// Compute Z Axis
	if(axis.z != 0)
    {
		float3 axialDepthVector = ClosestPointOnALine(pos, float3(0, 0, axis.z), center);
		float waveZ = 0;

		if(axialFrequency.z != 0)
        {
			waveZ = sin(2*PI*axialFrequency.z*pos.z) * cos(PI*axialFrequency.z*pos.y);
		}
		else
        {
			waveZ = 1;
		}
		axialForceZ = ( axis.z / pow(abs(normalizedDistance+1),abs(axialFactor) ) ) * waveZ * normalize(axialDepthVector);
	}		

    // Total force contribution from this center

    return axialForceX + axialForceY + axialForceZ;
}