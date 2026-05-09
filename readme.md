# MonoGame Pixel Planets

Originally created by **Deep-Fold**, ported by **Enthusiast Guy**.

---

## 0. Showcase

https://www.youtube.com/watch?v=wnwpnkBD6OQ

---

## 1. About

### 1.1. This document history

**13.03.2021**

- Added this history section so people who read this long text before can jump straight to updates without re-reading the whole thing.
- Updated sections: 2.1.2 (Shaders).

**09.05.2026**

- **Documentation:** Restored the full legacy **`README`** material into **`readme.md`** (personal note, MIT text, project structure, shader debugging, version history, known issues / planned work) and merged it with the newer sections (Godot parity table, controls, exports, blend notes). Later edits document GIF timing parity and **`MonoGame-Pixel-Planets/.gitignore`**.
- **Godot parity — celestials:** Reordered and renamed bodies to match the Godot UI (e.g. “Terran Wet”, “Gas giant 1/2”). Added **Galaxy** (`Galaxy.fx`) and **Black Hole** (`BlackHole.fx` composite: horizon + accretion disk in one effect). Extended **`Predefined.cs`** with parameter sets for both.
- **Godot parity — Star:** **Three-pass** rendering: **`StarBlobs.fx`**, **`Star.fx`** (surface), **`StarFlares.fx`**; wired via **`Celestial.PassShaderIds`** and **`Shaders.CelestialEffects`**. **`Renderer`** draws each pass in order. **`state.json`** migration merges **`PassShaderIds`** from templates when missing so older saves still get three layers.
- **`Shaders.cs`:** Multi-effect load/update; shared uniforms applied to every pass; **`ComputeShaderTime`** (live / sprite sheet) handles Star layers, Galaxy, Black Hole disk time; snapshot of changed params so **rotation/seed** apply to all Star passes in one frame; Black Hole **rotation + 0.7**; Galaxy/BlackHole/Star pixel scaling rules (**pixels**, **`pixels_ring`** for Black Hole).
- **GIF export (F3) — Godot alignment:** Phase **`i / frameCount`** matches Godot **`lerp(0, 1, i/float(frames))`** (even spacing on a loop, including wrap last→first). While exporting, **`State.ExportGifGodotTimeMapping`** drives **`time`** via **`ComputeGodotGifShaderTime`** — the same family of formulas as Godot’s **`set_custom_time(t)`** per planet (e.g. **`t * get_multiplier`**, Galaxy **`t * 2π * time_speed`**, Black Hole disk **`t * 314.15 * time_speed * 0.5`**, Star surface **`t / time_speed`**, …), **not** live **`update_time`** / **`ComputeShaderTime`**. **Asteroid:** **`rotation = t * 2π`**, **`time = 0`**, matching **`Asteroid.gd`**. **`AdjustGifFrameCountForPlanetTimeSpeed`** can raise frame counts when the primary UI time-speed slider is below **1** (smoother slow settings). Indexed GIF via **SixLabors.ImageSharp**; defaults **32** frames, delay **5** (5/100 s per frame); **`RepeatCount`** infinite.
- **Other exports:** **P** — planet-only PNG (save dialog). **F2** — sprite sheet PNG (default 8×8, optional margin); phase **0…1** across cells using **`ComputeSpriteSheetExportAnimationTimeScale`** (one live-update step per cell, **`TimeModifier`**). **`State`:** **`ExportAnimationActive`**, **`ExportAnimationPhase01`**, **`ExportAnimationTimeScale`**, **`SuspendTimeForExport`**; **`State.ApproxUpdatesPerSecond`** from **`TargetElapsedTime`** (used with sprite-sheet / timing helpers). **`Renderer`:** **`CapturePlanetToPng`**, **`CapturePlanetPixels`**; captures use **alpha blend** + transparent clear where needed.
- **Viewport / blending:** Main window clears **black** and draws with **`BlendState.Additive`** (restored original-port behaviour; avoids the wrong flat white/grey plate behind the sphere that appeared when using standard alpha blend on the backbuffer). Off-screen captures still use **alpha blend** + transparent clear for correct file alpha.
- **Lifecycle:** **`Shaders.UpdateShader()`** moved from **`Initialize`** to **`LoadContent`** (after **`Renderer.Initialize`**) so content and graphics are ready before loading effects.
- **UI refactor:** Split into **`ParameterInspector.cs`** (parameter column interaction) and **`PlanetHud.cs`** (titles, help text); **`UI.cs`** delegates to them. **`UserInput`:** **P**, **F2**, **F3**, **X**; **`PixelPlanets.csproj`:** **SixLabors.ImageSharp** for GIF/sheet assembly.
- **Repo hygiene:** **`.gitignore`** next to the MonoGame solution (same folder as **`PixelPlanets.csproj`**) so Git ignores **`bin/`**, **`obj/`**, **`.vs/`**, **`Content/bin|obj`**, etc., when the repo root is **`MonoGame-Pixel-Planets`**. If those folders were tracked before ignores were added, run **`git rm -r --cached`** on them once. (A parent workspace **`.gitignore`** may still exist for a monorepo.)

*(Godot-only features still not in this build: HTML colour import/export popups, layer toggles, web download, GIF/sprite UI parity with Godot’s progress popups. Land/Rivers in Godot uses separate land vs cloud **`time`** in export; this port keeps one **`time` uniform.)*

### 1.2. Personal note

This project is somewhat branched off this one right here: https://github.com/Deep-Fold/PixelPlanets  
The author, **Deep-Fold**, is the original wizard of all this, and large amounts of praise should go his way.  
Please visit his itch.io project page here: https://deep-fold.itch.io/pixel-planet-generator

I'm the **Enthusiast Guy**, responsible for this port to MonoGame. You can find me on YouTube:  
https://www.youtube.com/channel/UCs3I6aDQ6Hj7m6_9l0HBgQA

I've stumbled upon this crazy cool gem just a few days before writing this. I have my own game project that I'm working on, and it so happened that I needed planets for some parts of my game. The original project is written with **Godot** (https://godotengine.org); however my own game project is written quite extensively in **MonoGame**. The shaders Deep-Fold wrote are in **GLSL** (https://en.wikipedia.org/wiki/OpenGL_Shading_Language) while MonoGame uses **HLSL** (https://en.wikipedia.org/wiki/High-Level_Shading_Language). Basically think of them as OpenGL vs DirectX. Because MonoGame's ancestor is XNA, written by Microsoft for Xbox 360, you can see why it would not use OpenGL shaders.

I had never worked with shaders before this, but I had hoped that this wonderful project could somehow be converted to the HLSL that MonoGame supports — and in fact it did, as this very project stands witness to.

Following Deep-Fold's guidance in this new chapter of shaders, I was able to port this while honestly not fully understanding all the math behind it — for the moment, at least.

This workspace also contains a **Godot** reference implementation (`Godot-Pixel-Planets`) with `gdshader` materials; the MonoGame side ports those ideas to **HLSL** under `Content/ShadersV2/`.

### 1.3. Further development / support

I do intend to bring this one to a stable form, as accurate as possible, and I'll provide moderate support if any bugs are reported; however, please consider that my main work is concentrated elsewhere and I may not have time for substantial features. Moreover, I believe this should remain a rather simple project. Feel free to fork it, though, and let me know what you did with it.

### 1.4. Use / LICENSE

The original author released his work under the **MIT** license and specifically mentioned we can do anything we want with it, under any circumstances, while (not obligatory) a mention of his name would be nice. The same conditions are valid here.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

See also the `LICENSE` file in this folder.

---

## 2. Project

**Important.** This project is **not** an example in code tidiness. Please don't assume it is. A lot of shortcuts were taken to get to the desired result; the central piece of this is the **actual shaders**, not the code architecture.

### 2.1. Structure

#### 2.1.1. Entry point

The entry point of the application is **`PixelPlanets.cs`**. It contains the most important methods: **Update()** and **Draw()**.

Ideally **Update** should prepare everything in state and **Draw** should simply draw. That separation is not always perfect here; if you use MonoGame to build your game, try to respect it more cleanly.

The **`State.cs`** class contains the state of most things in this app. It also contains the persisted data that is saveable to **`state.json`** with **F1**.

Additional UI-related types:

- **`UserInterface/UI.cs`** — orchestrates update/draw.
- **`UserInterface/ParameterInspector.cs`** — hit-testing and wheel editing for the parameter column.
- **`UserInterface/PlanetHud.cs`** — titles, help lines, and parameter labels.
- **`General/Renderer.cs`** — sprite batches, planet passes, capture via render targets.
- **`General/Shaders.cs`** — loads effects, pushes uniforms, multi-pass handling for Star, export time override during GIF/sheet capture.

#### 2.1.2. Shaders

As of version **1.2.0.17**, the whole project was migrated from MonoGame **3.7** to **3.8**. That introduced a problem when compiling shaders: *"unable to unroll loop, loop does not appear to terminate in a timely manner"*. After investigation, the fix was to change **one line** in **`planet_utils.fx`**:

```text
from:  #define PS_SHADERMODEL ps_4_0_level_9_1
to:    #define PS_SHADERMODEL ps_4_0
```

See: https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/specifying-compiler-targets  

Originally shader model 2-ish profiles were used; moving to **SM 4.0** removed the issue. Devices that don't support shader model 4 can no longer run this code. Attempting SM 3.0 was met with: *"Pixel shader must be SM 4.0 level 9.1 or higher"*.

You'll find the shaders in **`Content/ShadersV2/`**. All shaders that draw planets/stars include a common file at the top: **`planet_utils.fx`**, which holds shared math.

Each shader exposes **parameters** (non-static `float`, `float2`, etc.) so MonoGame can assign them:

```csharp
shader.Parameters["time"].SetValue(yourFloatValue); // See Shaders.cs
```

Many shaders split work into layer helper functions and compose them in **`MainPS`**. **`MainPS`** receives **`VertexShaderInput`** with **TextureCoordinates** (the current pixel's UV). Layer functions compute land, lakes, clouds, etc., and **`MainPS`** returns the final colour for that pixel.

**Rendering note:** The **main window** clears to **opaque black** and draws with **`BlendState.Additive`** (original port behaviour). These HLSL outputs were tuned for additive stacking on a dark background. Using standard **alpha blend** on the backbuffer tends to show a **flat white or tinted rectangle** behind the sphere — that is a **blend-mode / SpriteBatch** issue, not necessarily a bug inside a single `.fx` file.

**Export / render-target captures** (PNG, GIF, sprite sheet) use an off-screen target with **`BlendState.AlphaBlend`** and a **transparent** clear where needed so alpha outside the disk is preserved in files.

#### 2.1.3. Debugging shaders

Arm yourself with patience.

To compile shaders, open **`Content/Content.mgcb`** — ideally that launches the **MonoGame Pipeline Tool**. If it opens as plain text, right-click **`Content.mgcb`** → **Open With** → choose **MonoGame Pipeline Tool**, set as default.

Right-click a shader under **`ShadersV2`** and select **Rebuild**. Green output means success.

Because of **`#include`**, error **line numbers** may not match your editor — they can point higher than the real location. A workaround is to temporarily paste **`planet_utils.fx`** contents into the shader you're editing and remove the `#include` until the error is fixed, then restore the include. Avoid permanently duplicating that code.

Shaders are **pre-processed**: if you comment out code, the compiler may strip **unused parameters**, and you can get errors when C# still tries to set a uniform that no longer exists.

In **GLSL** you might write `vec2(.5)` meaning `(0.5, 0.5)`. In **HLSL** you must write **`float2(0.5, 0.5)`** explicitly.

#### 2.1.4. Packages

- **Newtonsoft.Json** — serialize/deserialize **`state.json`**.
- **SixLabors.ImageSharp** — GIF export and assembling sprite-sheet PNGs.

---

## Requirements

- Windows (project targets **`netcoreapp3.1`**, MonoGame **3.8** Windows DX).
- Visual Studio or **`dotnet`** CLI.
- MonoGame Content Pipeline (MGCB) if you edit **`Content/Content.mgcb`** or shaders.

## Build & run

```bash
dotnet build
dotnet run --project PixelPlanets.csproj
```

Or open **`PixelPlanets.sln`** and run with F5.

After editing an **`.fx`** file, rebuild that asset in the Pipeline Tool or rebuild the project so MGCB recompiles effects.

---

## Celestial bodies (parity with Godot)

Bodies follow the same order as the Godot UI (`GUI.gd`):

| Name | Shader(s) |
|------|-----------|
| Terran Wet | `LandRivers.fx` |
| Terran Dry | `TerranDry.fx` |
| Islands | `LandMasses.fx` |
| No atmosphere | `NoAtmosphere.fx` |
| Gas giant 1 | `GasPlanet.fx` |
| Gas giant 2 | `GasPlanetLayers.fx` |
| Ice World | `IceWorld.fx` |
| Lava World | `LavaWorld.fx` |
| Asteroid | `Asteroid.fx` |
| Black Hole | `BlackHole.fx` (event horizon + disk composite) |
| Galaxy | `Galaxy.fx` |
| Star | `StarBlobs.fx`, `Star.fx`, `StarFlares.fx` |

**Star** uses three passes (blobs, surface, flares). **Black Hole** combines disk and hole in one effect matching the Godot layout.

---

## Controls

| Input | Action |
|--------|--------|
| Left / Right | Previous / next celestial |
| Up / Down | Increase / decrease pixel resolution (`pixels` uniform) |
| Space | Pause / resume time progression |
| Mouse left drag on planet | Move light origin (where the shader supports `light_origin`) |
| Mouse right drag | Scrub custom time offset |
| Tab | Randomize visible parameters |
| ` (tilde) | Restore defaults |
| F1 | Save **`state.json`** next to the executable |
| **P** | Export planet-only **PNG** (transparent outside the sphere; save dialog) |
| **F2** | Export **sprite sheet** (default 8×8 grid, no margin; save dialog) |
| **F3** | Export **animated GIF** (default 32 frames, 5/100 s per frame; save dialog) |
| **X** | Full-window screenshot including UI |
| Esc | Exit |

Click a parameter label, hold **left mouse**, and scroll the wheel to change values; **middle-click** resets the focused parameter to its default while “locked”.

---

## Export notes

- **PNG (P)** — Renders at **`PLANET_RECT_SIZE`** (see **`Config.cs`**) with a transparent background outside the planet where applicable.
- **Sprite sheet (F2)** — One PNG tiled row-major; each cell samples animation phase **0 … 1** across **`columns × rows`** frames. Defaults: 8×8, **0** px margin between cells (see code for overloads).
- **GIF (F3)** — Indexed GIF via ImageSharp; defaults **32** frames, delay **5** (= 5/100 second per frame). Phase **`i / frameCount`** matches Godot **`lerp(0, 1, i/float(frames))`**. Shader **`time`** (and Asteroid **`rotation`**) follow Godot’s **`set_custom_time(t)`** per planet — **not** live **`update_time`** / **`ComputeShaderTime`** — so export matches the original GIF behaviour and loops cleanly with Godot’s spacing.

Features that exist in the **Godot** UI but are **not** fully replicated here include: HTML hex colour import/export popups, per-layer visibility toggles, web-specific download paths, and some export UI polish.

---

## State file

**`state.json`** stores parameter values per celestial. Older files without **`PassShaderIds`** get those lists merged from built-in templates so **Star** stays three-pass.

Delete **`state.json`** to regenerate defaults. Invalid edits can crash the app — there are few guards.

---

## 3. Version history

### 1.2.0.17 — March 13, 2021

- Migrated from MonoGame **3.7** to **3.8.0.1641**.
- Small refactoring.

### 0.2.0.17 — March 3, 2021 (first published alpha)

Not all features were connected; some were missing.

**Features**

- GLSL → HLSL conversion of pixel shaders from the original Pixel Planets project (Asteroid, GasPlanet, IceWorld, LandMasses, LandRivers, LavaWorld, NoAtmosphere, Star, TerranDry).
- Some optimizations/refactoring allowed by MonoGame.
- Canvas-style planet preview.

**User interface**

- Browse planets (**LEFT** / **RIGHT**).
- Randomize parameters (**TAB**) — experimental; procedural seeding was hoped for later.
- Restore defaults (**~**).
- Resolution (**UP** / **DOWN**).
- Pause rotation (**SPACE**).
- Move light (**left mouse** on planet area).
- Manual time scrub (**right mouse** drag).
- Scroll parameters while holding click on a row; middle-click resets.
- Save state (**F1**) to **`state.json`**.
- Exit (**Esc**).

**Known issues (historical)**

- Shader code still messy; room for cleanup.
- Gas giant II framing vs original (fixed viewport vs ring scale); experimental scale modifier in params.
- Star was incomplete (flares / layering) — later addressed with multi-pass shaders.
- Ice World clouds dithering artefacts on some GPUs — platform/API sensitivity.

**Planned work (historical wishlist)**

- Better procedural seed than blind random.
- Code cleanup.
- Finish star rendering (multi-pass since added).
- Light shading / non-square pixels when zoomed.
- PNG exports: atlases + JSON pointers; layered passes (e.g. clouds separate).
- Colour params in UI (JSON first).
- Shader refactors / shared math.
- Per-body light behaviour where global light is wrong.

*(Several items overlap with later updates — see repository history for what landed.)*

---

## Authors / credits

- **Deep-Fold** — original Pixel Planets shaders and concept.  
- **Enthusiast Guy** — MonoGame port and documentation.  
- Subsequent edits — see git history.
