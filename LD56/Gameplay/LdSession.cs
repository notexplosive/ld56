using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using LD56.CartridgeManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD56.Gameplay;

public class LdSession : ISession
{
    private readonly List<Entity> _entities = new();
    private readonly WormBehavior _wormBehavior;
    private int _buttonInput;
    private readonly Camera _camera;
    private readonly Entity _worm;
    private readonly BackgroundDust _dust;

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem)
    {
        _camera = new Camera(runtimeWindow.RenderResolution.ToVector2());
        _worm = new Entity();
        var screenRectangle = runtimeWindow.RenderResolution.ToRectangleF();
        _worm.Position = screenRectangle.Center + new Vector2(0, screenRectangle.Height / 4f);
        var wormRenderer = _worm.AddComponent(new WormRenderer(_worm));
        _wormBehavior = _worm.AddComponent(new WormBehavior(_worm, wormRenderer));

        _dust = new BackgroundDust(_camera.Size);

        _entities.Add(_worm);
    }

    public void OnHotReload()
    {
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        var leftInput = input.Keyboard.GetButton(Keys.Left).IsDown || input.Keyboard.GetButton(Keys.A).IsDown ? -1 : 0;
        var rightInput = input.Keyboard.GetButton(Keys.Right).IsDown || input.Keyboard.GetButton(Keys.D).IsDown ? 1 : 0;
        _buttonInput = leftInput + rightInput;
        _wormBehavior.DirectionalInput = _buttonInput;

        if (input.Keyboard.GetButton(Keys.Space).WasPressed)
        {
            _wormBehavior.Jet();
        }
    }

    public void Update(float dt)
    {
        foreach (var entity in _entities)
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

        foreach (var entity in _entities)
        {
            entity.Draw(painter);
        }

        painter.EndSpriteBatch();
    }
}