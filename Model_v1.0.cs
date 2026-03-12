using Raylib_cs;
using System; // Нужно для Math.Sin и Math.Cos
using System.Numerics;

class Program
{
    static void Main()
    {
        // 1. Инициализация
        Raylib.InitWindow(1000, 800, "3D Симуляция Атома (Модель Бора)");
        Raylib.SetTargetFPS(60);

        // Скрываем курсор, чтобы управлять камерой
        Raylib.DisableCursor(); 

        // 2. Настройка Камеры (используем "ультимативный" конструктор)
        Camera3D camera = new Camera3D(
            new Vector3(12, 12, 12),     // Позиция камеры (откуда смотрим)
            new Vector3(0, 0, 0),       // Цель (куда смотрим, в центр)
            new Vector3(0, 1, 0),       // "Вверх" (ось Y)
            45.0f,                      // Угол обзора
            CameraProjection.Perspective // Перспектива
        );

        // 3. Данные для физики вращения
        // Нам нужно знать радиусы орбит и скорости
        float[] electronAngles = { 0.0f, 1.5f, 3.0f }; // Начальные углы для 3-х электронов
        float[] orbitRadius = { 4.0f, 6.0f, 6.0f };     // Радиусы их орбит
        float[] orbitSpeeds = { 2.0f, 1.2f, 1.2f };     // Скорости вращения (чем дальше, тем медленнее)

        // Основной цикл
        while (!Raylib.WindowShouldClose())
        {
            // 4. Логика (Физика и Управление)
            float dt = Raylib.GetFrameTime(); // Получаем время кадра

            // Позволяем камере летать (WASD + Мышь)
            Raylib.UpdateCamera(ref camera, CameraMode.Free); 

            // Обновляем углы электронов на основе времени
            for (int i = 0; i < electronAngles.Length; i++)
            {
                electronAngles[i] += orbitSpeeds[i] * dt; // Угол = Скорость * Время
            }

            // 5. Отрисовка
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black); // Атом в космосе (черный фон)

            Raylib.BeginMode3D(camera); // ВКЛЮЧАЕМ 3D

                // --- РИСУЕМ ЯДРО ---
                // Создадим ядро из нескольких плотных сфер (имитация протонов и нейтронов)
                Raylib.DrawSphere(new Vector3(0, 0, 0), 0.8f, Color.Red);   // Центральный протон
                Raylib.DrawSphere(new Vector3(0.5f, 0, 0), 0.7f, Color.Gray); // Нейтрон рядом
                Raylib.DrawSphere(new Vector3(-0.2f, 0.4f, 0), 0.7f, Color.Gray);
                Raylib.DrawSphere(new Vector3(0, -0.3f, 0.5f), 0.7f, Color.Red);

                // --- РИСУЕМ ОРБИТЫ И ЭЛЕКТРОНЫ ---
                Color[] electronColors = { Color.SkyBlue, Color.Lime, Color.Lime }; // Цвета

                for (int i = 0; i < electronAngles.Length; i++)
                {
                    // РИСУЕМ ОРБИТАЛЬНОЕ КОЛЬЦО
                    // Чтобы нарисовать кольцо, нужно повернуть его вокруг оси
                    // Мы наклоним 2 и 3 орбиты для 3D эффекта
                    Vector3 rotationAxis = new Vector3(0, 1, 0); // Орбита по умолчанию
                    if (i == 1) rotationAxis = new Vector3(1, 0, 0); // Повернули вокруг X
                    if (i == 2) rotationAxis = new Vector3(0, 0, 1); // Повернули вокруг Z
                    
                    Raylib.DrawCircle3D(new Vector3(0, 0, 0), orbitRadius[i], rotationAxis, 90.0f, Color.DarkGray);

                    // РАССЧИТЫВАЕМ ПОЗИЦИЮ ЭЛЕКТРОНА ПО ФОРМУЛЕ
                    float angle = electronAngles[i];
                    float x = (float)Math.Cos(angle) * orbitRadius[i]; // X = R * Cos(угол)
                    float y = 0; // По умолчанию он лежит в плоскости XZ
                    float z = (float)Math.Sin(angle) * orbitRadius[i]; // Z = R * Sin(угол)

                    Vector3 finalElectronPos;

                    // ПРИМЕНЯЕМ НАКЛОН ОРБИТЫ К ЭЛЕКТРОНУ
                    if (i == 0) finalElectronPos = new Vector3(x, y, z); // Первая (горизонтальная)
                    else if (i == 1) finalElectronPos = new Vector3(x, z, y); // Вторая (вертикальная вдоль X)
                    else finalElectronPos = new Vector3(y, z, x); // Третья (вертикальная вдоль Z)

                    // РИСУЕМ ЭЛЕКТРОН
                    Raylib.DrawSphere(finalElectronPos, 0.3f, electronColors[i]);
                    // Добавим свечение (Trail effect)
                    Raylib.DrawSphereWires(finalElectronPos, 0.4f, 16, 16, electronColors[i]);
                }

                Raylib.DrawGrid(20, 1.0f); // Сетка для масштаба

            Raylib.EndMode3D(); // ВЫКЛЮЧАЕМ 3D

            // Текст (учитывая твою проблему с кодировкой, пишем на английском)
            Raylib.DrawText("ATOM Simulation (Bohr Model)", 10, 10, 20, Color.White);
            Raylib.DrawText("Controls: WASD + Mouse", 10, 40, 18, Color.Gray);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow(); // Закрываем
    }
}
