# First Aid Animation — IMU-Tracked VR Animation Scrubber

A Unity/VR prototype where a trainee's real hand and head position — tracked by a custom
Wi-Fi IMU rig, not a mouse or a timeline — drives which pose a training animation shows. Move
your hand further along a guide path in physical space and the character's pose advances to
match it, step by step through a multi-stage sequence.

This repo is a cleaned-up, portfolio-safe export of an ongoing private project. It keeps every
line of **original** code and fixes real bugs found while reviewing it; it deliberately leaves
out unrelated experiments and anything that belongs to Unity, Meta, or other third parties (see
[What's not in this repo, and why](#whats-not-in-this-repo-and-why)).

## Architecture: sensor → animation, end to end

```
 Physical "Sonia" IMU sensors (headset + 2 hand units, ESP-style devices on the LAN)
        │  HTTP GET, polled every `interval` seconds ("x/y/z/..." string response)
        ▼
 GetData_M.cs / GetData_S1.cs / GetData_S2.cs   (Scripts/Device)
        │  parses the response, writes rotation/grip/position into shared fields
        ▼
 VRInputSender.cs   (Scripts/Device)
        │  holds the live headset + both hands' position/rotation/grip/joystick state;
        │  can also be driven from keyboard+mouse (`HareketWithKeyboard`) for testing
        │  without any hardware — see "Running it" below
        ▼
 SoniaDevice.cs   (Scripts/Device)
        │  a custom `UnityEngine.InputSystem.InputDevice` that republishes that state as
        │  real Input System controls (Headset/LeftHand/RightHand Position & Rotation,
        │  grips, joystick) — so the rest of the project reads it like any other XR device
        ▼
 An XR Origin's controllers (bind their Tracked Pose Driver to the SoniaDevice's
 controls — not wired into the sample scene in this repo, see below)
        │  moves an XR Interaction Toolkit hand interactor in world space
        ▼
 PositionHandle.cs   (Scripts/Animation, on a grabbable handle with an XRGrabInteractable)
        │  snaps the grabbed handle to the closest point on a straight guide line and
        │  reports how far along it (0 → 1) as `ratio`
        ▼
 CreateAnimationLine.cs   (Scripts/Animation)
        │  reads `positionHandle.ratio` and writes it to the `Animator` as a float
        │  parameter — the pose follows hand position, not a clock
        ▼
 ArcScrubAnimator.controller   →   blends Armature_pose1.anim / Armature_pose2.anim
        ▲
        │  once a segment finishes (`AnimationValue >= 1`), advances to the next one
 AnimationLineManager.cs   (Scripts/Animation)
        (sequences a list of CreateAnimationLine segments into one multi-step exercise)
```

A separate, independent system for evaluating a trainee's movement:

```
 BonePositionManager.cs   (Scripts/Pose)
   records every bone's local rotation on an object tagged "Bone", saves/loads named
   snapshots as JSON under StreamingAssets/Saves/, and compares a live pose against a
   saved one (per-bone angle threshold) — a building block for "did the trainee's arm end
   up close enough to the reference pose", independent of the path-scrubbing mechanic above.
```

## What's genuinely custom here

Every script under [`Scripts/`](First%20Aid/Assets/Scripts) was written for this project:

- **`Scripts/Device/`** — `SoniaDevice.cs`, `VRInputSender.cs`, `GetData_M.cs`, `GetData_S1.cs`,
  `GetData_S2.cs`: the sensor-to-Input-System bridge described above.
- **`Scripts/Animation/`** — `CreateAnimationLine.cs`, `PositionHandle.cs`,
  `AnimationLineManager.cs`: the path-scrubbing and multi-step sequencing logic.
- **`Scripts/Pose/`** — `BonePositionManager.cs`: pose recording/comparison.

One honest caveat on `SoniaDevice.cs`: its *shape* (an `InputDevice` subclass +
`IInputUpdateCallbackReceiver`, a state struct with `[InputControl]` attributes, a static
constructor that calls `InputSystem.RegisterLayout`, and Editor menu items to add/remove the
device) follows Unity's own documented pattern for building a custom Input System device — that
scaffolding is Unity's intended extension point, not something we invented. What's ours is the
specific control layout (headset + two hands' position/rotation/grip/joystick) and wiring it to
a live network data source instead of Unity's tutorial's dummy data.

## What's not in this repo, and why

The source project also contains things that are explicitly **not** carried over, because they
aren't ours to present as original work or aren't part of this project's story:

- A ragdoll-physics drag demo (`DragRagdoll.prefab`, `Robot Kyle-Ragdolled.prefab`) built on
  Unity's free "Robot Kyle" sample character and a renamed copy of the
  [DevLocker](https://github.com/NibbleByte/DevLocker) `DragRigidbodyBetter` utility script — a
  separate physics experiment, unrelated to the animation-scrubbing mechanic above.
- Unity's XR Interaction Toolkit / Input System / Oculus Hands **sample content**
  (`Assets/Samples/…`) — reference material imported via Package Manager, not project code.
- The default Unity URP template boilerplate (`Readme.asset`, `TutorialInfo/`).
- A couple of standalone debug scratch scripts with no bearing on the real mechanic
  (a per-frame `Debug.Log` of a raw grip value, a generic mouse-drag test script).

The XR Origin / rig itself (`Complete XR Origin Set Up.prefab`) is Unity's own XR Interaction
Toolkit **Starter Assets** prefab. Rather than vendor a copy of it into this repo, use it as an
actual dependency: install it via **Package Manager → XR Interaction Toolkit → Samples →
Starter Assets** (it also includes the **XR Device Simulator**, useful for testing the
grab-based path without a headset). The same applies to `com.unity.xr.openxr` / `com.unity.xr.management`,
declared in `Packages/manifest.json` and required to compile `SoniaDevice.cs`.

## Bugs found and fixed while preparing this export

Reviewing this code end-to-end (without a Unity install available to run it — see the note in
[Running it](#running-it)) turned up a few concrete issues, fixed here:

- **`GetData_S2.cs` wrote the right hand's live rotation into the *headset's* fields**
  (`vrInputSender.HS_rotationx/y/z`) instead of `RH_rotationx/y/z`. Combined with `GetData_M`
  also writing `HS_rotation*`, this meant the two sensors fought over the same fields and the
  right hand's rotation was never actually updated from live data. Fixed to write `RH_rotation*`.
- **Dead-on-arrival reconnect timeout** in all three `GetData_*` pollers: `lastDataReceivedTime`
  was reset to `Time.time` on *every* poll (success or failure) right before checking
  `Time.time - lastDataReceivedTime > timeoutDuration` — that difference is always ~0, so the
  "connection lost" branch could never trigger. Fixed to only update the timestamp when data is
  actually parsed successfully.
- **Orphaned `[InputControl]` attributes** in `SoniaDeviceState`: several fields (e.g.
  `hs_rotationx/y/z/w`) had been commented out but the `[InputControl]` attributes above them
  were left in place, so they silently attached themselves to the *next* real field instead
  (e.g. `lh_positionx` ended up carrying six stacked `[InputControl]` attributes). Removed the
  dead attributes.
- `SoniaDevice.cs` had `using UnityEditor;` outside any `#if UNITY_EDITOR` guard, which would
  fail to compile in a player build. Moved inside the guard.
- Duplicate-marker bug in `CreateAnimationLine.cs`: re-enabling the object spawned a fresh set
  of line markers without destroying the previous set. Fixed, plus added guards against
  divide-by-zero when path endpoints coincide, and null-checks before driving the `Animator`.
- `GetData_S2.cs`'s hardcoded LAN IP (`http://192.168.85.111/s1`) was replaced with
  `http://sonia_s2.local`, matching the mDNS hostname convention `GetData_M`/`GetData_S1` use —
  this one is an inferred fix, not a verified one; update it if the real device differs.

None of these were re-verified by actually running the project in Unity — see the note below.

## Tech stack

- Unity **2021.3.23f1** (LTS)
- Universal Render Pipeline (URP) 12.1.11
- Unity Input System 1.5.1 — including a hand-written custom `InputDevice`
- XR Interaction Toolkit 2.3.2, XR Plug-in Management 4.4.0, OpenXR Plugin 1.7.0
- Shader Graph for the animated glow-path effect

## Running it

1. Install Unity **2021.3.23f1** (or a compatible 2021.3 LTS patch) via Unity Hub.
2. Unity Hub → **Add** → select the `First Aid` folder → open with that editor version, letting
   Package Manager resolve the XRI/OpenXR/XR Management packages added to `manifest.json`.
3. Open `Assets/Scenes/SampleScene.unity`. It includes a **"VR Input (Keyboard Test)"** object
   with `VRInputSender.HareketWithKeyboard` enabled — in Play mode, hold **Q**/**E** + mouse to
   rotate a hand, **G** to grip, **Space** + mouse to look around. Open
   **Window → Analysis → Input Debugger** to watch the live `SoniaDevice` control values change —
   this exercises the full sensor-simulation → Input System device pipeline without any hardware
   or XR rig.
4. To exercise the grab-and-scrub path end to end, import the XR Interaction Toolkit's
   **Starter Assets** sample (see above) for an XR Origin + XR Device Simulator, and bind a
   controller's Tracked Pose Driver to the `SoniaDevice` controls.

> **Verification note:** this cleanup and restructuring (moving/renaming scripts, updating the
> "Animation Line" prefab and animator controller to the versions that use `PositionHandle`, and
> the bug fixes above) was done by reading and cross-referencing the project's YAML at the text
> level — GUIDs, `fileID`s, and field names were checked by hand to confirm scene/prefab
> references still resolve — without a local Unity installation to open the project and confirm
> it compiles or runs. Treat it as reviewed-but-not-executed.

## Project structure

```
First Aid/
├─ Assets/
│  ├─ Scripts/
│  │  ├─ Device/     SoniaDevice.cs, VRInputSender.cs, GetData_M/S1/S2.cs
│  │  ├─ Animation/  CreateAnimationLine.cs, PositionHandle.cs, AnimationLineManager.cs
│  │  └─ Pose/       BonePositionManager.cs
│  ├─ Animation Line.prefab         # root object: script + path anchors + grabbable handle
│  ├─ AnimationPath.prefab / PathMarker.prefab   # glowing marker set spawned along the path
│  ├─ ArcScrubAnimator.controller   # blends the two arm poses, sequenced by AnimationLineManager
│  ├─ Armature_pose1.anim / Armature_pose2.anim
│  ├─ GlowPath.mat / GlowPath.shadergraph / PathArrowIcon.png
│  ├─ ReferenceProp.fbx
│  ├─ Scenes/SampleScene.unity
│  ├─ Settings/      # URP quality presets
│  └─ XR/, XRI/       # OpenXR + XR Interaction Toolkit project settings
├─ Packages/
└─ ProjectSettings/
```

## Known limitations

- The interactive grab-and-scrub path needs an XR Origin/rig, which this repo doesn't vendor
  (see above) — the keyboard-simulated device path works standalone.
- `BonePositionManager` isn't wired into the sample scene; it's provided as source showing the
  pose-comparison approach.
- No automated tests.
