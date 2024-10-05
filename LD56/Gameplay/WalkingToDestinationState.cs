using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class WalkingToDestinationState : WorkerState
{
    private readonly Vector2 _destination;
    private readonly Entity _entity;
    private readonly Worker _worker;
    private bool _hasArrived;

    public WalkingToDestinationState(Worker worker, Entity entity, Vector2 vector2)
    {
        _worker = worker;
        _entity = entity;
        _destination = vector2;
    }

    public override void Update(float dt)
    {
        if (!_hasArrived)
        {
            var displacementToDestination = _destination - _entity.Position;

            if (displacementToDestination.Length() < 1)
            {
                _hasArrived = true;
            }
            else
            {
                _worker.Velocity = displacementToDestination.Normalized();
                _worker.SpriteRenderer.SetAnimation(Constants.WorkerWalkAnimation);
            }
        }

        if (_hasArrived)
        {
            _worker.Velocity = Vector2.Zero;
            MarkAsFinished();
        }
    }
}
