/*
Infinite life mode (OrbGroup.infiniteLife), Update part. See InfiniteLifeInit.hlsl for how ranks are given.

For particles spawned in infinite life mode (infiniteRank > 0) :
- rank above the expected count (OrbGroup.infiniteCount) -> killed, so lowering the intensity removes particles.
- otherwise the age is held just after the fade-in (INFINITE_LIFE_HOLD) so they never die and over-life curves
  (size, Color Over Life) stay visible.
- when infinite life is switched off (or the orb is killed), the rank is cleared : the particle ages again and dies normally.

Placed at the end of Update, buffer is the "Orb Buffer".

Known limitations :
- in UnlitAdditive render, dark particles are still killed at spawn (KillDarkParticles), so a few less than infiniteCount may be alive.
- if the effect is not simulated during a frame where the count changes (culled, paused), counts can drift.
  Switching infiniteLife off and on resyncs them.
*/

#include "OxipitalHelpers.hlsl"

void UpdateInfiniteLife(inout VFXAttributes attributes, in StructuredBuffer<Single> buffer)
{
    if (attributes.infiniteRank == 0) return;

    if (getBufferFloatProperty(ORB_INFINITE_LIFE, buffer) < 0.5)
    {
        attributes.infiniteRank = 0;
        return;
    }

    if (attributes.infiniteRank > (uint)getBufferFloatProperty(ORB_INFINITE_COUNT, buffer))
    {
        attributes.alive = false;
        return;
    }

    attributes.age = min(attributes.age, attributes.lifetime * INFINITE_LIFE_HOLD);
}
