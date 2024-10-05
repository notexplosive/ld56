using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using LD56.CartridgeManagement;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class LdSession : ISession
{
    private readonly Entity _entity;

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem)
    {
        _entity = new Entity();
        var spriteRenderer = _entity.AddComponent(new SpriteRenderer(_entity, Constants.WorkerBodySheet));
        spriteRenderer.SetAnimation(Constants.WorkerWalkAnimation);

        _entity.Position = new Vector2(200, 200);
    }

    public void OnHotReload()
    {
        
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        
    }

    public void Update(float dt)
    {
        _entity.Update(dt);
        _entity.Position += new Vector2(5* dt * 60, 0);
    }

    public void Draw(Painter painter)
    {
        painter.Clear(ColorExtensions.FromRgbHex(0x060608));
        
        painter.BeginSpriteBatch();
        
        _entity.Draw(painter);
        
        painter.EndSpriteBatch();
    }
}