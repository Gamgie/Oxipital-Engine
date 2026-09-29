/*
Optical flow force (Kinect -> TouchDesigner -> Spout -> OpticalFlowManager), fed by Ballet / OrbGroup.
Exposed properties to create in the graph and link to the block ports :
    Texture2D  "Optical Flow Map"             -> flowMap
    Matrix4x4  "Optical Flow World To Plane"  -> worldToPlane
    Vector3    "Optical Flow Plane Right"     -> planeRight
    Vector3    "Optical Flow Plane Up"        -> planeUp
    Vector4    "Optical Flow Params"          -> params
    Delta Time operator                       -> deltaTime
*/

/// Hidden
float4 SampleOpticalFlow(VFXSampler2D flowMap, float4x4 worldToPlane, float depthBand, float3 position, out float influence)
{
    float3 local = mul(worldToPlane, float4(position, 1)).xyz;
    float2 uv = local.xy + 0.5;
    bool inside = all(uv >= 0) && all(uv <= 1);

    influence = inside ? 1 - smoothstep(0, max(depthBand, 1e-4), abs(local.z)) : 0;
    return influence > 0 ? SampleTexture(flowMap, uv, 0) : 0;
}

/// flowMap: Optical Flow Map. RG = flow on the plane, B = magnitude, A = body mask
/// worldToPlane: Optical Flow World To Plane. The FlowPlane is a unit quad in its local XY plane
/// planeRight: Optical Flow Plane Right. World X axis of the plane
/// planeUp: Optical Flow Plane Up. World Y axis of the plane
/// params: Optical Flow Params. x = add strength, y = depth band, z = wind speed, w = wind rate (set by OpticalFlowManager)
/// deltaTime: Delta Time
void OpticalFlowForce(inout VFXAttributes attributes, in VFXSampler2D flowMap, in float4x4 worldToPlane, in float3 planeRight, in float3 planeUp, in float4 params, in float deltaTime)
{
    if (params.x <= 0 && params.w <= 0) return;

    float influence;
    float4 s = SampleOpticalFlow(flowMap, worldToPlane, params.y, attributes.position, influence);
    if (influence <= 0) return;

    float3 flow = planeRight * s.r + planeUp * s.g;

    // Add : the flow pushes the particles
    attributes.velocity += flow * params.x * influence * deltaTime;

    // Wind : where there is motion, particles are dragged towards the flow speed
    float windAmount = saturate(params.w * deltaTime * influence * saturate(s.b));
    attributes.velocity = lerp(attributes.velocity, flow * params.z, windAmount);
}

/// flowMap: Optical Flow Map
/// worldToPlane: Optical Flow World To Plane
/// depthBand: Optical Flow Params.y
/// position: Position to sample, usually the particle position
/// return: Local motion amount (0..1), to modulate size / color / intensity
float OpticalFlowMagnitude(in VFXSampler2D flowMap, in float4x4 worldToPlane, in float depthBand, in float3 position)
{
    float influence;
    float4 s = SampleOpticalFlow(flowMap, worldToPlane, depthBand, position, influence);
    return s.b * influence;
}
