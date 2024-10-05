using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class Entity : IDrawHook, IUpdateHook
{
    private readonly List<Component> _components = new();
    public Vector2 Position { get; set; }
    public Scale2D Scale { get; set; } = Scale2D.One; 
    public Depth Depth { get; set; } = Depth.Middle.AsInt;

    public T AddComponent<T>(T component) where T : Component
    {
        _components.Add(component);
        return component;
    }
    
    public void Draw(Painter painter)
    {
        foreach (var component in _components)
        {
            component.Draw(painter);
        }
    }

    public void Update(float dt)
    {
        foreach (var component in _components)
        {
            component.Update(dt);
        }
    }
}
