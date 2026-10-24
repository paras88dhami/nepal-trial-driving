# Realistic Mobile Art — Import and Install (Unity 6000.6)

## Status

Graphics integration **source is committed to `dev`**. We cannot download Sketchfab's
Peugeot 205 anonymously: the official download requires the user's Sketchfab
account and CC BY attribution. Unity Asset Store props require the user's own
Asset Store account. The free Poly Haven texture downloader runs **on your PC**;
it has not yet been executed in Unity, and its downloads must be verified there.
No downloaded third-party binaries have been committed to GitHub.

## 1. Get free road textures on Windows

Close Unity, open PowerShell in the repository, then:

```powershell
git status
git pull origin dev
.\scripts\prepare-trial-art.ps1
```

The script asks the official Poly Haven API for 2K JPG Diffuse, Normal (GL),
and Rough maps. It saves them to:

`Assets/Game/Art/Environment/Roads/Asphalt01/`

If its network download is blocked, download manually from
https://polyhaven.com/a/asphalt_01 with resolution 2K.
Optionally run `./scripts/prepare-trial-art.ps1 -IncludeKenneyFallback`
for the **stylized** Kenney CC0 fallback car ZIP (NOT the Peugeot).

## 2. Download chosen Peugeot 205 (requires your login)

Open https://sketchfab.com/3d-models/peugeot-205-low-poly-game-ready-3d-model-8636fde6c69c4a82bcc1d9d3d6f8fbcd

1. Sign in to Sketchfab and click **Download 3D Model**.
2. Download the available glTF archive. Sketchfab normally does **not**
   provide FBX through the public download API.
3. Import the `scene.gltf` into Blender, then export **FBX** with textures
   accessible to Unity; or install a supported glTF importer in Unity.
4. Copy the FBX and relevant textures into
   `Assets/Game/Art/Vehicles/Car/Peugeot205/`.
5. Keep the author and original model URL with game credits (Creative Commons
   Attribution). Verify the downloaded asset's actual license.

**Do not upload a Sketchfab access token or session cookie into the repository.**

## 3. Use the new Unity menu commands

Reopen Unity. Wait for compile, and confirm the connected scenes already exist.

**To install asphalt**

1. In Project panel select the imported `asphalt_01_diff_2k.jpg`.
2. **Nepal Trial > Graphics > Apply Selected Road Texture to Full Grounds**.
3. It installs `Trial_Asphalt_URP.mat` and updates road renderers in car,
   bike and scooter scenes. Existing connected scene files are backed up
   under `Build/GraphicsSceneBackups/`.

**To install your Peugeot**

1. Select the imported **Peugeot FBX or prefab asset** in the Project panel.
2. **Nepal Trial > Graphics > Attach Selected Car Model to Full Car**.
3. The tool backs up the scene, adds `TrialCar/CarGraphics/<Peugeot>`,
   disables old cube visuals and strips imported Rigidbody/colliders, keeping
   `CarDriveController` and the existing exam sensors.
4. Open `NepalTrial_FullCar` and select `CarGraphics`. Set position, facing
   (+Z forward) and scale (about 3.7m long) in Inspector.
5. On `TrialCar/CarWheelVisualSync`, drag the visible **front-left,
   front-right, rear-left, rear-right wheel mesh transforms** from the
   imported model into their corresponding `...Mesh` fields.
   The WheelCollider fields are already prelinked. If the FBX has no
   separate wheels, wheel animation needs mesh separation in Blender.

**To install a touch steering wheel**

1. **Nepal Trial > Graphics > Add Analog Steering Wheel to Full Car**.
2. The tool generates a transparent steering-wheel Sprite in
   `Assets/Game/Art/UI/DrivingControls/SteeringWheel.png`.
3. It installs a drag-operated wheel and hides old Left/Right UI buttons
   in the connected car scene. Keyboard A/D still works; gas/brake/gear and
   other controls are preserved.
4. Start Play Mode and test multitouch on Android or touch simulator.

## 4. Safety and next steps

- **Do not run Connected Exam > Rebuild** after manual art edits without
  committing/backing them up first: the scene generator recreates scenes and
  will discard hand-added visual objects. Graphics commands back up the scene
  files before modifying them.
- This is a **visual integration layer**, not an official site replica.
  Imported asphalt does not change physical boundaries or scoring logic.
- The Peugeot listing **does not guarantee a modeled interior**. A real
  first-person cockpit and matching dashboard need a separate 3D interior
  asset or custom modeling and camera integration.
- Add traffic props through the Unity Asset Store under your own account,
  then place them outside driving lanes. Do not replace traffic-light sensor
  GameObjects with decorative meshes.
- License links: Kenney CC0 https://kenney.nl/assets/car-kit ;
  Poly Haven CC0 https://polyhaven.com/a/asphalt_01 ;
  Peugeot CC BY model link above.
- After installation run EditMode tests and drive each full course in PlayMode.
  No completed Android build or physics validation is claimed here.
