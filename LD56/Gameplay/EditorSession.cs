using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using LD56.CartridgeManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using SandboxFM.Save;

namespace LD56.Gameplay;

public class EditorSession : ISession
{
    private readonly Camera _camera;
    private readonly ClientFileSystem _runtimeFileSystem;
    private readonly List<EditorTool> _tools;
    private int _levelIndex;
    private int _toolIndex;
    private readonly World _world = new();

    public EditorSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem)
    {
        _runtimeFileSystem = runtimeFileSystem;
        _camera = new Camera(runtimeWindow.RenderResolution.ToVector2());

        _tools = new List<EditorTool>
        {
            new PlacePlayerTool(),
            new PlaceWallTool(),
            new PlaceFoodTool(),
            new PlaceEnemyTool(),
            new PlaceCurrentTool(0),
            new PlaceCurrentTool(-MathF.PI/2f),
            new PlaceCurrentTool(MathF.PI),
            new PlaceCurrentTool(MathF.PI/2f),
        };

        LoadCurrentLevel();
    }

    public Level CurrentLevel { get; set; } = new();

    public EditorTool CurrentTool => _tools[_toolIndex];

    public void OnHotReload()
    {
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        var cameraHitTest = hitTestStack.AddLayer(_camera.ScreenToCanvas, Depth.Middle);

        var mousePosition = input.Mouse.Position(cameraHitTest.WorldMatrix);
        if (input.Mouse.ScrollDelta() != 0)
        {
            var scrollDelta = input.Mouse.ScrollDelta();
            var scrollMultiplier = 1f;

            if (scrollDelta > 0)
            {
                _camera.ZoomOutFrom((int)(-scrollDelta * scrollMultiplier), mousePosition);
            }

            if (scrollDelta < 0)
            {
                _camera.ZoomInTowards((int)(scrollDelta * scrollMultiplier), mousePosition);
            }

            input.Mouse.ConsumeScrollDelta();
        }

        if (input.Mouse.GetButton(MouseButton.Middle).IsDown)
        {
            _camera.CenterPosition -= input.Mouse.Delta(cameraHitTest.WorldMatrix);
        }

        if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
        {
            CurrentTool.Use(mousePosition, CurrentLevel);
        }

        if (input.Mouse.GetButton(MouseButton.Right).IsDown)
        {
            foreach (var entity in _world.Entities)
            {
                if (entity.EditorHitTest(mousePosition))
                {
                    entity.Destroy();
                    CurrentLevel.DeleteRelatedData(entity);
                    return;
                }
            }
        }

        if (input.Keyboard.GetButton(Keys.A).WasPressed)
        {
            _toolIndex--;
            if (_toolIndex < 0)
            {
                _toolIndex = _tools.Count - 1;
            }
        }

        if (input.Keyboard.GetButton(Keys.D).WasPressed)
        {
            _toolIndex++;
            _toolIndex %= _tools.Count;
        }

        if (input.Keyboard.GetButton(Keys.Q).WasPressed)
        {
            SaveCurrentLevel();
            _levelIndex--;
            if (_levelIndex < 0)
            {
                _levelIndex = 0;
            }

            LoadCurrentLevel();
        }

        if (input.Keyboard.GetButton(Keys.E).WasPressed)
        {
            SaveCurrentLevel();

            _levelIndex++;
            
            LoadCurrentLevel();
        }

        if (input.Keyboard.Modifiers.Control && input.Keyboard.GetButton(Keys.S, true).WasPressed)
        {
            SaveCurrentLevel();
        }

        if (input.Keyboard.GetButton(Keys.F5).WasPressed)
        {
            SaveCurrentLevel();
            RequestPlay?.Invoke();
        }

        if (CurrentTool is PlaceWallTool placeWallTool)
        {
            var delta = 10;
            if (input.Keyboard.Modifiers.Shift)
            {
                delta *= 10;
            }
            
            if (input.Keyboard.GetButton(Keys.OemPlus).WasPressed)
            {
                placeWallTool.Radius += delta;
            }

            if (input.Keyboard.GetButton(Keys.OemMinus).WasPressed)
            {
                placeWallTool.Radius -= delta;
            }
        }
    }

    public void Update(float dt)
    {
    }

    public void Draw(Painter painter)
    {
        painter.BeginSpriteBatch(_camera.CanvasToScreen);

        _world.LoadLevel(CurrentLevel);

        foreach (var entity in _world.Entities)
        {
            entity.Draw(painter);
            entity.Update(1/60f);
        }

        painter.DrawLineRectangle(new RectangleF(0, 0, 1920, 1080), new LineDrawSettings {Thickness = 5});

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch();

        painter.DrawStringWithinRectangle(Client.Assets.GetFont("fishbone/font", 25),
            _levelIndex + "\n" + CurrentTool.DebugInfo(),
            _camera.OutputResolution.ToRectangleF(), Alignment.BottomLeft, new DrawSettings());
        painter.EndSpriteBatch();
    }

    private void LoadCurrentLevel()
    {
        CurrentLevel = JsonConvert.DeserializeObject<Level>(Client.Debug.RepoFileSystem.ReadFile(LevelName())) ??
                       new Level();
    }

    private void SaveCurrentLevel()
    {
        var levelSerialized = JsonConvert.SerializeObject(CurrentLevel, Formatting.Indented);
        Client.Debug.RepoFileSystem.WriteToFile(LevelName(), levelSerialized);
    }

    private string LevelName()
    {
        return $"Levels/level{_levelIndex}.json";
    }

    public event Action? RequestPlay;
}

public class PlaceFoodTool : EditorTool
{
    public override void Use(Vector2 mousePosition, Level level)
    {
        level.Foods.Add(new SerializableVector2(mousePosition));
    }
}

public class PlaceEnemyTool : EditorTool
{
    public override void Use(Vector2 mousePosition, Level level)
    {
        level.Enemies.Add(new SerializableVector2(mousePosition));
    }
}

public class PlaceCurrentTool : EditorTool
{
    public PlaceCurrentTool(float angle)
    {
        Angle = angle;
    }
    
    public override void Use(Vector2 mousePosition, Level level)
    {
        level.Currents.Add(new CurrentData
        {
            Position = new SerializableVector2(mousePosition),
            Radius = Radius,
            Angle = Angle
        });
    }
    
    public float Radius { get; set; } = 500f;
    public float Angle { get; set; }

    public override string DebugInfo()
    {
        return base.DebugInfo() + " " + Radius +  " radius, " + Angle + " degrees";
    }
}

[Serializable]
public class Level
{
    [JsonProperty("goalSpawn")]
    public SerializableVector2 GoalSpawnPosition { get; set; } = new();

    [JsonProperty("obstacles")]
    public List<WallData> Obstacles { get; set; } = new();
    
    [JsonProperty("current")]
    public List<CurrentData> Currents { get; set; } = new();

    [JsonProperty("coins")]
    public List<SerializableVector2> Foods { get; set; } = new();

    [JsonProperty("enemies")]
    public List<SerializableVector2> Enemies { get; set; } = new();

    public void DeleteRelatedData(Entity entity)
    {
        if (entity is Obstacle)
        {
            Obstacles.RemoveAll(wall => wall.Position.ToVector2() == entity.Position);
        }

        if (entity is Food)
        {
            Foods.RemoveAll(coinPosition => coinPosition.ToVector2() == entity.Position);
        }

        if (entity is Enemy)
        {
            Enemies.RemoveAll(enemyPosition => enemyPosition.ToVector2() == entity.Position);
        }

        if (entity is Current)
        {
            Currents.RemoveAll(current => current.Position.ToVector2() == entity.Position);
        }
    }
}

[Serializable]
public class CurrentData
{
    [JsonProperty("position")]
    public SerializableVector2 Position { get; set; } = new();

    [JsonProperty("radius")]
    public float Radius { get; set; } = 100;
    
    [JsonProperty("angle")]
    public float Angle { get; set; }
}

[Serializable]
public class WallData
{
    [JsonProperty("position")]
    public SerializableVector2 Position { get; set; } = new();

    [JsonProperty("radius")]
    public float Radius { get; set; } = 100;
}

public class PlacePlayerTool : EditorTool
{
    public override void Use(Vector2 mousePosition, Level level)
    {
        level.GoalSpawnPosition = mousePosition;
    }
}

public class PlaceWallTool : EditorTool
{
    public float Radius { get; set; } = 50f;

    public override void Use(Vector2 mousePosition, Level level)
    {
        level.Obstacles.Add(new WallData
        {
            Position = new SerializableVector2(mousePosition),
            Radius = Radius
        });
    }

    public override string DebugInfo()
    {
        return base.DebugInfo() + " " + Radius;
    }
}

public abstract class EditorTool
{
    public abstract void Use(Vector2 mousePosition, Level level);

    public virtual string DebugInfo()
    {
        return GetType().Name;
    }
}
