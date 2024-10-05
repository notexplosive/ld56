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
    private readonly FrogRenderer _frog;

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem)
    {
        var entity = new Entity();
        var screenRectangle = runtimeWindow.RenderResolution.ToRectangleF();
        entity.Position = screenRectangle.Center + new Vector2(0,screenRectangle.Height / 4f);
        
        
        _frog = entity.AddComponent(new FrogRenderer(entity));

        
        _entities.Add(entity);
    }

    public void OnHotReload()
    {
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (input.Keyboard.GetButton(Keys.Space, true).WasPressed)
        {
            _frog.Jump();
        }
    }

    public void Update(float dt)
    {
        foreach (var entity in _entities)
        {
            entity.Update(dt);
        }
    }

    public void Draw(Painter painter)
    {
        painter.Clear(ColorExtensions.FromRgbHex(0x060608));

        painter.BeginSpriteBatch();

        foreach (var entity in _entities)
        {
            entity.Draw(painter);
        }

        painter.EndSpriteBatch();
    }
}
