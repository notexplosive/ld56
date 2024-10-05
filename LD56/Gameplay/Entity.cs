using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public abstract class Entity : IDrawHook, IUpdateHook
{
    public Vector2 Position { get; set; }
    public Scale2D Scale { get; set; } = Scale2D.One; 
    public Depth Depth { get; set; } = Depth.Middle.AsInt;

    public abstract void Draw(Painter painter);

    public abstract void Update(float dt);
}
