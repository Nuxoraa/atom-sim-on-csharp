using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;

class Program
{
    struct OrbitalPoint
    {
        public Vector3 OriginalPosition;
        public Color Color;
        public float Alpha;
        public float Size;
    }

    static float Psi2pZ(float r, float cosTheta)
    {
        float a0 = 1.0f;
        float rho = r / a0;
        return (float)(1.0 / (4.0 * Math.Sqrt(2.0 * Math.PI)) * Math.Pow(rho, 1) * Math.Exp(-rho / 2.0) * cosTheta);
    }

    static float ProbabilityDensity(Vector3 pos)
    {
        float r = pos.Length();
        if (r < 1e-4f) return 0f;
        float cosTheta = pos.Z / r;
        float psi = Psi2pZ(r, cosTheta);
        return psi * psi * r * r;
    }

    static Color GetDensityColor(Vector3 pos, float maxProb)
    {
        float r = pos.Length();
        if (r < 1e-4f) return Color.White;

        float cosTheta = pos.Z / r;
        bool upperLobe = pos.Z > 0;

        float prob = ProbabilityDensity(pos);
        float t = Math.Clamp(prob / maxProb, 0f, 1f);
        float brightness = 0.3f + t * 0.7f;

        if (upperLobe)
        {
            return new Color(
                (byte)(255 * brightness),
                (byte)(80 * brightness),
                (byte)(220 * brightness),
                (byte)(180 + t * 75)
            );
        }
        else
        {
            return new Color(
                (byte)(60 * brightness),
                (byte)(180 * brightness),
                (byte)(255 * brightness),
                (byte)(180 + t * 75)
            );
        }
    }

    static void DrawAxes()
    {
        float len = 8f;
        float thick = 0.04f;
        Raylib.DrawCylinderEx(new Vector3(-len, 0, 0), new Vector3(len, 0, 0), thick, thick, 6, new Color(255, 80, 80, 120));
        Raylib.DrawCylinderEx(new Vector3(0, -len, 0), new Vector3(0, len, 0), thick, thick, 6, new Color(80, 255, 80, 120));
        Raylib.DrawCylinderEx(new Vector3(0, 0, -len), new Vector3(0, 0, len), thick, thick, 6, new Color(80, 140, 255, 120));
    }

    static void DrawOrbitalOutline(float rotationAngle)
    {
        int steps = 64;
        float lobeRadius = 4.2f;
        float lobeCenter = 3.5f;

        for (int lobe = -1; lobe <= 1; lobe += 2)
        {
            for (int i = 0; i < steps; i++)
            {
                float phi = (float)i / steps * MathF.PI * 2;
                float phi2 = (float)(i + 1) / steps * MathF.PI * 2;

                float x1 = lobeRadius * MathF.Cos(phi);
                float y1 = lobeRadius * MathF.Sin(phi);
                float z1 = lobe * lobeCenter;

                float x2 = lobeRadius * MathF.Cos(phi2);
                float y2 = lobeRadius * MathF.Sin(phi2);
                float z2 = lobe * lobeCenter;

                float c = MathF.Cos(rotationAngle);
                float s = MathF.Sin(rotationAngle);

                Vector3 p1 = new Vector3(x1 * c - z1 * s, y1, x1 * s + z1 * c);
                Vector3 p2 = new Vector3(x2 * c - z2 * s, y2, x2 * s + z2 * c);

                Color outlineColor = lobe > 0
                    ? new Color(255, 100, 230, 60)
                    : new Color(80, 200, 255, 60);

                Raylib.DrawLine3D(p1, p2, outlineColor);
            }
        }
    }

    static void Main()
    {
        Raylib.InitWindow(1280, 720, "Hydrogen 2pz Orbital — Quantum Simulation");
        Raylib.SetTargetFPS(60);
        Raylib.DisableCursor();

        Camera3D camera = new Camera3D(
            new Vector3(22, 14, 22),
            Vector3.Zero,
            new Vector3(0, 1, 0),
            45.0f,
            CameraProjection.Perspective
        );

        var points = new List<OrbitalPoint>();
        var rnd = new Random(42);

        float maxSample = 0f;
        int sampleCount = 5000;
        for (int i = 0; i < sampleCount; i++)
        {
            float r = (float)(rnd.NextDouble() * 18);
            float theta = (float)(rnd.NextDouble() * Math.PI);
            float phi = (float)(rnd.NextDouble() * 2 * Math.PI);
            float x = r * MathF.Sin(theta) * MathF.Cos(phi);
            float y = r * MathF.Sin(theta) * MathF.Sin(phi);
            float z = r * MathF.Cos(theta);
            float p = ProbabilityDensity(new Vector3(x, y, z));
            if (p > maxSample) maxSample = p;
        }

        float rejectionMax = maxSample * 1.1f;
        int generated = 0;
        int maxAttempts = 5000000;

        while (generated < 80000 && maxAttempts-- > 0)
        {
            float r = (float)(rnd.NextDouble() * 18);
            float theta = (float)(rnd.NextDouble() * Math.PI);
            float phi = (float)(rnd.NextDouble() * 2 * Math.PI);

            float x = r * MathF.Sin(theta) * MathF.Cos(phi);
            float y = r * MathF.Sin(theta) * MathF.Sin(phi);
            float z = r * MathF.Cos(theta);

            Vector3 pos = new Vector3(x, y, z);
            float prob = ProbabilityDensity(pos);

            if (rnd.NextDouble() < prob / rejectionMax)
            {
                float t = Math.Clamp(prob / rejectionMax, 0f, 1f);
                float size = 0.035f + t * 0.045f;

                OrbitalPoint p = new OrbitalPoint();
                p.OriginalPosition = pos;
                p.Color = GetDensityColor(pos, rejectionMax);
                p.Alpha = 0.5f + t * 0.5f;
                p.Size = size;
                points.Add(p);
                generated++;
            }
        }

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

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(4, 4, 12, 255));

            Raylib.BeginMode3D(camera);

            if (showAxes) DrawAxes();
            if (showOutline) DrawOrbitalOutline(rotationAngle);

            float nucleusGlow = 0.28f + 0.06f * MathF.Sin(time * 3f);
            Raylib.DrawSphere(Vector3.Zero, nucleusGlow, new Color(255, 240, 180, 255));
            Raylib.DrawSphereWires(Vector3.Zero, nucleusGlow + 0.12f, 8, 8, new Color(255, 200, 80, 60));

            float sinR = MathF.Sin(rotationAngle);
            float cosR = MathF.Cos(rotationAngle);

            foreach (var pt in points)
            {
                Vector3 orig = pt.OriginalPosition;

                float rx = orig.X * cosR - orig.Z * sinR;
                float rz = orig.X * sinR + orig.Z * cosR;
                float ry = orig.Y;

                Vector3 rotated = new Vector3(rx, ry, rz);

                float wave = MathF.Sin(time * 1.5f + orig.Length() * 0.4f) * 0.04f;
                rotated += Vector3.Normalize(rotated) * wave;

                Raylib.DrawCube(rotated, pt.Size, pt.Size, pt.Size, pt.Color);
            }

            Raylib.EndMode3D();

            Raylib.DrawFPS(10, 10);

            Raylib.DrawText("Hydrogen 2pz Orbital", 10, 35, 18, new Color(200, 200, 255, 200));
            Raylib.DrawText($"Points: {points.Count}", 10, 58, 16, new Color(160, 160, 200, 180));
            Raylib.DrawText("[A] Axes  [O] Outline  [Space] Auto-rotate", 10, 690, 14, new Color(120, 120, 160, 180));

            int legendY = 180;
            Raylib.DrawRectangle(10, legendY, 18, 18, new Color(255, 80, 220, 200));
            Raylib.DrawText("+Z lobe", 34, legendY + 1, 15, new Color(200, 180, 255, 200));
            Raylib.DrawRectangle(10, legendY + 24, 18, 18, new Color(60, 180, 255, 200));
            Raylib.DrawText("-Z lobe", 34, legendY + 25, 15, new Color(180, 220, 255, 200));

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
