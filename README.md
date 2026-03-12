# Hydrogen 2pz Orbital Simulation

A real-time 3D quantum mechanics visualization of the hydrogen atom's **2pz orbital** built with [Raylib-cs](https://github.com/chrisdill/raylib-cs) and .NET.

![Platform](https://img.shields.io/badge/platform-.NET%209.0-blueviolet)
![Library](https://img.shields.io/badge/graphics-Raylib--cs-blue)
![License](https://img.shields.io/badge/license-MIT-green)

---

## What is this?

The 2pz orbital is one of the four hydrogen atomic orbitals in the second energy shell (n=2). It has two lobes oriented along the Z-axis, separated by a nodal plane at z=0 where the electron probability drops to zero.

This simulation generates **80,000 points** distributed according to the exact quantum mechanical probability density |ψ|², so the visual density of points directly corresponds to where the electron is most likely to be found.

### Wave function

The 2pz orbital wave function used:

```
ψ₂ₚz(r, θ) = (1 / 4√(2π)) · r · e^(-r/2) · cos(θ)
```

Where:
- `r` — radial distance from the nucleus (in Bohr radii)
- `θ` — polar angle from the Z-axis
- Probability density: `|ψ|² · r²` (includes the spherical volume element)

---

## Features

- Physically accurate point distribution via **rejection sampling** in spherical coordinates
- **80,000 particles** with size and brightness encoding local probability density
- Two-color scheme: magenta (+Z lobe) and cyan (−Z lobe)
- Animated wave perturbation proportional to radial distance
- Pulsing nucleus
- Optional coordinate axes and orbital outline guides
- Free-look camera
- Multithreaded rotation updates via `Parallel.For`

---

## Requirements

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Raylib-cs](https://www.nuget.org/packages/Raylib-cs) (NuGet)

---

## Getting started

```bash
# Create project
dotnet new console -n HydrogenOrbital
cd HydrogenOrbital

# Add Raylib-cs
dotnet add package Raylib-cs

# Replace Program.cs with HydrogenOrbital.cs content
# Then run
dotnet run -c Release
```

> Use `-c Release` — Debug mode is significantly slower with 80k particles.

---

## Controls

| Key | Action |
|-----|--------|
| Mouse | Look around (free camera) |
| `W A S D` | Move camera |
| `Space` | Toggle auto-rotation |
| `A` | Toggle coordinate axes |
| `O` | Toggle orbital outline |
| `Esc` | Exit |

---

## Performance notes

The main bottleneck is `DrawCube` × 80,000 calls per frame. Raylib does not batch draw calls natively, so FPS scales inversely with point count.

Optimizations applied:
- **SoA layout** (Structure of Arrays) — flat arrays instead of a struct array for better cache locality
- **`Parallel.For`** on rotation + wave computation across all CPU cores
- `sin` / `cos` for the rotation angle computed once per frame
- Pre-computed outline geometry
- `MathF` throughout — avoids double-to-float casts

On a mid-range CPU + GPU the simulation runs at 60 FPS. If performance is insufficient, reduce `PointCount`.

---

## Project structure

```
HydrogenOrbital/
├── HydrogenOrbital.csproj
├── Program.cs
└── README.md
```

---

## Physics background

The hydrogen atom is the only atom with analytically exact solutions to the Schrödinger equation. Each orbital is described by three quantum numbers:

| Symbol | Name | Value for 2pz |
|--------|------|----------------|
| n | Principal | 2 |
| l | Azimuthal | 1 |
| m | Magnetic | 0 |

The angular node at θ = 90° (the xy-plane) divides the orbital into two lobes of opposite phase, visualized here as two distinct colors.

---

## License

MIT
