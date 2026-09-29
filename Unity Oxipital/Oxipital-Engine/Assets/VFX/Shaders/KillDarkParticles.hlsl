/*
In additive render mode a black particle is invisible but still costs simulation and fill rate.
Placed at the end of Initialize (after the color has been sampled from the emitter texture),
this kills at spawn every particle darker than the threshold, only when the orb renders in UnlitAdditive.

renderType comes from a Sample Graphics Buffer on "Orb Buffer" at index 20
(OrbGroup.renderType is [InBuffer(18)], +2 for the dancer count / start index header).
The threshold is set directly on the block port in the graph (not exposed).
*/

#define RENDER_TYPE_UNLIT_ADDITIVE 1 // OrbGroup.RenderType.UnlitAdditive

/// renderType: OrbGroup.renderType (Orb Buffer index 20)
/// threshold: Minimum luminance (linear) to keep a particle in additive mode
void KillDarkParticles(inout VFXAttributes attributes, in float renderType, in float threshold)
{
    if (round(renderType) != RENDER_TYPE_UNLIT_ADDITIVE) return;

    float luminance = dot(attributes.color, float3(0.2126, 0.7152, 0.0722));
    if (luminance < threshold) attributes.alive = false;
}
