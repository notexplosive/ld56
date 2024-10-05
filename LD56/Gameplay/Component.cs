using ExplogineMonoGame;
using ExplogineMonoGame.Rails;

namespace LD56.Gameplay;

public abstract class Component : IDrawHook, IUpdateHook
{
    public abstract void Draw(Painter painter);

    public abstract void Update(float dt);
}
