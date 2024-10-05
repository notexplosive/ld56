using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using LD56.CartridgeManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD56.Gameplay;

public class World
{
    public List<Entity> Entities { get; } = new();
}

public class LdSession : ISession
{
    private readonly Camera _camera;
    private readonly BackgroundDust _dust;
    private readonly World _world = new();
    private readonly Worm _worm;
    private int _buttonInput;

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem)
    {
        _camera = new Camera(runtimeWindow.RenderResolution.ToVector2());
        _worm = new Worm(_world);
        var screenRectangle = runtimeWindow.RenderResolution.ToRectangleF();
        _worm.Position = screenRectangle.Center + new Vector2(0, screenRectangle.Height / 4f);

        _dust = new BackgroundDust(_camera.Size);

        _world.Entities.Add(_worm);

        var food = new Food();
        _world.Entities.Add(food);
    }

    public void OnHotReload()
    {
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        var leftInput = input.Keyboard.GetButton(Keys.Left).IsDown || input.Keyboard.GetButton(Keys.A).IsDown ? -1 : 0;
        var rightInput = input.Keyboard.GetButton(Keys.Right).IsDown || input.Keyboard.GetButton(Keys.D).IsDown ? 1 : 0;
        _buttonInput = leftInput + rightInput;
        _worm.DirectionalInput = _buttonInput;

        if (input.Keyboard.GetButton(Keys.Space).WasPressed)
        {
            _worm.Jet();
        }
    }

    public void Update(float dt)
    {
        foreach (var entity in _world.Entities)
        {
            entity.Update(dt);
        }

        _camera.CenterPosition += (_worm.Position - _camera.CenterPosition) / 2f;
    }

    public void Draw(Painter painter)
    {
        painter.Clear(ColorExtensions.FromRgbHex(0x060608));

        var canvasToScreen = _camera.CanvasToScreen;

        painter.BeginSpriteBatch();

        _dust.Draw(painter, _camera.ViewBounds.Inflated(80, 80));

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(canvasToScreen);

        foreach (var entity in _world.Entities)
        {
            entity.Draw(painter);
        }

        painter.EndSpriteBatch();
    }
}
