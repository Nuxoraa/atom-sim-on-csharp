using Raylib_cs;
using System;
using System.Numerics;

class Program
{
    static void Main()
    {
        Raylib.InitWindow(1280, 800, "Quantum Atom Cloud");
        Raylib.SetTargetFPS(60);
        Raylib.DisableCursor();

        Camera3D camera = new Camera3D(
            new Vector3(15, 15, 15),
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            45.0f,
            0 
        );

        int maxParticles = 5000;
        Vector3[] positions = new Vector3[maxParticles];
        Color[] colors = new Color[maxParticles];
        Random rnd = new Random();

        float stdDevScale = 2.5f;

        for (int i = 0; i < maxParticles; i++)
        {
            float u1 = (float)rnd.NextDouble();
            float u2 = (float)rnd.NextDouble();
            float randStdDev = (float)Math.Sqrt(-2.0 * Math.Log(u1 + 0.00001f)); 

            float x = randStdDev * (float)Math.Cos(2.0 * Math.PI * u2) * stdDevScale;
            float y = randStdDev * (float)Math.Sin(2.0 * Math.PI * u2) * stdDevScale;
            float z = ((float)rnd.NextDouble() - 0.5f) * 10.0f;

            positions[i] = new Vector3(x, y, z);

            float dist = positions[i].Length();
            if (dist < 1.5f) colors[i] = Color.White;
            else if (dist < 3.5f) colors[i] = Color.Yellow;
            else colors[i] = Color.Violet;
        }

        while (!Raylib.WindowShouldClose())
        {
            Raylib.UpdateCamera(ref camera, CameraMode.Free);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            Raylib.BeginMode3D(camera);

            Raylib.DrawSphere(Vector3.Zero, 0.5f, Color.White);

            for (int i = 0; i < maxParticles; i++)
            {
                Raylib.DrawCube(positions[i], 0.05f, 0.05f, 0.05f, colors[i]);

                positions[i].X += (float)(rnd.NextDouble() - 0.5) * 0.02f;
                positions[i].Y += (float)(rnd.NextDouble() - 0.5) * 0.02f;
            }

            Raylib.EndMode3D();

            Raylib.DrawText("Quantum Atom Simulation", 10, 10, 20, Color.White);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
