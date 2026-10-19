# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this project is

Oxipital-Engine is a Unity project (Editor version `6000.3.15f1`, HDRP render pipeline) that drives a real-time
generative visual show ("Oxipital" / "Seleynora") for dome/immersive projection. It renders a particle-based
"ballet" of glowing orbs (VFX Graph), Gaussian-splat environments, and 3D models, driven live during a
performance via OSC (OSCQuery, for control from Chataigne) and output to external projection/media software via
Spout (Windows) or Syphon (macOS) texture sharing.

The engine itself is not in this project's `Assets`: it lives in two Unity packages, **Oxipital Core** and
**Oxipital Camera**, in the [Oxipital-Packages](https://github.com/Gamgie/Oxipital-Packages) repository, which is
a git submodule at the root of this repo (`Oxipital-Packages/`). `Packages/manifest.json` references them with
`file:../../../Oxipital-Packages/Core` and `.../Camera`, so they are editable from this project and show under
*Packages* in the Project window. **The engine architecture is documented in `Oxipital-Packages/CLAUDE.md`.**
This project is the engine's workbench and the home of the show-specific content.

There is no separate build/test tooling — this is developed and run entirely inside the Unity Editor
(Window > Package Manager, Play mode, Build Settings). There are no unit tests in this repo.

## Key scenes

- `Assets/Scenes/Oxipital HDRP.unity` — main flat-screen/desktop scene.
- `Assets/Scenes/Oxipital Dome.unity` — full-dome projection variant (uses `DomeCameraRigFollow` and the
  `com.pfc.dome-tools` package).
- `Assets/Scenes/Oxipital Chimere.unity` — scene for the "Chimère" show.

## What stays in this project

Show-specific content and project tools: scenes, HDRP settings and volume profiles (`Assets/Settings`), heavy
models (`Assets/Mesh`: Earth, Moon), `Assets/StreamingAssets/emitters` (all emitter shapes, including the custom
ones), `SolarSystemController` (Seleynora's sun/earth/moons), `ToolsManager` (Graphy / Logs Viewer toggles; reads
`CameraController.renderMainWindow`), the `RecordScene` editor tool, and the Asset Store plugins in
`Assets/Plugins` (DOTween, Graphy, Unity-Logs-Viewer, MeshToSDF). The packages must never depend on these.

## Working on the engine (submodule workflow)

- After cloning: `git clone --recursive`, or `git submodule update --init` (LFS is needed in both repos).
- The submodule is checked out on a fixed commit (detached HEAD). Before changing engine files, run
  `git switch main` inside `Oxipital-Packages/`, otherwise the commits belong to no branch.
- Engine changes are committed **inside `Oxipital-Packages/`** (they go to the Oxipital-Packages repo), then this
  repo commits the new submodule pointer. Push with `git push --recurse-submodules=on-demand` from this repo, so
  the submodule is pushed before the pointer that needs it.
- Follow the packaging rules of `Oxipital-Packages/CLAUDE.md` (committed `.meta` for every new file, no renaming
  of classes/public fields/GameObjects used by Chataigne, releases tagged `vX.Y.Z`).
- Show projects use the packages by git URL and tag; to work on the engine from inside a show project, they can
  temporarily point to this submodule's folder (Package Manager > Add package from disk). Never commit a show
  project while it points to a local folder.
