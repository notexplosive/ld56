using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public abstract class Entity : IDrawHook, IUpdateHook
{
    public bool FlaggedForDestroy { get; private set; }
    public Vector2 Position { get; set; }
    public Scale2D Scale { get; set; } = Scale2D.One;
    public Depth Depth { get; set; } = Depth.Middle;

    public abstract void Draw(Painter painter);

    public abstract void Update(float dt);

    public void Destroy()
    {
        FlaggedForDestroy = true;
    }

    public abstract bool EditorHitTest(Vector2 mousePosition);
}
