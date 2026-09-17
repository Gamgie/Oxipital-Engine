# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this project is

Oxipital-Engine is a Unity project (Editor version `6000.3.15f1`, HDRP render pipeline) that drives a real-time
generative visual show ("Oxipital" / "Seleynora") for dome/immersive projection. It renders a particle-based
"ballet" of glowing orbs (VFX Graph), Gaussian-splat environments, and 3D models, driven live during a
performance via OSC (OSCQuery, for control from Chataigne) and output to external projection/media software via
Spout (Windows) or Syphon (macOS) texture sharing.

There is no separate build/test tooling — this is developed and run entirely inside the Unity Editor
(Window > Package Manager, Play mode, Build Settings). There are no unit tests in this repo.

## Key scenes

- `Assets/Scenes/Oxipital HDRP.unity` — main flat-screen/desktop scene.
- `Assets/Scenes/Oxpital_Dome.unity` — full-dome projection variant (uses `DomeCameraRigFollow` and the
  `com.pfc.dome-tools` package).
- `Assets/Scenes/Oxipital Seleynora.unity` — scene for the "Seleynora" show.

## Architecture

### Generic item/manager pattern (`Assets/Scripts/Tools/BaseItem.cs`, `BaseManager.cs`)

Most runtime-spawned, live-tunable objects follow a manager/item pair:

- `BaseItem` — a killable `MonoBehaviour` with a fade-out (`kill(time)`) driven by `killProgress` in `Update`.
- `BaseManager<T>` — spawns/despawns `T` (a `BaseItem`) children under itself to match a live `count` float
  each frame (`addItem` / `removeLastItem`), so the number of instances can be smoothly ramped from a show
  controller. Subclasses override `addItem`, `killLastItem`, `getKillTime` for custom spawn/despawn behavior.

This pattern is reused for orbs (`OrbManager : BaseManager<OrbGroup>`), physics force fields
(`StandardForceManager : BaseManager<StandardForceGroup>`), and dancers (`DancerGroup<T> : BaseManager<T>`).

### Ballet / particle system (`Assets/Scripts/Ballet/`, `OrbManager.cs`, `OrbGroup.cs`, namespace `Oxipital`)

- `Ballet.cs` is the top-level per-frame conductor: each frame it collects `GraphicsBuffer`s from every
  `StandardForceGroup` (named force fields, e.g. wind/attractors) and hands them to every `OrbGroup`'s VFX Graph
  via `OrbGroup.setForceBuffers`, and sums `aliveParticleCount` across all orb groups into `totalParticles`.
- `DancerGroup<T>` (generic, `T : Dancer`) manages a set of `Dancer` transforms ("dancers") arranged by one or
  more `DancePattern`s (Circle/Line/NBodyProblem/Manual — `Assets/Scripts/Ballet/Patterns/`), and packs their
  per-dancer state (position, rotation, intensity, size) plus group-level fields into a single `GraphicsBuffer`
  that's fed straight into a VFX Graph as a Structured Buffer.
  - Group-level float fields destined for that buffer are marked with `[InBuffer(index)]`
    (`Assets/Scripts/Ballet/InBufferAttribute.cs`); `DancerGroup` uses reflection over these attributes to
    pack the buffer layout (`initBufferAndFieldInfoList` / `getList`). When adding a new buffer-exposed field on
    an `OrbGroup`/`DancerGroup` subclass, give it the next free `InBuffer` index and keep `DANCER_DATA_SIZE`
    (per-dancer floats) and the VFX Graph's buffer-reading nodes in sync.
  - `DancePattern.updatePattern<T>` blends a target position onto each dancer's local position using
    `blendMode` (Add/Multiply/Replace) and a smoothed `weight`; concrete patterns only need to implement
    `getPatternPositions`.
  - `OrbGroup : DancerGroup<Dancer>` is the concrete emitter: it owns a `VisualEffect` (VFX Graph) and pushes
    emitter shape/appearance/physics parameters (also `[InBuffer]`-tagged) into it every frame, plus loads
    emitter meshes/textures (via `MeshLoader`, using glTFast to load `.glb` files from
    `StreamingAssets/emitters`) and optional Spout-fed textures.

### Live OSC control (OSCQuery / Chataigne)

Public fields on `MonoBehaviour`s are auto-exposed over OSCQuery (via `com.benkuper.oscquery`) for control from
an external show-control tool (Chataigne). Mark any field that should stay internal/not be exposed with
`[OSCQuery.DoNotExpose]` (see `Dancer.cs`, `OrbGroup.meshName`). OSCQuery only reflects flat public fields per
component — it can't see values living in a list on a parent manager — which is why e.g.
`GaussianManager` looks components up by name across children (`SetVisibility(splatName, value)`) rather than
exposing a collection.

### Camera system (`Assets/Scripts/Camera/`)

- `CameraController` is the top-level camera state machine: it owns a Cinemachine setup (`CinemachineBrain` +
  one `CinemachineCamera` per mode) and switches between `CameraMovementType.Orbital / Spaceship / Hands`
  (`OrbitalMovement`, `SpaceshipMovement`, `HandsMovement`, each a `CameraMovement` subclass). On switch it
  hands the outgoing camera's live position/rotation to the incoming one (`SetCameraTransform`) so the
  Cinemachine blend starts smoothly instead of snapping. It also drives shared FOV/ortho/noise/transition-time
  parameters into whichever camera is active, and toggles `isFullDome` for dome-rig output.
- `CameraMovement` (abstract) is the interface each movement mode implements: `Init`, `UpdateMovement`,
  `SetActive`, `Reset`, `UpdateFOV`/`UpdateOrthoSize`, `UpdateZOffset`, `UpdateNoiseParameter` (Cinemachine Perlin
  noise), `SetCameraTransform`.
- `DomeCameraRigFollow` re-targets a dome rig transform to follow a camera target, compensating for the
  `com.pfc.dome-tools` rig being offset from the projection center.
- `CameraSpoutManager` (`[ExecuteInEditMode]`) owns the output `RenderTexture` that the main camera renders
  into, and streams it out via `Klak.Spout.SpoutSender` on Windows or `Klak.Syphon.SyphonServer` on macOS
  (platform is chosen at runtime via `Application.platform`); output resolution is persisted in `PlayerPrefs`.

### Gaussian splats (`Assets/Scripts/Gaussian/`)

Splat scenes use `org.nesnausk.gaussian-splatting` (aras-p's `UnityGaussianSplatting`). Each splat has its own
`GaussianSplatVisibility` component (same GameObject as its `GaussianSplatRenderer`) so it can be addressed
directly over OSCQuery; `GaussianManager` is just a by-name lookup/convenience wrapper
(`FadeIn`/`FadeOut`/`SetVisibility`) over the child splats — for the same "OSCQuery can't see a list" reason
described above.

### Editor tooling (`Assets/Editor/`)

`RecordScene.cs` uses Unity's `GameObjectRecorder` to bake a live performance (camera, ballet transforms, orb
groups, force groups, dance patterns) into a legacy `AnimationClip` for later replay, driven by an in-Editor
`record` toggle and `RecordState` (record/replay/none).

## Third-party packages of note (`Packages/manifest.json`)

- `com.unity.render-pipelines.high-definition` — HDRP.
- `com.benkuper.oscquery` (scoped registry `package.openupm.com`) — OSCQuery server, live parameter exposure to
  Chataigne.
- `com.pfc.dome-tools` (OpenUPM) — dome projection/warping rig.
- `jp.keijiro.klak.spout` / `jp.keijiro.klak.syphon` / `jp.keijiro.klak.ndi` — video texture sharing out of Unity.
- `org.nesnausk.gaussian-splatting` (git dependency, aras-p/UnityGaussianSplatting) — Gaussian splat rendering.
- `com.unity.cloud.gltfast` — runtime `.glb` mesh loading (`MeshLoader` in `OrbGroup.cs`).
- `com.unity.cinemachine` — camera rigs/blending.
- `com.unity.recorder` — used alongside the custom `RecordScene` editor tool.
