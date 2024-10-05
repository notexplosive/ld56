using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class Worker : Component
{
    private WorkerState _state;
    private readonly Entity _entity;

    public Worker(Entity entity, SpriteRenderer spriteSpriteRenderer)
    {
        _entity = entity;
        SpriteRenderer = spriteSpriteRenderer;
        _state = new WalkingToDestinationState(this, _entity, new Vector2(500, 500));
    }

    public SpriteRenderer SpriteRenderer { get; }

    public float Speed { get; set; } = 3f;
    public Vector2 Velocity { get; set; }

    public override void Draw(Painter painter)
    {
    }

    public override void Update(float dt)
    {
        _state.Update(dt);
        if (_state.IsFinished)
        {
            _state = DetermineNewState();
        }

        if (Velocity.X < 0)
        {
            SpriteRenderer.FlipX = true;
        }

        if (Velocity.X > 0)
        {
            SpriteRenderer.FlipX = false;
        }
        
        _entity.Position += Velocity * dt * 60f * Speed;
    }

    private WorkerState DetermineNewState()
    {
        return new WanderState(this,_entity);
    }
}