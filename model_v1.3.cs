using Raylib_cs;
using System;
using System.Numerics;
using System.Threading.Tasks;

class Program
{
    const int PointCount = 80000;
    const float MaxRadius = 18f;
    const float LobeRadius = 4.2f;
    const float LobeCenter = 3.5f;
    const int OutlineSteps = 64;

    static readonly Vector3[] OriginalPositions = new Vector3[PointCount];
    static readonly Color[] Colors = new Color[PointCount];
    static readonly float[] Sizes = new float[PointCount];
    static readonly float[] Radii = new float[PointCount];
    static readonly Vector3[] RotatedPositions = new Vector3[PointCount];

    static readonly Vector3[] OutlineUpper = new Vector3[OutlineSteps];
    static readonly Vector3[] OutlineLower = new Vector3[OutlineSteps];

    static float ProbabilityDensity(float x, float y, float z, float r)
    {
        float cosTheta = z / r;
        float psi = r * MathF.Exp(-r * 0.5f) * cosTheta;
        return psi * psi * r * r;
    }

    static void GeneratePoints()
    {
        var rnd = new Random(42);

        float maxSample = 0f;
        for (int i = 0; i < 5000; i++)
        {
            float r = (float)(rnd.NextDouble() * MaxRadius);
            float theta = (float)(rnd.NextDouble() * MathF.PI);
            float phi = (float)(rnd.NextDouble() * MathF.Tau);
            float sinT = MathF.Sin(theta);
            float p = ProbabilityDensity(
                r * sinT * MathF.Cos(phi),
                r * sinT * MathF.Sin(phi),
                r * MathF.Cos(theta), r);
            if (p > maxSample) maxSample = p;
        }

        float rejMax = maxSample * 1.1f;
        float invRejMax = 1f / rejMax;
        int generated = 0;

        while (generated < PointCount)
        {
            float r = (float)(rnd.NextDouble() * MaxRadius);
            if (r < 1e-4f) continue;
            float theta = (float)(rnd.NextDouble() * MathF.PI);
            float phi = (float)(rnd.NextDouble() * MathF.Tau);

            float sinT = MathF.Sin(theta);
            float x = r * sinT * MathF.Cos(phi);
            float y = r * sinT * MathF.Sin(phi);
            float z = r * MathF.Cos(theta);

            float prob = ProbabilityDensity(x, y, z, r);
            if (rnd.NextDouble() >= prob * invRejMax) continue;

            float t = prob * invRejMax;
            float brightness = 0.3f + t * 0.7f;

            OriginalPositions[generated] = new Vector3(x, y, z);
            Radii[generated] = r;
            Sizes[generated] = 0.035f + t * 0.045f;
            Colors[generated] = z > 0
                ? new Color((byte)(255 * brightness), (byte)(80 * brightness), (byte)(220 * brightness), (byte)(180 + t * 75))
                : new Color((byte)(60 * brightness), (byte)(180 * brightness), (byte)(255 * brightness), (byte)(180 + t * 75));

            generated++;
        }
    }

    static void PrecomputeOutline()
    {
        float step = MathF.Tau / OutlineSteps;
        for (int i = 0; i < OutlineSteps; i++)
        {
            float phi = i * step;
            float cx = LobeRadius * MathF.Cos(phi);
            float cy = LobeRadius * MathF.Sin(phi);
            OutlineUpper[i] = new Vector3(cx, cy, LobeCenter);
            OutlineLower[i] = new Vector3(cx, cy, -LobeCenter);
        }
    }

    static void UpdateRotations(float cosR, float sinR, float time)
    {
        Parallel.For(0, PointCount, i =>
        {
            float ox = OriginalPositions[i].X;
            float oy = OriginalPositions[i].Y;
            float oz = OriginalPositions[i].Z;
            float wave = MathF.Sin(time * 1.5f + Radii[i] * 0.4f) * 0.04f;
            float scale = 1f + wave / (Radii[i] + 1e-4f);
            RotatedPositions[i] = new Vector3(
                (ox * cosR - oz * sinR) * scale,
                oy * scale,
                (ox * sinR + oz * cosR) * scale);
        });
    }

    static void DrawAxes()
    {
        const float len = 8f, thick = 0.04f;
        Raylib.DrawCylinderEx(new Vector3(-len, 0, 0), new Vector3(len, 0, 0), thick, thick, 6, new Color(255, 80, 80, 120));
        Raylib.DrawCylinderEx(new Vector3(0, -len, 0), new Vector3(0, len, 0), thick, thick, 6, new Color(80, 255, 80, 120));
        Raylib.DrawCylinderEx(new Vector3(0, 0, -len), new Vector3(0, 0, len), thick, thick, 6, new Color(80, 140, 255, 120));
    }

    static void DrawOrbitalOutline(float cosR, float sinR)
    {
        Color upperColor = new Color(255, 100, 230, 60);
        Color lowerColor = new Color(80, 200, 255, 60);

        for (int i = 0; i < OutlineSteps; i++)
        {
            int j = (i + 1) % OutlineSteps;

            Vector3 u0 = OutlineUpper[i], u1 = OutlineUpper[j];
            Raylib.DrawLine3D(
                new Vector3(u0.X * cosR - u0.Z * sinR, u0.Y, u0.X * sinR + u0.Z * cosR),
                new Vector3(u1.X * cosR - u1.Z * sinR, u1.Y, u1.X * sinR + u1.Z * cosR),
                upperColor);

            Vector3 l0 = OutlineLower[i], l1 = OutlineLower[j];
            Raylib.DrawLine3D(
                new Vector3(l0.X * cosR - l0.Z * sinR, l0.Y, l0.X * sinR + l0.Z * cosR),
                new Vector3(l1.X * cosR - l1.Z * sinR, l1.Y, l1.X * sinR + l1.Z * cosR),
                lowerColor);
        }
    }

    static void Main()
    {
        Raylib.InitWindow(1280, 720, "Hydrogen 2pz Orbital — Quantum Simulation");
        Raylib.SetTargetFPS(60);
        Raylib.DisableCursor();

        GeneratePoints();
        PrecomputeOutline();

        Camera3D camera = new Camera3D(
            new Vector3(22, 14, 22),
            Vector3.Zero,
            new Vector3(0, 1, 0),
            45.0f,
            CameraProjection.Perspective);

        float rotationAngle = 0f;
        float time = 0f;
        bool showAxes = true;
        bool showOutline = true;
        bool autoRotate = true;

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            time += dt;
            if (autoRotate) rotationAngle += 0.3f * dt;

            if (Raylib.IsKeyPressed(KeyboardKey.A)) showAxes = !showAxes;
            if (Raylib.IsKeyPressed(KeyboardKey.O)) showOutline = !showOutline;
            if (Raylib.IsKeyPressed(KeyboardKey.Space)) autoRotate = !autoRotate;

            Raylib.UpdateCamera(ref camera, CameraMode.Free);

            float cosR = MathF.Cos(rotationAngle);
            float sinR = MathF.Sin(rotationAngle);

            UpdateRotations(cosR, sinR, time);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(4, 4, 12, 255));
            Raylib.BeginMode3D(camera);

            if (showAxes) DrawAxes();
            if (showOutline) DrawOrbitalOutline(cosR, sinR);

            float nucleusGlow = 0.28f + 0.06f * MathF.Sin(time * 3f);
            Raylib.DrawSphere(Vector3.Zero, nucleusGlow, new Color(255, 240, 180, 255));
            Raylib.DrawSphereWires(Vector3.Zero, nucleusGlow + 0.12f, 8, 8, new Color(255, 200, 80, 60));

            for (int i = 0; i < PointCount; i++)
            {
                float s = Sizes[i];
                Raylib.DrawCube(RotatedPositions[i], s, s, s, Colors[i]);
            }

            Raylib.EndMode3D();

            Raylib.DrawFPS(10, 10);
            Raylib.DrawText("Hydrogen 2pz Orbital", 10, 35, 18, new Color(200, 200, 255, 200));
            Raylib.DrawText($"Points: {PointCount}", 10, 58, 16, new Color(160, 160, 200, 180));
            Raylib.DrawText("[A] Axes  [O] Outline  [Space] Auto-rotate", 10, 690, 14, new Color(120, 120, 160, 180));
            Raylib.DrawRectangle(10, 180, 18, 18, new Color(255, 80, 220, 200));
            Raylib.DrawText("+Z lobe", 34, 181, 15, new Color(200, 180, 255, 200));
            Raylib.DrawRectangle(10, 204, 18, 18, new Color(60, 180, 255, 200));
            Raylib.DrawText("-Z lobe", 34, 205, 15, new Color(180, 220, 255, 200));

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
