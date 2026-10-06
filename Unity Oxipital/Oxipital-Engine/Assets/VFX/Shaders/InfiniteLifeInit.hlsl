/*
Infinite life mode (OrbGroup.infiniteLife), Initialize part. See InfiniteLifeUpdate.hlsl for the Update part.

In infinite life mode the rate spawner is stopped (Emitter Intensity = 0) and OrbGroup sends a "SpawnInfinite" event
with spawnCount = missing particles. Each of these particles gets a unique rank (custom attribute infiniteRank) :
infiniteRankBase + 1 .. infiniteRankBase + spawnCount. Rank 0 means a normal (finite life) particle.

Placed at the end of Initialize, buffer is the "Orb Buffer".
*/

#include "OxipitalHelpers.hlsl"

void InitInfiniteLife(inout VFXAttributes attributes, in StructuredBuffer<Single> buffer)
{
    if (getBufferFloatProperty(ORB_INFINITE_LIFE, buffer) < 0.5) return;

    attributes.infiniteRank = (uint)getBufferFloatProperty(ORB_INFINITE_RANK_BASE, buffer) + attributes.spawnIndex + 1;
}
