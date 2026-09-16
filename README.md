# VACAC Modular Conveyor System

A Unity application for building modular conveyor lines at runtime and
simulating products travelling along them. Conveyor pieces are selected,
placed and snapped together with the mouse, and products spawn at the head
of a line and travel along it to the end.

Built for the VACAC Graduate Software Engineer take-home technical task.

## Requirements met

- Runs as a standalone Windows build
- Conveyor pieces can be selected and placed into the environment
- Pieces snap together end-to-end to form a continuous conveyor line
- Products spawn at the head of a line and move along it

## Beyond the minimum requirements

- Four conveyor types — long belt, short belt, incline and decline —
  selectable at runtime with the number keys
- Two product types, spawned at random
- Translucent placement preview that highlights green when a snap is available
- Direction flipping, which swaps which end of a piece acts as its entry
- Right-click removal that unlinks neighbouring pieces, with automatic
  reconnection when a replacement is placed in the gap
- RTS-style camera with pan, orbit, tilt and zoom
- On-screen control reference and live counters for pieces placed and
  products delivered
- Pause and quit handling

## Controls

| Input | Action |
|---|---|
| Mouse move | Position the conveyor preview |
| Left click | Place the previewed conveyor |
| Right click | Delete the conveyor under the cursor |
| 1 / 2 / 3 / 4 | Select conveyor type (long / short / incline / decline) |
| R | Rotate the preview 90 degrees |
| F | Reverse the preview's direction of travel |
| Space | Start / stop product spawning |
| W A S D | Pan the camera |
| Q / E | Orbit the camera |
| T / G | Tilt the camera |
| Scroll wheel | Zoom in and out |
| P | Pause / resume |
| Esc | Quit |

## How it works

**Sockets.** Each conveyor prefab is an empty root object with the supplied
model as a child, plus two empty child transforms acting as an entry socket
and an exit socket. Both sockets point along the direction products travel.
That convention is deliberate: because the arrows both face downstream,
joining two pieces means giving the new piece's entry socket exactly the
same world position and rotation as the target's exit socket, rather than
mirroring one against the other. The alignment code is four lines as a
direct result.

Wrapping the model in a clean root also isolates the supplied FBX files'
authored transform — they import with a 90 degree rotation and a scale of
100 — so that all placement code works against a root with identity
rotation and unit scale.

**Placement.** A translucent preview instance follows the cursor via a
raycast onto the ground layer. Each frame the placer searches its list of
placed pieces for the nearest free exit socket within a snap radius; if one
is found, the preview aligns to it and tints green. Colliders on the preview
are disabled so it cannot block its own raycast, and its materials are
runtime copies so tinting never modifies the shared material assets.

Because the alignment matches full 3D transforms rather than working on the
ground plane, the inclined and declined pieces snap through exactly the same
code path as the flat ones, with no special handling for height.

**Product movement.** Products are moved kinematically rather than with
physics. Each product stores which segment it is on and how far along that
segment it has travelled, then asks the segment for the world position at
that distance. When it passes the end, the overshoot carries onto the next
segment so there is no stutter at a join. This keeps movement frame-rate
independent and stable across joins and corners, which a friction- or
force-based approach would not be, and it makes the position of every
product deterministic at any moment.

**Connections.** Segments link in both directions. Placing a piece links it
backwards to whatever it snapped onto, and also links it forwards if another
piece's free entry socket is already sitting on its exit. The forward check
uses a much tighter tolerance than the snap radius, since it should only
fire when two sockets are effectively coincident. Together this means a
piece deleted from the middle of a line can be replaced and the line
reconnects on both sides.

**Flipping.** A segment can be flipped, which swaps which of its two sockets
is reported as the entry and which as the exit. Every other system reads the
sockets through those properties, so reversing a piece's direction of travel
needs no changes anywhere else — the snapping, the length calculation and
the product movement all follow automatically.

**Scripts.**

| Script | Responsibility |
|---|---|
| `ConveyorSegment` | One conveyor piece: its sockets, its length, the position at a given distance along it, and its links to neighbours |
| `ConveyorPlacer` | Runtime placement, snapping, rotation, flipping, type selection and removal |
| `GhostVisual` | Makes the preview translucent and tints it based on snap state |
| `Product` | Moves a single product along the chain of connected segments |
| `ProductSpawner` | Spawns products onto the head of a line at a fixed interval |
| `CameraController` | Pan, orbit, tilt, zoom and clamping |
| `HudDisplay` | On-screen control reference and live counters |
| `ApplicationController` | Pause and quit handling |

The HUD uses Unity's immediate-mode GUI rather than a Canvas, which keeps the scene simpler for an overlay of this size. A Canvas-based UI would be the right choice for anything more complex. 
## Project structure

```
Assets/
  _Project/
    Materials/   ground material
    Prefabs/     conveyor and product prefabs
    Scripts/     all custom code
  Scenes/
    Main.unity   the only scene
  VACAC/         models supplied by VACAC (not committed — see below)
```

## Running from source

1. Clone this repository
2. Open the project in Unity 6000.0.36f1
3. Import `VACAC_Graduate_Software_Engineer_Take-Home_Technical_Task.unitypackage`
   via Assets > Import Package > Custom Package, then click All and Import
4. Open `Assets/Scenes/Main.unity` and press Play

The supplied FBX models are between 125 MB and 241 MB each, which exceeds
GitHub's 100 MB per-file limit, so `Assets/VACAC/` is excluded from the
repository. Importing the package restores those files with their original
asset GUIDs, so every prefab reference in the scene resolves correctly.

## Running the build

Extract the zipped release and run `ConveyorTask.exe`. No installation
required.

## Development tools

- Unity 6000.0.36f1 (Unity 6 LTS), Built-In Render Pipeline
- C# in Microsoft Visual Studio
- Visual Studio Code for documentation
- Git and GitHub for version control

## Known limitations

- A connected line cannot change direction. When a piece snaps, its rotation
  is taken entirely from the socket it is joining, so the R key only affects
  pieces placed in open space. The supplied models include no corner piece,
  and turning a line would require either a curved segment or allowing the
  snap to apply a fixed rotation offset at the join
- Two inclined pieces cannot be chained into a continuous ramp. The sockets
  are not rotated to match the slope, so a piece snapping onto an incline's
  exit arrives level rather than continuing the climb. Rotating the sockets
  in the prefab would resolve this, at the cost of flat pieces arriving
  tilted when they come off a ramp
- A piece deleted from the middle of a line can only be replaced by a piece
  of the same length. A shorter or longer replacement leaves its exit socket
  away from the next piece's entry socket, so the forward reconnection does
  not fire and the line stays broken
- Snapping only joins a new piece's entry socket to an existing free exit
  socket. Two separately built lines cannot be joined by placing one against
  the other, and a line cannot be extended backwards from its head
- Products follow the path of an inclined segment but do not rotate to match
  its slope; they stay axis-aligned throughout
- Products riding a deleted segment are removed rather than re-routed
- Built layouts cannot be saved or loaded


## Repository

https://github.com/ADKgit/vacac-conveyor-system