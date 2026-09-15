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

- Two conveyor types, selectable at runtime with the number keys
- Translucent placement preview that highlights green when a snap is available
- Right-click removal that unlinks neighbouring pieces, with automatic
  reconnection when a replacement is placed in the gap
- RTS-style camera with pan, orbit and zoom
- Products are removed automatically at the end of a line

## Controls

| Input | Action |
|---|---|
| Mouse move | Position the conveyor preview |
| Left click | Place the previewed conveyor |
| Right click | Delete the conveyor under the cursor |
| 1 / 2 | Select conveyor type (long / short) |
| R | Rotate the preview 90 degrees |
| Space | Start / stop product spawning |
| W A S D | Pan the camera |
| Q / E | Orbit the camera |
| Scroll wheel | Zoom in and out |

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

**Scripts.**

| Script | Responsibility |
|---|---|
| `ConveyorSegment` | One conveyor piece: its sockets, its length, the position at a given distance along it, and its links to neighbours |
| `ConveyorPlacer` | Runtime placement, snapping, rotation, type selection and removal |
| `GhostVisual` | Makes the preview translucent and tints it based on snap state |
| `Product` | Moves a single product along the chain of connected segments |
| `ProductSpawner` | Spawns products onto the head of a line at a fixed interval |
| `CameraController` | Pan, orbit, zoom and clamping |

## Project structure

```
Assets/
  _Project/
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

- Snapping only joins a new piece's entry socket to an existing free exit
  socket. Two separately built lines cannot be joined by placing one against
  the other
- Products riding a deleted segment are removed rather than re-routed
- Built layouts cannot be saved or loaded
- The supplied inclined conveyor model is not yet included as a placeable
  type, though the socket alignment is written in full 3D and would support
  it without code changes
- Only one product type is currently spawned

## Repository

https://github.com/ADKgit/vacac-conveyor-system