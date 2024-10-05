using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using LD56.CartridgeManagement;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class LdSession : ISession
{
    private readonly Entity _oreEntity;
    private readonly Entity _workerEntity;
    private List<Entity> _entities = new();

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem)
    {
        _workerEntity = CreateWorker();
        _workerEntity = CreateWorker();
        _workerEntity = CreateWorker();
        _workerEntity = CreateWorker();
        _workerEntity = CreateWorker();
        _workerEntity = CreateWorker();
        _oreEntity = CreateOre();
    }

    public void OnHotReload()
    {
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
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

    private Entity CreateOre()
    {
        var entity = new Entity();
        var spriteRenderer = entity.AddComponent(new SpriteRenderer(entity, Constants.OreSheet));

        entity.Position = new Vector2(500, 500);
        entity.Depth = Depth.Middle + 100;

        _entities.Add(entity);
        return entity;
    }

    private Entity CreateWorker()
    {
        var entity = new Entity();
        var spriteRenderer = entity.AddComponent(new SpriteRenderer(entity, Constants.WorkerBodySheet));
        spriteRenderer.SetAnimation(Constants.WorkerWalkAnimation);
        var workerBehavior = entity.AddComponent(new Worker(entity, spriteRenderer));
        workerBehavior.Velocity = new Vector2(1, 0);

        entity.Position = new Vector2(200, 200);
        
        _entities.Add(entity);
        return entity;
    }
}
