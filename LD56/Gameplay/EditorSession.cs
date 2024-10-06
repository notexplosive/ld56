using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly ClientFileSystem _runtimeFileSystem;
    private readonly Camera _camera;
    private readonly List<EditorTool> _tools;
    public Level CurrentLevel { get; } = new();
    private int _toolIndex;
    private World _world = new();

    public EditorSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem)
    {
        _runtimeFileSystem = runtimeFileSystem;
        _camera = new Camera(runtimeWindow.RenderResolution.ToVector2());

        _tools = new List<EditorTool>();
        _tools.Add(new PlacePlayerTool());
        _tools.Add(new PlaceWallTool());
        _tools.Add(new PlaceFoodTool());

        _toolIndex = 0;

        CurrentLevel = JsonConvert.DeserializeObject<Level>(Client.Debug.RepoFileSystem.ReadFile("level.json"))?? new Level();
    }

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

            if (scrollDelta > 0)
            {
                _camera.ZoomOutFrom(-scrollDelta / 10, mousePosition);
            }

            if (scrollDelta < 0)
            {
                _camera.ZoomInTowards(scrollDelta / 10, mousePosition);
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

        if (input.Keyboard.Modifiers.Control && input.Keyboard.GetButton(Keys.S, true).WasPressed)
        {
            var levelSerialized = JsonConvert.SerializeObject(CurrentLevel, Formatting.Indented);
            Client.Debug.RepoFileSystem.WriteToFile("level.json",levelSerialized);
        }

        if (input.Keyboard.GetButton(Keys.F5).WasPressed)
        {
            RequestPlay?.Invoke();
        }

        if (CurrentTool is PlaceWallTool placeWallTool)
        {
            if (input.Keyboard.GetButton(Keys.OemPlus).WasPressed)
            {
                placeWallTool.Radius += 10;
            }
            
            if (input.Keyboard.GetButton(Keys.OemMinus).WasPressed)
            {
                placeWallTool.Radius -= 10;
            }
        }
    }

    public event Action? RequestPlay;

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
            entity.Update(1f);
        }
        
        painter.DrawLineRectangle(new RectangleF(0, 0, 1920, 1080), new LineDrawSettings {Thickness = 5});

        
        painter.EndSpriteBatch();
        
        painter.BeginSpriteBatch();

        painter.DrawStringWithinRectangle(Client.Assets.GetFont("engine/console-font", 25), CurrentTool.DebugInfo(),
            _camera.OutputResolution.ToRectangleF(), Alignment.BottomLeft, new DrawSettings());
        painter.EndSpriteBatch();
    }
}

public class PlaceFoodTool : EditorTool
{
    public override void Use(Vector2 mousePosition, Level level)
    {
        level.Coins.Add(new SerializableVector2(mousePosition));
    }
}

[Serializable]
public class Level
{
    [JsonProperty("goalSpawn")]
    public SerializableVector2 GoalSpawnPosition { get; set; } = new();

    [JsonProperty("obstacles")]
    public List<WallData> Walls { get; set; } = new();

    [JsonProperty("coins")]
    public List<SerializableVector2> Coins { get; set; } = new();

    public void DeleteRelatedData(Entity entity)
    {
        if (entity is Obstacle)
        {
            Walls.RemoveAll(wall => wall.Position.ToVector2() == entity.Position);
        }

        if (entity is Food)
        {
            Coins.RemoveAll(coinPosition => coinPosition.ToVector2() == entity.Position);
        }
    }
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
        level.Walls.Add(new WallData
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
