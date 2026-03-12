using Raylib_cs;
using System; 
using System.Numerics;

class Program
{
    static void Main()
    {
        Raylib.InitWindow(1000, 800, "3D Симуляция Атома (Модель Бора)");
        Raylib.SetTargetFPS(60);

        Raylib.DisableCursor(); 

        Camera3D camera = new Camera3D(
            new Vector3(12, 12, 12),     
            new Vector3(0, 0, 0),       
            new Vector3(0, 1, 0),       
            45.0f,
            CameraProjection.Perspective
        );

        float[] electronAngles = { 0.0f, 1.5f, 3.0f };
        float[] orbitRadius = { 4.0f, 6.0f, 6.0f };
        float[] orbitSpeeds = { 2.0f, 1.2f, 1.2f };


        while (!Raylib.WindowShouldClose())
        {

            float dt = Raylib.GetFrameTime();

            Raylib.UpdateCamera(ref camera, CameraMode.Free); 

            for (int i = 0; i < electronAngles.Length; i++)
            {
                electronAngles[i] += orbitSpeeds[i] * dt;
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black); 

            Raylib.BeginMode3D(camera); 

                Raylib.DrawSphere(new Vector3(0, 0, 0), 0.8f, Color.Red);   
                Raylib.DrawSphere(new Vector3(0.5f, 0, 0), 0.7f, Color.Gray); 
                Raylib.DrawSphere(new Vector3(-0.2f, 0.4f, 0), 0.7f, Color.Gray);
                Raylib.DrawSphere(new Vector3(0, -0.3f, 0.5f), 0.7f, Color.Red);

                Color[] electronColors = { Color.SkyBlue, Color.Lime, Color.Lime }; 

                for (int i = 0; i < electronAngles.Length; i++)
                {

                    Vector3 rotationAxis = new Vector3(0, 1, 0);
                    if (i == 1) rotationAxis = new Vector3(1, 0, 0); 
                    if (i == 2) rotationAxis = new Vector3(0, 0, 1); 
                    
                    Raylib.DrawCircle3D(new Vector3(0, 0, 0), orbitRadius[i], rotationAxis, 90.0f, Color.DarkGray);

                    float angle = electronAngles[i];
                    float x = (float)Math.Cos(angle) * orbitRadius[i]; 
                    float y = 0; 
                    float z = (float)Math.Sin(angle) * orbitRadius[i]; 

                    Vector3 finalElectronPos;


                    if (i == 0) finalElectronPos = new Vector3(x, y, z); 
                    else if (i == 1) finalElectronPos = new Vector3(x, z, y); 
                    else finalElectronPos = new Vector3(y, z, x); 

                    Raylib.DrawSphere(finalElectronPos, 0.3f, electronColors[i]);
                    Raylib.DrawSphereWires(finalElectronPos, 0.4f, 16, 16, electronColors[i]);
                }

                Raylib.DrawGrid(20, 1.0f); 

            Raylib.EndMode3D(); 

            Raylib.DrawText("ATOM Simulation (Bohr Model)", 10, 10, 20, Color.White);
            Raylib.DrawText("Controls: WASD + Mouse", 10, 40, 18, Color.Gray);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow(); 
    }
}
