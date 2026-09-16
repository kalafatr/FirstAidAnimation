# First Aid Animation — VR Hand-Guided Animation Scrubber

A small Unity prototype exploring one interaction idea for VR training tools: instead of watching
a first-aid animation play back on a timer, a trainee's hand position along a guide path directly
scrubs through it. Move your hand further along the path and the character's pose advances to match.

## What this actually is (and isn't)

This is a **focused technical prototype**, not a full application:

- One scene, one custom script, one interaction mechanic.
- No menus, no UI, no first-aid course content, no networking.
- The committed scene does not have a VR headset/controller rig wired in — see
  [Running it](#running-it) below for how it's tested without one.

## How the scrubbing works

[`CreateAnimationLine.cs`](First%20Aid/Assets/CreateAnimationLine.cs) defines the guide path with
three anchor `Transform`s (`firstPointTransform`, `middlePointTransform`, `lastPointTransform`),
forming a two-segment arc. On enable, it walks along both segments and instantiates
[`PathMarker`](First%20Aid/Assets/PathMarker.prefab) instances (via
[`AnimationPath.prefab`](First%20Aid/Assets/AnimationPath.prefab)) to visualize the arc with a
custom glow shader ([`GlowPath.shadergraph`](First%20Aid/Assets/GlowPath.shadergraph) /
[`GlowPath.mat`](First%20Aid/Assets/GlowPath.mat), using
[`PathArrowIcon.png`](First%20Aid/Assets/PathArrowIcon.png) as the arrow texture).

Every frame, the script measures the tracked hand's distance to the three anchors. Once it's
within `proximity` of the path, that position is converted into a normalized `AnimationValue`
(0 → 1) and written to the `Animator` as a float parameter, blending between the two arm poses
([`Armature_pose1.anim`](First%20Aid/Assets/Armature_pose1.anim) and
[`Armature_pose2.anim`](First%20Aid/Assets/Armature_pose2.anim)) driven by
[`ArcScrubAnimator.controller`](First%20Aid/Assets/ArcScrubAnimator.controller) — so the pose
tracks hand position instead of playing back linearly over time.

Hand tracking itself goes through Unity's Input System: `leftGrip`/`rightGrip`
(`InputActionProperty`) read a VR controller's grip button, and the script only treats a hand as
"active" once its grip is pressed and it's near the start of the path. When no VR hand is close
enough (or none is assigned), it falls back to a plain stand-in `Transform` so the mechanic can
still be exercised in the Editor without a headset connected.

## Tech stack

- Unity **2021.3.23f1** (LTS)
- Universal Render Pipeline (URP) 12.1.11
- Unity Input System 1.5.1 (VR controller grip bindings)
- Shader Graph for the animated glow-path effect

## Project structure

```
First Aid/
├─ Assets/
│  ├─ CreateAnimationLine.cs        # the scrubbing mechanic
│  ├─ Animation Line.prefab         # root object: script + 3 path anchors
│  ├─ AnimationPath.prefab          # marker set spawned along the arc
│  ├─ PathMarker.prefab             # single glowing arrow marker
│  ├─ ArcScrubAnimator.controller   # blends between the two arm poses
│  ├─ Armature_pose1.anim / Armature_pose2.anim
│  ├─ GlowPath.mat / GlowPath.shadergraph / PathArrowIcon.png
│  ├─ ReferenceProp.fbx             # reference model placed in the sample scene
│  ├─ Scenes/SampleScene.unity
│  └─ Settings/                     # URP quality presets (Balanced/HighFidelity/Performant)
├─ Packages/
└─ ProjectSettings/
```

## Running it

1. Install Unity **2021.3.23f1** (or a compatible 2021.3 LTS patch) via Unity Hub.
2. Unity Hub → **Add** → select the `First Aid` folder → open with that editor version.
3. Open `Assets/Scenes/SampleScene.unity` and enter Play mode.
4. Without a connected VR headset/controllers, the script falls back to a stand-in `Cube` object
   as the tracked hand, so the scrubbing behavior can still be tested from the Editor. Move the
   Cube in the Scene view near the guide path (visualized with red gizmo spheres/lines) to see
   `AnimationValue` update.

> **Note:** This cleanup pass (renaming prototype-named assets, removing unused Unity template
> boilerplate, and fixing a few reliability issues in the script) was done without a local Unity
> installation available to open and re-verify the project in the Editor. All `.meta` GUIDs were
> preserved and asset renames were cross-checked against scene/prefab references at the YAML
> level, but the project itself has not been re-opened and play-tested since these changes.

## Known limitations

- The guide path is a fixed 3-point arc, not an arbitrary spline.
- `leftHand`/`rightHand` are not wired to an actual XR rig in the committed scene.
- No automated tests.
